using Emzi0767.Utilities;

namespace Nomia.EventArgs;

public class LavalinkNodeConnectedEventArgs : AsyncEventArgs
{
    public NomiaNode Node { get; }
    public LavalinkNodeConnectedEventArgs(NomiaNode node)
    {
        Node = node;
    }
}