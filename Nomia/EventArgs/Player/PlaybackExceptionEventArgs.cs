using Emzi0767.Utilities;
using Nomia.Entities;

namespace Nomia.EventArgs.Player;

public class PlaybackExceptionEventArgs : AsyncEventArgs
{
    /// <summary>
    /// The track that caused the exception
    /// </summary>
    public LavalinkTrack Track { get; }
    
    /// <summary>
    /// The exception that was thrown
    /// </summary>
    public LavalinkException Exception { get; }
    
    public PlaybackExceptionEventArgs(LavalinkTrack track, LavalinkException exception)
    {
        Track = track;
        Exception = exception;
    }
}