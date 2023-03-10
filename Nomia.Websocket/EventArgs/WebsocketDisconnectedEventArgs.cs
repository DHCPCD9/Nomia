using Emzi0767.Utilities;

namespace Nomia.Websocket.EventArgs;

public class WebsocketDisconnectedEventArgs : AsyncEventArgs
{
    public string Reason { get; }
    public WebsocketDisconnectedEventArgs(string reason)
    {
        Reason = reason;
    }
}