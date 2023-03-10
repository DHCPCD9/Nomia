using Emzi0767.Utilities;

namespace Nomia.EventArgs;

public class LavalinkNodeConnectedEventArgs : AsyncEventArgs
{
    /// <summary>
    /// Gets the node that was connected.
    /// </summary>
    public NomiaNode Node { get; }
    public LavalinkNodeConnectedEventArgs(NomiaNode node)
    {
        Node = node;
    }
}