using Newtonsoft.Json;

namespace Nomia.Entities;

public class LavalinkMemory
{
    /// <summary>
    /// The amount of free memory in bytes
    /// </summary>
    [JsonProperty("free")]
    public long Free { get; set; }
    
    /// <summary>
    /// The amount of used memory in bytes
    /// </summary>
    [JsonProperty("used")]
    public long Used { get; set; }
    
    /// <summary>
    /// The amount of allocated memory in bytes
    /// </summary>
    [JsonProperty("allocated")]
    public long Allocated { get; set; }
    
    /// <summary>
    /// The amount of reservable memory in bytes
    /// </summary>
    [JsonProperty("reservable")]
    public long Reservable { get; set; }
}

public class LavalinkCpu
{
    /// <summary>
    /// The amount of cores the node has
    /// </summary>
    [JsonProperty("cores")]
    public int Cores { get; set; }
    
    /// <summary>
    /// The system load of the node
    /// </summary>
    [JsonProperty("systemLoad")]
    public double SystemLoad { get; set; }
    
    /// <summary>
    /// The load of Lavalink on the node
    /// </summary>
    [JsonProperty("lavalinkLoad")]
    public double LavalinkLoad { get; set; }
}

public class LavalinkFrameStats
{
    /// <summary>
    /// The amount of frames sent to Discord
    /// </summary>
    [JsonProperty("sent")]
    public long Sent { get; set; }
    
    /// <summary>
    /// The amount of frames that were nulled
    /// </summary>
    [JsonProperty("nulled")]
    public long Nulled { get; set; }
    
    /// <summary>
    /// The amount of frames that were deficit
    /// </summary>
    [JsonProperty("deficit")]
    public long Deficit { get; set; }
}

public class LavalinkStats
{
    /// <summary>
    /// The amount of players connected to the node
    /// </summary>
    [JsonProperty("players")]
    public int Players { get; set; }
    
    /// <summary>
    /// The amount of players playing a track
    /// </summary>
    [JsonProperty("playingPlayers")]
    public int PlayingPlayers { get; set; }
    
    /// <summary>
    /// The uptime of the node in milliseconds
    /// </summary>
    [JsonProperty("uptime")]
    public long Uptime { get; set; }
    
    /// <summary>
    /// The memory stats of the node
    /// </summary>
    [JsonProperty("memory")]
    public LavalinkMemory Memory { get; set; }
    
    /// <summary>
    /// The cpu stats of the node
    /// </summary>
    [JsonProperty("cpu")]
    public LavalinkCpu Cpu { get; set; }
    
    /// <summary>
    /// The frame stats of the node.
    /// </summary>
    [JsonProperty("frameStats")]
    public LavalinkFrameStats FrameStats { get; set; }
}