using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.AsyncEvents;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nomia.Entities;
using Nomia.EventArgs;
using Nomia.EventArgs.Player;
using Nomia.Websocket;
using Nomia.Websocket.EventArgs;

namespace Nomia
{
    public class NomiaEndpoint
    {
        /// <summary>
        /// Gets or sets the host of the endpoint.
        /// </summary>
        public string Host { get; }

        /// <summary>
        /// Gets or sets the port of the endpoint.
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// Gets or sets the route of the endpoint.
        /// </summary>
        public string Route { get; }

        /// <summary>
        /// Gets or sets whether the endpoint is secure.
        /// </summary>
        public bool IsSecure { get; }

        /// <summary>
        /// Gets or sets the password of the endpoint.
        /// </summary>
        public string Password { get; }


        public NomiaEndpoint(string host, int port, string password, string route = "/", bool isSecure = false)
        {
            if (string.IsNullOrWhiteSpace(host)) throw new ArgumentNullException(nameof(host));
            if (port < 1 || port > 65535) throw new ArgumentOutOfRangeException(nameof(port));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentNullException(nameof(password));
            if (string.IsNullOrWhiteSpace(route)) throw new ArgumentNullException(nameof(route));

            Host = host;
            Port = port;
            Password = password;
            Route = route;
            IsSecure = isSecure;
        }

        public Uri ToUri() => new Uri($"http{(IsSecure ? "S" : string.Empty)}://{Host}:{Port}{Route}");
        public override string ToString() => $"http{(IsSecure ? "S" : string.Empty)}://{Host}:{Port}{Route}";

        public string ToWebSocketString() => $"ws{(IsSecure ? "s" : string.Empty)}://{Host}:{Port}{Route}";
    }

    public sealed class NomiaNode : IDisposable
    {
        /// <summary>
        /// Gets the rest endpoint.
        /// </summary>
        public NomiaEndpoint RestEndpoint { get; }

        /// <summary>
        /// Gets the websocket endpoint.
        /// </summary>
        public NomiaEndpoint WebSocketEndpoint { get; }

        private readonly NomiaWebsocket _websocket;

        /// <summary>
        /// Gets or sets the name of the node.
        /// </summary>
        public string NodeName { get; set; }

        /// <summary>
        /// Fired when the node is connected, but not ready.
        /// </summary>
        public event AsyncEventHandler<NomiaNode, LavalinkNodeConnectedEventArgs> OnConnected
        {
            add => _onConnected.Register(value);
            remove => _onConnected.Unregister(value);
        }

        /// <summary>
        /// Fired when an exception occurs.
        /// </summary>
        public event AsyncEventHandler<NomiaNode, LavalinkClientExceptionEventArgs> OnException
        {
            add => _onException.Register(value);
            remove => _onException.Unregister(value);
        }
        
        /// <summary>
        /// Fired when the node is ready.
        /// </summary>
        public event AsyncEventHandler<NomiaNode, LavalinkNodeReadyEventArgs> OnReady
        {
            add => _onReady.Register(value);
            remove => _onReady.Unregister(value);
        }
        
        /// <summary>
        /// Fired when the node is disconnected.
        /// </summary>
        public event AsyncEventHandler<NomiaNode, LavalinkNodeDisconnectedEventArgs> OnDisconnected
        {
            add => _onDisconnected.Register(value);
            remove => _onDisconnected.Unregister(value);
        }
        
        /// <summary>
        /// Gets the session id of the node.
        /// </summary>
        public string SessionId { get; private set; }
        
        /// <summary>
        /// Whether the node is ready.
        /// </summary>
        public bool IsReady { get; set; }
        
        /// <summary>
        /// Stats of the node.
        /// </summary>
        public LavalinkStats Stats { get; set; }
        
        /// <summary>
        /// Gets the discord client.
        /// </summary>
        internal DiscordClient Discord { get; set; }
        
        /// <summary>
        /// Resume key of the node.
        /// </summary>
        public string ResumeKey { get; set; }
        
        internal NomiaNodeRest Rest { get; }

        private readonly AsyncEvent<NomiaNode, LavalinkClientExceptionEventArgs> _onException;

        private readonly AsyncEvent<NomiaNode, LavalinkNodeConnectedEventArgs> _onConnected;
        private readonly AsyncEvent<NomiaNode, LavalinkNodeDisconnectedEventArgs> _onDisconnected;
        
