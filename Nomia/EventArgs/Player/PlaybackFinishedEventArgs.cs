using Emzi0767.Utilities;
using Nomia.Entities;

namespace Nomia.EventArgs.Player;

public class PlaybackFinishedEventArgs : AsyncEventArgs
{
    /// <summary>
    /// The track that finished playing
    /// </summary>
    public LavalinkTrack Track { get; }
    
    /// <summary>
    /// The reason the track ended
    /// </summary>
    public LavalinkTrackEndReason EndReason { get; }
    
    public PlaybackFinishedEventArgs(LavalinkTrack track, LavalinkTrackEndReason endReason)
    {
        Track = track;
        EndReason = endReason;
    }
}