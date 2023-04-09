using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Emzi0767.Utilities;
using Newtonsoft.Json;
using Nomia.Entities;
using Nomia.Entities.Filters;
using Nomia.EventArgs.Player;

namespace Nomia;

/// <summary>
/// Represents a connection to a guild.
/// </summary>
public class LavalinkGuildConnection
{
    
    /// <summary>
    /// Fires when a track starts playing.
    /// </summary>
    public event AsyncEventHandler<LavalinkGuildConnection, PlaybackStartedEventArgs> OnTrackStart
    {
        add => _onTrackStart.Register(value);
        remove => _onTrackStart.Unregister(value);
    }
    
    /// <summary>
    /// Fires when a track finishes playing.
    /// </summary>
    public event AsyncEventHandler<LavalinkGuildConnection, PlaybackFinishedEventArgs> OnTrackFinish
    {
        add => _onTrackFinish.Register(value);
        remove => _onTrackFinish.Unregister(value);
    }
    
    /// <summary>
    /// Fires when a track encounters an exception.
    /// </summary>
    public event AsyncEventHandler<LavalinkGuildConnection, PlaybackExceptionEventArgs> OnTrackException
    {
        add => _onTrackException.Register(value);
        remove => _onTrackException.Unregister(value);
    }
    
    /// <summary>
    /// Fires when a track gets stuck.
    /// </summary>
    public event AsyncEventHandler<LavalinkGuildConnection, PlaybackStuckEventArgs> OnTrackStuck
    {
        add => _onTrackStuck.Register(value);
        remove => _onTrackStuck.Unregister(value);
    }
    
    /// <summary>
    /// Fires when the player is updated.
    /// </summary>
    public event AsyncEventHandler<LavalinkGuildConnection, PlayerUpdateEventArgs> OnPlayerUpdate
    {
        add => _onPlayerUpdate.Register(value);
        remove => _onPlayerUpdate.Unregister(value);
    }
    
    /// <summary>
    /// Fires when the player websocket is closed.
    /// </summary>
    public event AsyncEventHandler<LavalinkGuildConnection, PlayerWebsocketClosedEventArgs> OnWebsocketClosed
    {
        add => _onPlayerWebsocketClosed.Register(value);
        remove => _onPlayerWebsocketClosed.Unregister(value);
    }
    
    /// <summary>
    /// Fires when the player encounters an internal error.
    /// </summary>
    public event AsyncEventHandler<LavalinkGuildConnection, PlayerInternalError> OnPlayerError
    {
        add => _onPlayerError.Register(value);
        remove => _onPlayerError.Unregister(value);
    }
    
    internal VoiceServerUpdateEventArgs VoiceServerUpdateEventArgs { get; set; }
    internal VoiceStateUpdateEventArgs VoiceStateUpdateEventArgs { get; set; }

    /// <summary>
    /// The guild this player is connected to.
    /// </summary>
    public DiscordChannel Channel => VoiceStateUpdateEventArgs.Channel;
    
    /// <summary>
    /// Is the player connected to a voice channel.
    /// </summary>
    public bool IsConnected => Channel != null;

    /// <summary>
    /// Filters applied to the player.
    /// </summary>
    public List<ILavalinkFilter> Filters { get; }
    
    /// <summary>
    /// Current state of the player.
    /// </summary>
    public LavalinkPlayerState State { get; set; }
    
    internal readonly AsyncEvent<LavalinkGuildConnection, PlaybackStartedEventArgs> _onTrackStart;
    internal readonly AsyncEvent<LavalinkGuildConnection, PlaybackFinishedEventArgs> _onTrackFinish;
    internal readonly AsyncEvent<LavalinkGuildConnection, PlaybackExceptionEventArgs> _onTrackException;
    internal readonly AsyncEvent<LavalinkGuildConnection, PlaybackStuckEventArgs> _onTrackStuck;
    internal readonly AsyncEvent<LavalinkGuildConnection, PlayerUpdateEventArgs> _onPlayerUpdate;
    internal readonly AsyncEvent<LavalinkGuildConnection, PlayerWebsocketClosedEventArgs> _onPlayerWebsocketClosed;
    internal readonly AsyncEvent<LavalinkGuildConnection, PlayerInternalError> _onPlayerError;
    private readonly NomiaNode _node;

