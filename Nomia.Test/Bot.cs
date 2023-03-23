using System.Reflection;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace Nomia.Test;

public class Bot : IDisposable, IAsyncDisposable
{
    
    public DiscordShardedClient Client { get; }

    public Bot()
    {
        var token = Environment.GetEnvironmentVariable("TOKEN");
        if (string.IsNullOrEmpty(token))
        {
            throw new Exception("No token provided");
        }

        Client = new DiscordShardedClient(new DiscordConfiguration
        {
            Token = token,
            TokenType = TokenType.Bot,
            MinimumLogLevel = LogLevel.Debug,
            Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents | DiscordIntents.GuildMembers
        });

        
        Client.Ready += ClientOnReady;
    }

    private async Task ClientOnReady(DiscordClient sender, ReadyEventArgs e)
    {
        Client.Logger.LogInformation("Bot is ready");
        
        var nomia = await Client.UseNomiaAsync();
        
        foreach (var (k, shard) in nomia)
        {
            
            var restEndpoint = new NomiaEndpoint("localhost", 2333, "youshallnotpass", "/v3");
            var wsEndpoint = new NomiaEndpoint("localhost", 2333, "youshallnotpass", "/v3/websocket");
            shard.AddNode(new NomiaNode(restEndpoint, wsEndpoint));

            await shard.ConnectAllNodes();
        }
    }

    public void Dispose()
    {
        DisposeAsync().ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        await Client.StopAsync();
    }

    public async Task Run()
    {
        var cncfg = new CommandsNextConfiguration
        {
            StringPrefixes = new[] {"!"}
        };
       
        var commands = await Client.UseCommandsNextAsync(cncfg);
        
        foreach (var (k, commandShard) in commands)
        {
            commandShard.RegisterCommands(Assembly.GetExecutingAssembly());
        }
        
    
        
        
        await Client.StartAsync();
    }
}