        private readonly AsyncEvent<NomiaNode, LavalinkNodeReadyEventArgs> _onReady;

        private Dictionary<ulong, TaskCompletionSource<VoiceServerUpdateEventArgs>> _voiceServerUpdateTasks =
            new();
        private Dictionary<ulong, TaskCompletionSource<VoiceStateUpdateEventArgs>> _voiceStateUpdateTasks =
            new();

        internal Dictionary<ulong, LavalinkGuildConnection> Connections { get; } = new();
        private bool _previouslyConnected = false;
        private int _reconnectAttempts = 0;

        public NomiaNode(NomiaEndpoint restEndpoint, NomiaEndpoint webSocketEndpoint,
            string nodeName = "Nomia Node", string resumeKey = null)
        {
            RestEndpoint = restEndpoint ?? throw new ArgumentNullException(nameof(restEndpoint));
            WebSocketEndpoint = webSocketEndpoint ?? throw new ArgumentNullException(nameof(webSocketEndpoint));
            
            _websocket = new NomiaWebsocket(WebSocketEndpoint.ToWebSocketString());
            NodeName = nodeName;
            ResumeKey = resumeKey;
            Rest = new NomiaNodeRest(this);

            _onConnected = new AsyncEvent<NomiaNode, LavalinkNodeConnectedEventArgs>("LAVALINK_NODE_CONNECTED", InternalHandleException);
            _onDisconnected = new AsyncEvent<NomiaNode, LavalinkNodeDisconnectedEventArgs>("LAVALINK_NODE_DISCONNECTED", InternalHandleException);
            _onException = new AsyncEvent<NomiaNode, LavalinkClientExceptionEventArgs>("LAVALINK_NODE_EXCEPTION", InternalHandleException);
            _onReady = new AsyncEvent<NomiaNode, LavalinkNodeReadyEventArgs>("LAVALINK_NODE_READY", InternalHandleException);
            

            _websocket.AddHeader("Authorization", WebSocketEndpoint.Password);
            _websocket.AddHeader("Client-Name", "DHCPCD9/Nomia");
            
            if (ResumeKey != null)
            {
                _websocket.AddHeader("Resume-Key", ResumeKey);
            }
            
            _websocket.OnConnected += websocketOnOnConnected;
            _websocket.OnDisconnected += websocketOnOnDisconnected;
            
            _websocket.RegisterOp<LavalinkNodeReadyPayload>("ready", Websocket_NodeReady);
            _websocket.RegisterOp("event", Websocket_LavalinkEvent);
            _websocket.RegisterOp("playerUpdate", Websocket_PlayerUpdate);
            _websocket.RegisterOp<LavalinkStats>("stats", Websocket_Stats);

        }

        private async Task websocketOnOnDisconnected(NomiaWebsocket sender, WebsocketDisconnectedEventArgs e)
        {
            Discord.Logger.LogWarning(
                "Node {NodeName} disconnected, reconnecting in 5 seconds", NodeName);
            await _onDisconnected.InvokeAsync(this, new LavalinkNodeDisconnectedEventArgs());
        }

 

        private async Task Websocket_Stats(LavalinkStats stats)
        {
            Stats = stats;
        }

        private async Task Websocket_PlayerUpdate(object arg)
        {
            var jObject = JObject.Parse(JsonConvert.SerializeObject(arg));
            
            var guildId = jObject["guildId"].Value<ulong>();
            
            if (!Connections.TryGetValue(guildId, out var connection))
            {
                return;
            }
            
            var state = jObject["state"].ToObject<LavalinkPlayerState>();
            
            connection.State = state;
        }

