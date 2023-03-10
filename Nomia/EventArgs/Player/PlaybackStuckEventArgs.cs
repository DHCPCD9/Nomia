using Emzi0767.Utilities;
using Nomia.Entities;

namespace Nomia.EventArgs.Player;

public class PlaybackStuckEventArgs : AsyncEventArgs
{
    /// <summary>
    /// The track that got stuck
    /// </summary>
    public LavalinkTrack Track { get; }
    
    /// <summary>
    /// The threshold in milliseconds
    /// </summary>
    public int ThresholdMs { get; }
    
    public PlaybackStuckEventArgs(LavalinkTrack track, int thresholdMs)
    {
        Track = track;
        ThresholdMs = thresholdMs;
    }
}