    public LavalinkGuildConnection(NomiaNode node, VoiceServerUpdateEventArgs vsu, VoiceStateUpdateEventArgs vstu)
    {
        _node = node;
        VoiceServerUpdateEventArgs = vsu;
        VoiceStateUpdateEventArgs = vstu;
        Filters = new List<ILavalinkFilter>();
        
        _onTrackStart = new AsyncEvent<LavalinkGuildConnection, PlaybackStartedEventArgs>("ON_TRACK_START", TimeSpan.Zero, ConnectionErrorHandler);
        _onTrackFinish = new AsyncEvent<LavalinkGuildConnection, PlaybackFinishedEventArgs>("ON_TRACK_FINISH", TimeSpan.Zero, ConnectionErrorHandler);
        _onTrackException = new AsyncEvent<LavalinkGuildConnection, PlaybackExceptionEventArgs>("ON_TRACK_EXCEPTION", TimeSpan.Zero, ConnectionErrorHandler);
        _onTrackStuck = new AsyncEvent<LavalinkGuildConnection, PlaybackStuckEventArgs>("ON_TRACK_STUCK", TimeSpan.Zero, ConnectionErrorHandler);
        _onPlayerUpdate = new AsyncEvent<LavalinkGuildConnection, PlayerUpdateEventArgs>("ON_PLAYER_UPDATE", TimeSpan.Zero, ConnectionErrorHandler);
        _onPlayerWebsocketClosed = new AsyncEvent<LavalinkGuildConnection, PlayerWebsocketClosedEventArgs>("ON_PLAYER_WEBSOCKET_CLOSED", TimeSpan.Zero, ConnectionErrorHandler);
        _onPlayerError = new AsyncEvent<LavalinkGuildConnection, PlayerInternalError>("ON_PLAYER_ERROR", TimeSpan.Zero, ConnectionErrorHandler);
        
    }

    private void ConnectionErrorHandler<T>(AsyncEvent<LavalinkGuildConnection, T> asyncevent, Exception exception, AsyncEventHandler<LavalinkGuildConnection, T> handler, LavalinkGuildConnection sender, T eventargs) where T : AsyncEventArgs
    {
        _onPlayerError.InvokeAsync(this, new PlayerInternalError(exception));
    }


    /// <summary>
    /// Plays a track.
    /// </summary>
    /// <param name="track">Track to play.</param>
    /// <exception cref="InvalidOperationException">Thrown when the node is not ready or the player is not connected.</exception>
    public async Task PlayAsync(LavalinkTrack track)
    {
        if (!_node.IsReady || !IsConnected)
        {
            throw new InvalidOperationException("Node is not ready or not connected");
        }
        
        await _node.Rest.UpdatePlayer(VoiceStateUpdateEventArgs.Guild.Id, new LavalinkPlayerUpdatePayload
        {
            EncodedTrack = track.Encoded
        });
    }
    
    /// <summary>
    /// Stops the player. (Finishes the current track)
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the node is not ready or the player is not connected.</exception>
    public async Task StopAsync()
    {
        if (!_node.IsReady || !IsConnected)
        {
            throw new InvalidOperationException("Node is not ready or not connected");
        }
        
        await _node.Rest.UpdatePlayer(VoiceStateUpdateEventArgs.Guild.Id, new LavalinkPlayerUpdatePayload
        {
            EncodedTrack = null
        });
    }
    
    /// <summary>
    /// Pauses the player.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the node is not ready or the player is not connected.</exception>
    public async Task PauseAsync()
    {
        if (!_node.IsReady || !IsConnected)
        {
            throw new InvalidOperationException("Node is not ready or not connected");
        }
        
        await _node.Rest.UpdatePlayer(VoiceStateUpdateEventArgs.Guild.Id, new LavalinkPlayerUpdatePayload
        {
            Paused = true
        });
    }
    
    /// <summary>
    /// Resumes the player.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the node is not ready or the player is not connected.</exception>
    public async Task ResumeAsync()
    {
        if (!_node.IsReady || !IsConnected)
        {
            throw new InvalidOperationException("Node is not ready or not connected");
        }
        
        await _node.Rest.UpdatePlayer(VoiceStateUpdateEventArgs.Guild.Id, new LavalinkPlayerUpdatePayload
        {
            Paused = false
        });
    }
    
    /// <summary>
    /// Seeks to a position in the current track.
    /// </summary>
    /// <param name="position">Position to seek to.</param>
    /// <exception cref="InvalidOperationException">Thrown when the node is not ready or the player is not connected.</exception>
    public async Task SeekAsync(TimeSpan position)
    {
        if (!_node.IsReady || !IsConnected)
        {
            throw new InvalidOperationException("Node is not ready or not connected");
        }
        
        await _node.Rest.UpdatePlayer(VoiceStateUpdateEventArgs.Guild.Id, new LavalinkPlayerUpdatePayload
        {
            Position = position.Milliseconds
        });
    }
    
