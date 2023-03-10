using Emzi0767.Utilities;
using Nomia.Entities;

namespace Nomia.EventArgs.Player;

public class PlayerUpdateEventArgs : AsyncEventArgs
{
    /// <summary>
    /// The player state
    /// </summary>
    public LavalinkPlayerState State { get; }
    
    public PlayerUpdateEventArgs(LavalinkPlayerState state)
    {
        State = state;
    }
}