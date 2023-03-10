using Emzi0767.Utilities;
using Nomia.Entities;

namespace Nomia.EventArgs.Player;

public class PlaybackStartedEventArgs : AsyncEventArgs
{
    /// <summary>
    /// The track that started playing
    /// </summary>
    public LavalinkTrack Track { get; }
    
    public PlaybackStartedEventArgs(LavalinkTrack track)
    {
        Track = track;
    }
}