    /// <summary>
    /// Sets the volume of the player.
    /// </summary>
    /// <param name="volume">Volume to set.</param>
    /// <exception cref="InvalidOperationException">Thrown when the node is not ready or the player is not connected.</exception>
    public async Task SetVolumeAsync(int volume)
    {
        if (!_node.IsReady || !IsConnected)
        {
            throw new InvalidOperationException("Node is not ready or not connected");
        }
        
        await _node.Rest.UpdatePlayer(VoiceStateUpdateEventArgs.Guild.Id, new LavalinkPlayerUpdatePayload
        {
            Volume = volume
        });
    }
    
    /// <summary>
    /// Sets the volume of the player using a filter.
    /// </summary>
    /// <param name="volume">Volume to set.</param>
    /// <exception cref="InvalidOperationException">Thrown when the node is not ready or the player is not connected.</exception>
    public async Task SetFilterVolumeAsync(int volume)
    {
        if (!_node.IsReady || !IsConnected)
        {
            throw new InvalidOperationException("Node is not ready or not connected");
        }
        
        await _node.Rest.UpdatePlayer(VoiceStateUpdateEventArgs.Guild.Id, new LavalinkPlayerUpdatePayload
        {
            Filters = new()
            {
                Volume = volume
            }
        });
    }
    
    /// <summary>
    /// Sets the equalizer of the player.
    /// </summary>
    /// <param name="filter">Equalizer to set.</param>
    /// <exception cref="InvalidOperationException">Thrown when the node is not ready or the player is not connected.</exception>
    public async Task AddFilterAsync(ILavalinkFilter filter)
    {
        if (!_node.IsReady || !IsConnected)
        {
            throw new InvalidOperationException("Node is not ready or not connected");
        }
        
        var filters = new List<ILavalinkFilter>(Filters);
        if (filters.Contains(filter))
        {
            filters.Remove(filter);
            filters.Add(filter);
        }
        else
        {
            filters.Add(filter);
        }
        
        await _node.Rest.UpdatePlayer(VoiceStateUpdateEventArgs.Guild.Id, new LavalinkPlayerUpdatePayload
        {
            Filters = LavalinkFilters.FromFilters(filters)
        });
    }
    
    /// <summary>
    /// Removes a filter from the player.
    /// </summary>
    /// <param name="filter">Filter to remove.</param>
    /// <exception cref="InvalidOperationException">Thrown when the node is not ready or the player is not connected.</exception>
    public async Task RemoveFilterAsync(ILavalinkFilter filter)
    {
        if (!_node.IsReady || !IsConnected)
        {
            throw new InvalidOperationException("Node is not ready or not connected");
        }
        
        var filters = new List<ILavalinkFilter>(Filters);
        filters.Remove(filter);
        
        await _node.Rest.UpdatePlayer(VoiceStateUpdateEventArgs.Guild.Id, new LavalinkPlayerUpdatePayload
        {
            Filters = LavalinkFilters.FromFilters(filters)
        });
    }
    
    /// <summary>
    /// Clears all filters from the player.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the node is not ready or the player is not connected.</exception>
    public async Task ClearFiltersAsync()
    {
        if (!_node.IsReady || !IsConnected)
        {
            throw new InvalidOperationException("Node is not ready or not connected");
        }
        //Resetting values to default
        foreach (var filter in Filters)
        {
            filter.Reset();
        }
        
        await _node.Rest.UpdatePlayer(VoiceStateUpdateEventArgs.Guild.Id, new LavalinkPlayerUpdatePayload
        {
            Filters = LavalinkFilters.FromFilters(Filters)
        });
        
        //Clearing the list
        Filters.Clear();
    }
    

    /// <summary>
    /// Disconnects the player from the voice channel.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task DisconnectAsync()
    {
        if (!_node.IsReady || !IsConnected)
        {
            throw new InvalidOperationException("Node is not ready or not connected");
        }
        
        //Disconnecting from the voice channel
        var vsd = new VoiceDispatch
        {
            OpCode = 4,
            Payload = new VoiceStateUpdatePayload
            {
                GuildId = VoiceStateUpdateEventArgs.Guild.Id,
                ChannelId = null,
                Deafened = false,
                Muted = false
            }
        };
        
        var vsdJson = JsonConvert.SerializeObject(vsd);
        _node.DiscordWsSendAsync(vsdJson);
        
        //Removing the connection from the node
        await _node.Rest.DestroyPlayer(VoiceStateUpdateEventArgs.Guild.Id);
        
        //Removing the connection from the node
        _node.Connections.Remove(VoiceStateUpdateEventArgs.Guild.Id);
    }
}