using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;
using Nomia.Entities;
using Nomia.Entities.Filters;

namespace Nomia;

public class LavalinkGuildConnection : IDisposable
{
    private readonly NomiaNode _node;
    public VoiceServerUpdateEventArgs VoiceServerUpdateEventArgs { get; set; }
    public VoiceStateUpdateEventArgs VoiceStateUpdateEventArgs { get; set; }

    public DiscordChannel Channel => VoiceStateUpdateEventArgs.Channel;
    
    public bool IsConnected => Channel != null;

    public List<ILavalinkFilter> Filters { get; }

    public LavalinkGuildConnection(NomiaNode node, VoiceServerUpdateEventArgs vsu, VoiceStateUpdateEventArgs vstu)
    {
        _node = node;
        VoiceServerUpdateEventArgs = vsu;
        VoiceStateUpdateEventArgs = vstu;
        Filters = new List<ILavalinkFilter>();
    }
    
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
    
    public async Task AddFilterAsync(ILavalinkFilter filter)
    {
        if (!_node.IsReady || !IsConnected)
        {
            throw new InvalidOperationException("Node is not ready or not connected");
        }
        
        var filters = new List<ILavalinkFilter>(Filters);
        filters.Add(filter);
        
        await _node.Rest.UpdatePlayer(VoiceStateUpdateEventArgs.Guild.Id, new LavalinkPlayerUpdatePayload
        {
            Filters = LavalinkFilters.FromFilters(filters)
        });
    }
    
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


    public void Dispose()
    {
        
    }

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