

using DSharpPlus.AsyncEvents;

namespace Nomia.EventArgs;

public class LavalinkNodeReadyEventArgs : AsyncEventArgs
{
    /// <summary>
    /// If a session was resumed
    /// </summary>
    public bool Resumed { get; }
    
    /// <summary>
    /// The Lavalink session ID of this connection. Not to be confused with a Discord voice session id
    /// </summary>
    public string SessionId { get; }
    
    public LavalinkNodeReadyEventArgs(bool resumed, string sessionId)
    {
        Resumed = resumed;
        SessionId = sessionId;
    }
}