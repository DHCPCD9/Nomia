using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using Microsoft.Extensions.Logging;

namespace Nomia;

public static class DiscordClientExtension
{
    /// <summary>
    /// Enables Nomia for the client.
    /// </summary>
    /// <param name="client">Client to enable Nomia for.</param>
    /// <returns>Nomia extension instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when Nomia is already enabled for that client.</exception>
    public static NomiaExtension UseNomia(this DiscordClient client)
    {
        if (!client.Intents.HasIntent(DiscordIntents.GuildVoiceStates))
            client.Logger.LogCritical(NomiaEvents.Intents, "The Lavalink extension is registered but the guild voice states intent is not enabled. It is highly recommended to enable it");

        var lava = new NomiaExtension();
        client.AddExtension(lava);
        return lava;
    }
    
    
    /// <summary>
    /// Initializes all shards and enables Nomia for them.
    /// </summary>
    /// <param name="client">Client to initialize shards for.</param>
    /// <returns>Dictionary of shard IDs and their respective Nomia extensions.</returns>
    public static async Task<IReadOnlyDictionary<int, NomiaExtension>> UseNomiaAsync(this DiscordShardedClient client)
    {
        var modules = new Dictionary<int, NomiaExtension>();
        await client.InitializeShardsAsync().ConfigureAwait(false);

        foreach (var shard in client.ShardClients.Select(xkvp => xkvp.Value))
        {
            var lava = shard.GetExtension<NomiaExtension>();
            if (lava == null)
                lava = shard.UseNomia();

            modules[shard.ShardId] = lava;
        }

        return new ReadOnlyDictionary<int, NomiaExtension>(modules);
    }
    
    /// <summary>
    /// Gets the Nomia extension for the client.
    /// </summary>
    /// <param name="client">Client to get extension for.</param>
    /// <returns>Nomia extension instance.</returns>
    public static NomiaExtension GetNomia(this DiscordClient client)
        => client.GetExtension<NomiaExtension>();
    

    /// <summary>
    /// Gets all Nomia extensions for all shards.
    /// </summary>
    /// <param name="client">Client to get extensions for.</param>
    /// <returns>Dictionary of shard IDs and their respective Nomia extensions.</returns>
    public static async Task<IReadOnlyDictionary<int, NomiaExtension>> GetNomiaAsync(this DiscordShardedClient client)
    {
        await client.InitializeShardsAsync().ConfigureAwait(false);
        var extensions = new Dictionary<int, NomiaExtension>();

        foreach (var shard in client.ShardClients.Values)
        {
            extensions.Add(shard.ShardId, shard.GetExtension<NomiaExtension>());
        }

        return new ReadOnlyDictionary<int, NomiaExtension>(extensions);
    }
}