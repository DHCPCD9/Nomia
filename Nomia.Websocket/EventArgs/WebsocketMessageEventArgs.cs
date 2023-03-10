using Emzi0767.Utilities;

namespace Nomia.Websocket.EventArgs;

public class WebsocketMessageEventArgs : AsyncEventArgs
{
    public bool IsBinary { get; }
    
    public string Message { get; }
    
    public WebsocketMessageEventArgs(bool isBinary, string message)
    {
        IsBinary = isBinary;
        Message = message;
    }
}