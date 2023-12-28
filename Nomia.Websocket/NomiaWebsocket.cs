using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Emzi0767.Utilities;
using Newtonsoft.Json.Linq;
using Nomia.Websocket.Entities;
using Nomia.Websocket.EventArgs;
using Websocket.Client;

namespace Nomia.Websocket
{
    public class NomiaWebsocket : IDisposable
    {
        /// <summary>
        /// Fired when the websocket is connected.
        /// </summary>
        public event AsyncEventHandler<NomiaWebsocket, WebsocketConnectedEventArgs> OnConnected;
        
        /// <summary>
        /// Fired when the websocket is disconnected.
        /// </summary>
        public event AsyncEventHandler<NomiaWebsocket, WebsocketDisconnectedEventArgs> OnDisconnected;
        
        /// <summary>
        /// Fired when a message is received.
        /// </summary>
        public event AsyncEventHandler<NomiaWebsocket, WebsocketMessageEventArgs> OnMessage;
        
        /// <summary>
        /// Whether the websocket is connected.
        /// </summary>
        public bool IsConnected => websocket is not null && websocket.IsRunning;
        /// <summary>
        /// OP code key for handling messages.
        /// </summary>
        public string OpParam { get; set; } = "op";

        /// <summary>
        /// Uri of the websocket.
        /// </summary>
        public Uri Uri { get; }

        /// <summary>
        /// Headers of the websocket.
        /// </summary>
        public Dictionary<string, string> Headers { get; } = new();

        private readonly Dictionary<string, INomiaHandlerEntry> _handlers = new();
        private WebsocketClient websocket;
        
        /// <summary>
        /// Creates a new instance of <see cref="NomiaWebsocket"/>.
        /// </summary>
        /// <param name="uri">Uri of the websocket.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="uri"/> is null.</exception>
        public NomiaWebsocket(Uri uri)
        {
            Uri = uri ?? throw new ArgumentNullException(nameof(uri));
        }
        
        /// <summary>
        /// Creates a new instance of <see cref="NomiaWebsocket"/>.
        /// </summary>
        /// <param name="uri">Uri of the websocket.</param>
        public NomiaWebsocket(string uri) : this(new Uri(uri))
        {
        }
        
        /// <summary>
        /// Creates a new instance of <see cref="NomiaWebsocket"/>.
        /// </summary>
        /// <param name="host">Host of the websocket.</param>
        /// <param name="port">Port of the websocket.</param>
        /// <param name="route">Route of the websocket.</param>
        /// <param name="isSecure">Whether the websocket is secure.</param>
        public NomiaWebsocket(string host, int port, string route = "/", bool isSecure = false) : this(new Uri($"ws{(isSecure ? "s" : string.Empty)}://{host}:{port}{route}"))
        {
        }
        
        /// <summary>
        /// Adds a header to the websocket.
        /// </summary>
        /// <param name="key">Key of the header.</param>
        /// <param name="value">Value of the header.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> or <paramref name="value"/> is null.</exception>
        public void AddHeader(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value));
            
            Headers.Add(key, value);
        }
        
        /// <summary>
        /// Removes a header from the websocket.
        /// </summary>
        /// <param name="key">Key of the header.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is null.</exception>
        public void RemoveHeader(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            
            Headers.Remove(key);
        }
        
        /// <summary>
        /// Clears all headers from the websocket.
        /// </summary>
        public void ClearHeaders()
        {
            Headers.Clear();
        }
        
        /// <summary>
        /// Represents the websocket as a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Uri.ToString();
        
        /// <summary>
        /// Connects to the websocket.
        /// </summary>
        public async Task ConnectAsync()
        {
            var factory = new Func<ClientWebSocket>(() =>
            {
                var client = new ClientWebSocket();
                foreach (var (key, value) in Headers)
                {
                    client.Options.SetRequestHeader(key, value);
                }
                return client;
            });
            websocket = new WebsocketClient(Uri, factory);
            websocket.ErrorReconnectTimeout = TimeSpan.FromSeconds(10);
            websocket.ReconnectTimeout = null; //Disable weird reconnect timeout
            websocket.ReconnectionHappened.Subscribe(InternalOnReconnected);
            websocket.DisconnectionHappened.Subscribe(InternalOnDisconnected);
            websocket.MessageReceived.Subscribe(InternalOnMessage);

            await websocket.Start().ConfigureAwait(false);
        }

        private void InternalOnDisconnected(DisconnectionInfo disconnectionInfo)
        {
            OnDisconnected?.Invoke(this, new WebsocketDisconnectedEventArgs(disconnectionInfo.CloseStatusDescription));
        }

        private void InternalOnReconnected(ReconnectionInfo reconnectionInfo)
        {
            OnConnected?.Invoke(this, new WebsocketConnectedEventArgs());
        }
        
        private void InternalOnMessage(ResponseMessage responseMessage)
        {
            if (responseMessage.MessageType == WebSocketMessageType.Text)
            {
                //Handling op code
                var jObject = JObject.Parse(responseMessage.Text);
                
                if (!jObject.TryGetValue(OpParam, out _))
                {
                    return;
                }
                
                
                var op = jObject[OpParam].ToString();
                
                
                
                if (_handlers.ContainsKey(op))
                {
                    var handler = _handlers[op];
                    var obj = jObject.ToObject(handler.Type);
                    handler.Handler(obj);
                }
            }
            
            OnMessage?.Invoke(this, new WebsocketMessageEventArgs(responseMessage.MessageType == WebSocketMessageType.Binary, responseMessage.Text));
        }
        
        /// <summary>
        /// Registers an op code.
        /// </summary>
        /// <param name="op">Op code.</param>
        /// <param name="handler">Handler for the op code.</param>
        /// <typeparam name="T">Type of the object.</typeparam>
        public void RegisterOp<T>(string op, Func<T, Task> handler)
        {
            _handlers.Add(op, new NomiaHandlerEntry<T>
            {
                Handler = (obj) => handler((T) obj)
            });
        }
        
        /// <summary>
        /// Registers an op code.
        /// </summary>
        /// <param name="op">Op code.</param>
        /// <param name="handler">Handler for the op code.</param>
        public void RegisterOp(string op, Func<object, Task> handler)
        {
            _handlers.Add(op, new NomiaHandlerEntry<object>
            {
                Handler = handler
            });
        }
        
        /// <summary>
        /// Unregisters an op code.
        /// </summary>
        /// <param name="op">Op code.</param>
        public void UnregisterOp(string op)
        {
            _handlers.Remove(op);
        }
        
        /// <summary>
        /// Unregisters all op codes.
        /// </summary>
        public void UnregisterAllOps()
        {
            _handlers.Clear();
        }
        
        public void Dispose()
        {
            websocket?.Dispose();
        }

        /// <summary>
        /// Sends a message to the websocket.
        /// </summary>
        /// <param name="message">message to send</param>
        /// <param name="instantly">Send the message instantly. (bypass queue)</param>
        public void Send(string message, bool instantly = false)
        {
            Send(Encoding.UTF8.GetBytes(message), instantly);
        }

        /// <summary>
        /// Sends a message to the websocket.
        /// </summary>
        /// <param name="message">message to send</param>
        /// <param name="instantly">Send the message instantly. (bypass queue)</param>
        public void Send(byte[] message, bool instantly = false)
        {
            if (instantly)
            {
                websocket.SendInstant(message);
            }
            else
            {
                websocket.Send(message);
            }
        }
    }
}