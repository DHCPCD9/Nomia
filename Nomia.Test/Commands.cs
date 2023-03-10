using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Nomia.Entities;

namespace Nomia.Test;

public class Commands : BaseCommandModule
{
    [Command("ping")]
    public async Task Ping(CommandContext ctx)
    {
        await ctx.RespondAsync("Pong!");
    }
    
    [Command("join")]
    public async Task Join(CommandContext ctx)
    {
        var voiceState = ctx.Member?.VoiceState;
        if (voiceState == null)
        {
            await ctx.RespondAsync("You are not in a voice channel.");
            return;
        }

        var voiceChannel = voiceState.Channel;
        if (voiceChannel == null)
        {
            await ctx.RespondAsync("You are not in a voice channel.");
            return;
        }

        var nomia = ctx.Client.GetNomia();
        var node = nomia.GetNode();

        try
        {
            await node.ConnectAsync(voiceChannel);
        }
        catch (Exception e)
        {
            await ctx.RespondAsync(e.Message);
            await ctx.RespondAsync(e.StackTrace);
        }
    }
    
    [Command("leave")]
    public async Task Leave(CommandContext ctx)
    {
        var voiceState = ctx.Member?.VoiceState;
        if (voiceState == null)
        {
            await ctx.RespondAsync("You are not in a voice channel.");
            return;
        }

        var voiceChannel = voiceState.Channel;
        if (voiceChannel == null)
        {
            await ctx.RespondAsync("You are not in a voice channel.");
            return;
        }

        var nomia = ctx.Client.GetNomia();
        var node = nomia.GetNode();

        try
        {
            await node.DestroyPlayer(voiceChannel);
        }
        catch (Exception e)
        {
            await ctx.RespondAsync(e.Message);
            await ctx.RespondAsync(e.StackTrace);
        }
    }
    
    [Command("play")]
    public async Task Play(CommandContext ctx, [RemainingText] string query)
    {
        var voiceState = ctx.Member?.VoiceState;
        if (voiceState == null)
        {
            await ctx.RespondAsync("You are not in a voice channel.");
            return;
        }

        var voiceChannel = voiceState.Channel;
        if (voiceChannel == null)
        {
            await ctx.RespondAsync("You are not in a voice channel.");
            return;
        }

        var nomia = ctx.Client.GetNomia();
        var node = nomia.GetNode();

        try
        {
            var player = await node.ConnectAsync(voiceChannel);
            var tracks = await node.LoadTrackAsync(query);
            var track = tracks.Tracks.FirstOrDefault();
            
            if (track is null)
            {
                await ctx.RespondAsync("No tracks found.");
                return;
            }
            await player.PlayAsync(track);
            
            await ctx.RespondAsync($"Playing {track.Info}");
        }
        catch (Exception e)
        {
            await ctx.RespondAsync(e.Message);
            await ctx.RespondAsync(e.StackTrace);
        }
    }


}