﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Nomia.Entities;
using Nomia.Entities.Filters;

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
    
    [Command("nightcore")]
    public async Task Nightcore(CommandContext ctx)
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

        var player = await node.ConnectAsync(voiceChannel);
        
        if (player is null)
        {
            await ctx.RespondAsync("Player is null.");
            return;
        }

        await player.AddFilterAsync(new LavalinkTimescaleFilter
        {
            Pitch = (float) 1.5,
            Speed = (float) 1.5,
            Rate = 1
        });
        
        await ctx.RespondAsync("Added nightcore filter.");
    }
    
    [Command("daycore")]
    public async Task Daycore(CommandContext ctx)
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

        var player = await node.ConnectAsync(voiceChannel);
        
        if (player is null)
        {
            await ctx.RespondAsync("Player is null.");
            return;
        }

        await player.AddFilterAsync(new LavalinkTimescaleFilter
        {
            Pitch = (float) 0.5,
            Speed = (float) 0.5,
            Rate = 1
        });
        
        await ctx.RespondAsync("Added daycore filter.");
    }
    
    [Command("reset")]
    public async Task Reset(CommandContext ctx)
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

        var player = await node.ConnectAsync(voiceChannel);
        
        if (player is null)
        {
            await ctx.RespondAsync("Player is null.");
            return;
        }

        await player.ClearFiltersAsync();
        
        await ctx.RespondAsync("Reset filters.");
    }
    
    [Command("pause")]
    public async Task Pause(CommandContext ctx)
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

        var player = await node.ConnectAsync(voiceChannel);
        
        if (player is null)
        {
            await ctx.RespondAsync("Player is null.");
            return;
        }

        await player.PauseAsync();
        
        await ctx.RespondAsync("Paused.");
    }
    
    [Command("resume")]
    public async Task Resume(CommandContext ctx)
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

        var player = await node.ConnectAsync(voiceChannel);
        
        if (player is null)
        {
            await ctx.RespondAsync("Player is null.");
            return;
        }

        await player.ResumeAsync();
        
        await ctx.RespondAsync("Resumed.");
    }
    
    [Command("stop")]
    public async Task Stop(CommandContext ctx)
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

        var player = await node.ConnectAsync(voiceChannel);
        
        if (player is null)
        {
            await ctx.RespondAsync("Player is null.");
            return;
        }

        await player.StopAsync();
        
        await ctx.RespondAsync("Stopped.");
    }
    
    


}