using Emzi0767.Utilities;

namespace Nomia.EventArgs;

public class LavalinkNodeDisconnectedEventArgs : AsyncEventArgs
{
    /// <summary>
    /// Gets the node that was disconnected.
    /// </summary>
    public NomiaNode Node { get; }
    public LavalinkNodeDisconnectedEventArgs(NomiaNode node)
    {
        Node = node;
    }
}