        private async Task Websocket_LavalinkEvent(object arg)
        {
            var jObject = JObject.Parse(JsonConvert.SerializeObject(arg));
            
            var guildId = jObject["guildId"].Value<ulong>();
            
            if (!Connections.TryGetValue(guildId, out var connection))
            {
                throw new InvalidOperationException("Connection not found");
            }
            
            var typeRaw = jObject["type"].Value<string>();
            Enum.TryParse(typeRaw, true, out LavalinkEventType type);

            if (type == LavalinkEventType.TrackStartEvent)
            {
                var track = jObject.GetValue("track")!.ToObject<LavalinkTrack>();
                
                connection?._onTrackStart.InvokeAsync(connection, new PlaybackStartedEventArgs(track));
            }
            
            if (type == LavalinkEventType.TrackEndEvent)
            {
                var track = jObject.GetValue("track")!.ToObject<LavalinkTrack>();
                var reasonRaw = jObject["reason"]!.Value<string>();
                Enum.TryParse(reasonRaw, true, out LavalinkTrackEndReason reason);
                
                connection?._onTrackFinish.InvokeAsync(connection, new PlaybackFinishedEventArgs(track, reason));
            }
            
            if (type == LavalinkEventType.TrackExceptionEvent)
            {
                var track = jObject.GetValue("track")!.ToObject<LavalinkTrack>();
                var error = jObject["exception"].Value<LavalinkException>();
                
                connection?._onTrackException.InvokeAsync(connection, new PlaybackExceptionEventArgs(track, error));
            }
            
            if (type == LavalinkEventType.TrackStuckEvent)
            {
                var track = jObject.GetValue("track")!.ToObject<LavalinkTrack>();
                var threshold = jObject["thresholdMs"].Value<int>();
                
                connection?._onTrackStuck.InvokeAsync(connection, new PlaybackStuckEventArgs(track, threshold));
            }
            
            if (type == LavalinkEventType.WebSocketClosedEvent)
            {
                var code = jObject.GetValue("code")!.Value<int>();
                var reason = jObject.GetValue("reason")!.Value<string>();
                var byRemote = jObject.GetValue("byRemote")!.ToObject<bool>();
                
                connection?._onPlayerWebsocketClosed.InvokeAsync(connection, new PlayerWebsocketClosedEventArgs(code, reason, byRemote));
            }
        }

        /// <summary>
        /// No way...
        /// </summary>
        /// <param name="payload">Payload to send.</param>
        internal void DiscordWsSendAsync(string payload)
        { 
            var method = Discord.GetType().GetMethod("SendRawPayloadAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            
            method.Invoke(Discord, new object[] {payload});
        }

        public async Task<LavalinkGuildConnection> ConnectAsync(DiscordChannel channel)
        {
            if (channel.Type != ChannelType.Voice && channel.Type != ChannelType.Stage) throw new ArgumentException("Channel must be a voice channel.", nameof(channel));
         
            if (Connections.TryGetValue(channel.GuildId.Value, out var connectionCached))
            {
                if (connectionCached.Channel is null)
                {
                    //Deleting the connection because it's not connected to a channel.
                    Connections.Remove(channel.GuildId.Value);
                }
                if (connectionCached.Channel.Id == channel.Id) return connectionCached;

                return null;
            }
            
            var vstu = new TaskCompletionSource<VoiceStateUpdateEventArgs>();
            var vsru = new TaskCompletionSource<VoiceServerUpdateEventArgs>();
            
            _voiceStateUpdateTasks[channel.GuildId.Value] = vstu;
            _voiceServerUpdateTasks[channel.GuildId.Value] = vsru;

            var vsd = new VoiceDispatch
            {
                OpCode = 4,
                Payload = new VoiceStateUpdatePayload
                {
                    GuildId = channel.GuildId.Value,
                    ChannelId = channel.Id,
                    Deafened = false,
                    Muted = false,
                }
            };
            
            var vsj = JsonConvert.SerializeObject(vsd);
            
            
            DiscordWsSendAsync(vsj);
            
            if (!vstu.Task.Wait(TimeSpan.FromSeconds(10)) && !vsru.Task.Wait(TimeSpan.FromSeconds(10)))
            {
                throw new TimeoutException("Voice state update timed out.");
            }
            
            var vsu = await vstu.Task.ConfigureAwait(false);
            var vsr = await vsru.Task.ConfigureAwait(false);
            
            var connection = new LavalinkGuildConnection(this, vsr, vsu);
            
            //remove tasks
            _voiceStateUpdateTasks.Remove(channel.GuildId.Value);
            _voiceServerUpdateTasks.Remove(channel.GuildId.Value);

            var sessionId = vsu.SessionId;
            var endpoint = vsr.Endpoint;
            var token = vsr.VoiceToken;

            //Creating player
            await Rest.UpdatePlayer(channel.GuildId.Value, new LavalinkPlayerUpdatePayload
            {
                VoiceState = new LavalinkVoiceState
                {
                    SessionId = sessionId,
                    Token = token,
                    Endpoint = endpoint,
                }
            });
            
            
            Connections[channel.GuildId.Value] = connection;
            
            return connection;
        }

        private async Task Discord_ClientOnVoiceStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs e)
        {
            if (e.Guild is null) return;
            
            if (e.User.Id != Discord.CurrentUser.Id) return;
            
            if (_voiceStateUpdateTasks.TryGetValue(e.Guild.Id, out var task))
            {
                task.SetResult(e);
            }
        }

