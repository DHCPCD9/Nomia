using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using Emzi0767.Utilities;
using Nomia.EventArgs;
using Nomia.Websocket;

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

        public override string ToString() => $"{Host}:{Port}{Route}";

        public string ToWebSocketString() => $"ws{(IsSecure ? "s" : string.Empty)}://{Host}:{Port}{Route}";
    }

    public class NomiaNode
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
            add => this._onConnected.Register(value);
            remove => this._onConnected.Unregister(value);
        }

        public event AsyncEventHandler<NomiaNode, LavalinkClientExceptionEventArgs> OnException
        {
            add => this._onException.Register(value);
            remove => this._onException.Unregister(value);
        }

        private readonly AsyncEvent<NomiaNode, LavalinkClientExceptionEventArgs> _onException;

        private readonly AsyncEvent<NomiaNode, LavalinkNodeConnectedEventArgs> _onConnected;

        public NomiaNode(DiscordClient client, NomiaEndpoint restEndpoint, NomiaEndpoint webSocketEndpoint,
            string nodeName = "Nomia Node")
        {
            if (client is null) throw new ArgumentNullException(nameof(client));
            RestEndpoint = restEndpoint ?? throw new ArgumentNullException(nameof(restEndpoint));
            WebSocketEndpoint = webSocketEndpoint ?? throw new ArgumentNullException(nameof(webSocketEndpoint));
            _websocket = new NomiaWebsocket(WebSocketEndpoint.ToWebSocketString());
            NodeName = nodeName;

            _onConnected = new AsyncEvent<NomiaNode, LavalinkNodeConnectedEventArgs>("LAVALINK_NODE_CONNECTED",
                TimeSpan.Zero, InternalHandleException);
            _onException = new AsyncEvent<NomiaNode, LavalinkClientExceptionEventArgs>("LAVALINK_NODE_EXCEPTION",
                TimeSpan.Zero, InternalHandleException);

            _websocket.AddHeader("Authorization", WebSocketEndpoint.Password);
            _websocket.AddHeader("Num-Shards", client.ShardCount.ToString());
            _websocket.AddHeader("User-Id", client.CurrentUser.Id.ToString());
            _websocket.AddHeader("Client-Name", "DHCPCD9/Nomia");
        }

        private void InternalHandleException<TArgs>(AsyncEvent<NomiaNode, TArgs> asyncevent, Exception exception,
            AsyncEventHandler<NomiaNode, TArgs> handler, NomiaNode sender, TArgs eventargs) where TArgs : AsyncEventArgs
        {
            _onException.InvokeAsync(this, new LavalinkClientExceptionEventArgs(exception));
        }


        public async Task ConnectAsync()
        {
            //Running connection in background to prevent blocking.
            await Task.Run(async () => { await _websocket.ConnectAsync(); });
        }

        public override string ToString() => $"{NodeName} ({RestEndpoint})";

        public string ToWebSocketString() => $"{NodeName} ({WebSocketEndpoint.ToWebSocketString()})";
    }
}