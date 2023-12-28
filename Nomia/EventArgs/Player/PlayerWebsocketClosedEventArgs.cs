using Emzi0767.Utilities;
using Nomia.Entities;

namespace Nomia.EventArgs.Player;

public class PlayerWebsocketClosedEventArgs : AsyncEventArgs
{
    /// <summary>
    /// The close code
    /// </summary>
    public int Code { get; }
    
    /// <summary>
    /// The close reason
    /// </summary>
    public string Reason { get; }
    /// <summary>
    /// Whether the connection was closed by Discord
    /// </summary>
    public bool ByRemote { get; }
    
    public PlayerWebsocketClosedEventArgs(int code, string reason, bool byRemote)
    {
        Code = code;
        Reason = reason;
        ByRemote = byRemote;
    }
}