        private Task Discord_ClientOnVoiceServerUpdated(DiscordClient sender, VoiceServerUpdateEventArgs e)
        {
            if (e.Guild is null) return Task.CompletedTask;
            
            if (_voiceServerUpdateTasks.TryGetValue(e.Guild.Id, out var task))
            {
                task.SetResult(e);
            }

            return Task.CompletedTask;
        }

        private async Task Websocket_NodeReady(LavalinkNodeReadyPayload payload)
        {
            SessionId = payload.SessionId;
            IsReady = true;
            await _onReady.InvokeAsync(this, new LavalinkNodeReadyEventArgs(payload.Resumed, payload.SessionId));
            
            Discord.Logger.LogInformation(NomiaEvents.Lavalink, "Node {NodeName} is ready ({ClientShardId})", NodeName, Discord.ShardId);
        }

        private Task websocketOnOnConnected(NomiaWebsocket sender, WebsocketConnectedEventArgs e)
        {
            _onConnected.InvokeAsync(this, new LavalinkNodeConnectedEventArgs(this));
            return Task.CompletedTask;
        }

        private void InternalHandleException<TArgs>(AsyncEvent<NomiaNode, TArgs> asyncevent, Exception exception,
            AsyncEventHandler<NomiaNode, TArgs> handler, NomiaNode sender, TArgs eventargs) where TArgs : AsyncEventArgs
        {
            _onException.InvokeAsync(this, new LavalinkClientExceptionEventArgs(exception));
        }


        internal async Task ConnectNodeAsync(DiscordClient client)
        {
            
            if (IsReady) return;
            if (client is null) throw new ArgumentNullException(nameof(client));
            if (client.CurrentUser is null) throw new InvalidOperationException("Client is not ready yet.");
            
            Discord = client;
                        
            if (!_websocket.Headers.ContainsKey("Num-Shards"))
                _websocket.AddHeader("Num-Shards", Discord.ShardCount.ToString());
           
            if (!_websocket.Headers.ContainsKey("User-Id"))
                _websocket.AddHeader("User-Id", Discord.CurrentUser.Id.ToString());
            Discord.VoiceServerUpdated += Discord_ClientOnVoiceServerUpdated;
            Discord.VoiceStateUpdated += Discord_ClientOnVoiceStateUpdated;
            
            if (_websocket is not null && _websocket.IsConnected) return;
            //Running connection in background to prevent blocking.
            await Task.Run(async () =>
            {
                try
                {
                    await _websocket.ConnectAsync();
                }
                catch
                {
                    Discord.Logger.LogError(NomiaEvents.Lavalink, "Failed to connect to node {NodeName} ({ClientShardId})", NodeName, Discord.ShardId);
                }
            });
        }

        public override string ToString() => $"{NodeName} ({RestEndpoint})";

        public string ToWebSocketString() => $"{NodeName} ({WebSocketEndpoint.ToWebSocketString()})";

        public async Task DestroyPlayer(DiscordChannel voiceChannel)
        {
            if (voiceChannel.Type != ChannelType.Voice && voiceChannel.Type != ChannelType.Stage) throw new ArgumentException("Channel must be a voice channel.", nameof(voiceChannel));
            
            if (!Connections.TryGetValue(voiceChannel.GuildId.Value, out var connection)) return;
            
            await connection.DisconnectAsync();
        }

        public async Task<LavalinkLoadable> LoadTrackAsync(string query, LavalinkSearchType searchType = LavalinkSearchType.Youtube)
        {
            var prefix = searchType switch
            {
                LavalinkSearchType.Youtube => "ytsearch:",
                LavalinkSearchType.Soundcloud => "scsearch:",
                LavalinkSearchType.Raw => "",
                _ => throw new ArgumentOutOfRangeException(nameof(searchType), searchType, null)
            };
            var queryRaw = $"{prefix}{query}";
            return await Rest.ResolveTracks(queryRaw);;
        }

        public void Disconnect()
        {
            _websocket.Dispose();
        }

        public void Dispose()
        {
            _websocket?.Dispose();
        }
    }
}