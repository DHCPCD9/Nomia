using System.Reflection;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace Nomia.Test;

public class Bot : IDisposable, IAsyncDisposable
{
    
    public DiscordClient Client { get; }

    public Bot()
    {
        var token = Environment.GetEnvironmentVariable("TOKEN");
        if (string.IsNullOrEmpty(token))
        {
            throw new Exception("No token provided");
        }

        Client = new DiscordClient(new DiscordConfiguration
        {
            Token = token,
            TokenType = TokenType.Bot,
            MinimumLogLevel = LogLevel.Debug,
            Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents | DiscordIntents.GuildMembers
        });

        Client.UseNomia();
        
        var cncfg = new CommandsNextConfiguration
        {
            StringPrefixes = new[] {"!"}
        };
       
        var commands = Client.UseCommandsNext(cncfg);
        commands.RegisterCommands(Assembly.GetExecutingAssembly());
        
        Client.Ready += ClientOnReady;
    }

    private async Task ClientOnReady(DiscordClient sender, ReadyEventArgs e)
    {
        var nomia = sender.GetNomia();

        var restEndpoint = new NomiaEndpoint("localhost", 2333, "youshallnotpass", "/v3");
        var wsEndpoint = new NomiaEndpoint("localhost", 2333, "youshallnotpass", "/v3/websocket");
        nomia.AddNode(new NomiaNode(Client, restEndpoint, wsEndpoint, "localhost"));

        Task.Run(async () =>
        {
            await nomia.ConnectAllNodes();
        });
        
        Client.Logger.LogInformation("Bot is ready");
    }

    public void Dispose()
    {
        Client.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
    }

    public async Task Run()
    {
        await Client.ConnectAsync();
    }
}