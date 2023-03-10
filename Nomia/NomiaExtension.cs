using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using Emzi0767.Utilities;
using Nomia.EventArgs;

namespace Nomia
{
    public class NomiaExtension : BaseExtension
    {
        public List<NomiaNode> Nodes { get; } = new();
        public event AsyncEventHandler<NomiaExtension, LavalinkNodeConnectedEventArgs> LavalinkNodeConnected; 
        /// <summary>
        /// Creates lavalink client for client. Do not call this method manually.
        /// </summary>
        /// <param name="client"></param>
        protected override void Setup(DiscordClient client)
        {
            if (client is null) throw new ArgumentNullException(nameof(client));
        }
        
        public void ConnectNode(NomiaNode node)
        {
            if (node is null) throw new ArgumentNullException(nameof(node));
            Nodes.Add(node);
            
        }
        
        public void DisconnectNode(NomiaNode node)
        {
            if (node is null) throw new ArgumentNullException(nameof(node));
            Nodes.Remove(node);
        }

        public async Task ConnectAllNodes()
        {
            foreach (var node in Nodes)
            {
                await node.ConnectNodeAsync();
            }
        }

        public void AddNode(NomiaNode nomiaNode)
        {
            if (nomiaNode is null) throw new ArgumentNullException(nameof(nomiaNode));
            Nodes.Add(nomiaNode);
        }

        public NomiaNode GetNode()
        {
            return Nodes.FirstOrDefault();
        }
    }
}