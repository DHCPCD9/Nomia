using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Emzi0767.Utilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Nomia.Entities;
using Nomia.EventArgs;
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

    public sealed class NomiaNode
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
        
        internal NomiaNodeRest Rest { get; }

        private readonly AsyncEvent<NomiaNode, LavalinkClientExceptionEventArgs> _onException;

        private readonly AsyncEvent<NomiaNode, LavalinkNodeConnectedEventArgs> _onConnected;
        private readonly AsyncEvent<NomiaNode, LavalinkNodeDisconnectedEventArgs> _onDisconnected;
        
        private readonly AsyncEvent<NomiaNode, LavalinkNodeReadyEventArgs> _onReady;
        
        public DiscordClient Discord { get; }
        
        private Dictionary<ulong, TaskCompletionSource<VoiceServerUpdateEventArgs>> _voiceServerUpdateTasks =
            new();
        private Dictionary<ulong, TaskCompletionSource<VoiceStateUpdateEventArgs>> _voiceStateUpdateTasks =
            new();

        internal Dictionary<ulong, LavalinkGuildConnection> Connections { get; } = new();

        public NomiaNode(DiscordClient discord, NomiaEndpoint restEndpoint, NomiaEndpoint webSocketEndpoint,
            string nodeName = "Nomia Node")
        {
            if (discord is null) throw new ArgumentNullException(nameof(discord));
            RestEndpoint = restEndpoint ?? throw new ArgumentNullException(nameof(restEndpoint));
            WebSocketEndpoint = webSocketEndpoint ?? throw new ArgumentNullException(nameof(webSocketEndpoint));
            
            Discord = discord;
            _websocket = new NomiaWebsocket(WebSocketEndpoint.ToWebSocketString());
            NodeName = nodeName;
            Rest = new NomiaNodeRest(this);

            _onConnected = new AsyncEvent<NomiaNode, LavalinkNodeConnectedEventArgs>("LAVALINK_NODE_CONNECTED",
                TimeSpan.Zero, InternalHandleException);
            _onDisconnected = new AsyncEvent<NomiaNode, LavalinkNodeDisconnectedEventArgs>("LAVALINK_NODE_DISCONNECTED",
                TimeSpan.Zero, InternalHandleException);
            _onException = new AsyncEvent<NomiaNode, LavalinkClientExceptionEventArgs>("LAVALINK_NODE_EXCEPTION",
                TimeSpan.Zero, InternalHandleException);
            _onReady = new AsyncEvent<NomiaNode, LavalinkNodeReadyEventArgs>("LAVALINK_NODE_READY",
                TimeSpan.Zero, InternalHandleException);
            

            _websocket.AddHeader("Authorization", WebSocketEndpoint.Password);
            _websocket.AddHeader("Num-Shards", discord.ShardCount.ToString());
            _websocket.AddHeader("User-Id", discord.CurrentUser.Id.ToString());
            _websocket.AddHeader("Client-Name", "DHCPCD9/Nomia");
            
            _websocket.OnConnected += websocketOnOnConnected;
            
            _websocket.RegisterOp<LavalinkNodeReadyPayload>("ready", Websocket_NodeReady);
            
            discord.VoiceServerUpdated += Discord_ClientOnVoiceServerUpdated;
            discord.VoiceStateUpdated += Discord_ClientOnVoiceStateUpdated;
        }
        
        /// <summary>
        /// No way...
        /// </summary>
        /// <param name="payload">Payload to send.</param>
        internal void DiscordWsSendAsync(string payload)
        { 
            var method = Discord.GetType().GetMethod("WsSendAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            
            method.Invoke(Discord, new object[] {payload});
        }
        
        private void GetPropertyValue<T>(Type type, object instance, string fieldName, out T value)
        {
            var field = type.GetProperty(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            value = (T) field.GetValue(instance);
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
            
            GetPropertyValue(vsu.GetType(), vsu, "SessionId", out string sessionId);
            var endpoint = vsr.Endpoint;
            GetPropertyValue(vsr.GetType(), vsr, "VoiceToken", out string token);
            
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


        public async Task ConnectNodeAsync()
        {
            if (_websocket is not null && _websocket.IsConnected) return;
            //Running connection in background to prevent blocking.
            await Task.Run(async () => { await _websocket.ConnectAsync(); });
        }

        public override string ToString() => $"{NodeName} ({RestEndpoint})";

        public string ToWebSocketString() => $"{NodeName} ({WebSocketEndpoint.ToWebSocketString()})";

        public async Task DestroyPlayer(DiscordChannel voiceChannel)
        {
            if (voiceChannel.Type != ChannelType.Voice && voiceChannel.Type != ChannelType.Stage) throw new ArgumentException("Channel must be a voice channel.", nameof(voiceChannel));
            
            if (!Connections.TryGetValue(voiceChannel.GuildId.Value, out var connection)) return;
            
            await connection.DisconnectAsync();
        }

        public async Task<LavalinkLoadResult> LoadTrackAsync(string query, LavalinkSearchType searchType = LavalinkSearchType.Youtube)
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
    }
}