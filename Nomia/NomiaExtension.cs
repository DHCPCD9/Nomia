using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using Emzi0767.Utilities;
using Nomia.EventArgs;

namespace Nomia
{
    public class NomiaExtension : BaseExtension, IDisposable
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
            
            Client = client;
        }
        
        /// <summary>
        /// Connects a node to the client.
        /// </summary>
        /// <param name="node"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void ConnectNode(NomiaNode node)
        {
            if (node is null) throw new ArgumentNullException(nameof(node));
            Nodes.Add(node);
            
        }
        
        /// <summary>
        /// Disconnects a node and removes it from the client.
        /// </summary>
        /// <param name="node">Node</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void DisconnectNode(NomiaNode node)
        {
            if (node is null) throw new ArgumentNullException(nameof(node));
            node.Disconnect();
        }
        
        /// <summary>
        /// Connects all nodes.
        /// </summary>
        public async Task ConnectAllNodes()
        {
            foreach (var node in Nodes)
            {
                await node.ConnectNodeAsync(Client).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Add a node to the client.
        /// </summary>
        /// <param name="nomiaNode">Node</param>
        /// <exception cref="ArgumentNullException">Thrown when node is null.</exception>
        public void AddNode(NomiaNode nomiaNode)
        {
            if (nomiaNode is null) throw new ArgumentNullException(nameof(nomiaNode));
            Nodes.Add(nomiaNode);
        }

        /// <summary>
        /// Get the less loaded node.
        /// </summary>
        /// <returns></returns>
        public NomiaNode GetNode()
        {
            if (!Nodes.Any() || !Nodes.Any(c => c.IsReady)) throw new InvalidOperationException("No nodes are connected.");

            return Nodes.Where(c => c.IsReady).OrderBy(x => x.Stats.PlayingPlayers).First();   
        }

        internal void RemoveNode(NomiaNode nomiaNode)
        {
            Nodes.Remove(nomiaNode);
        }

        public void Dispose()
        {
            foreach (var node in Nodes)
            {
                node.Dispose();
            }
        }
    }
}