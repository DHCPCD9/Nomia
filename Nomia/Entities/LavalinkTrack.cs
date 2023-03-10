using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Nomia.Entities;

public class LavalinkTrack
{
    [JsonProperty("encoded")]
    public string Encoded { get; set; }

    [JsonProperty("info")]
    public LavalinkTrackInfo Info { get; set; }
}

public enum LavalinkSearchType
{
    [EnumMember(Value = "ytsearch")]
    Youtube,
    [EnumMember(Value = "scsearch")]
    Soundcloud,
    Raw
}

public enum LavalinkLoadType
{
    [EnumMember(Value = "TRACK_LOADED")]
    TrackLoaded,
    [EnumMember(Value = "PLAYLIST_LOADED")]
    PlaylistLoaded,
    [EnumMember(Value = "NO_MATCHES")]
    NoMatches,
    [EnumMember(Value = "LOAD_FAILED")]
    LoadFailed,
    [EnumMember(Value = "SEARCH_RESULT")]
    SearchResult
}

public class LavalinkPlaylistInfo {
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("selectedTrack")]
    public int SelectedTrack { get; set; }
}

public class LavalinkLoadException
{
    public string Message { get; set; }
    public string Severity { get; set; }
    public string Cause { get; set; }
}

public class LavalinkLoadResult
{
    /// <summary>
    /// The type of the result
    /// </summary>
    [JsonProperty("loadType")]
    public LavalinkLoadType LoadType { get; set; }
    
    /// <summary>
    /// All tracks which have been loaded	
    /// </summary>
    [JsonProperty("tracks")]
    public LavalinkTrack[] Tracks { get; set; }
    
    /// <summary>
    /// Additional info if the the load type is <see cref="LavalinkLoadType.TrackLoaded"/>
    /// </summary>
    [JsonProperty("playlistInfo")]
    public LavalinkPlaylistInfo PlaylistInfo { get; set; }
    
    /// <summary>
    /// The Exception this load failed with	
    /// </summary>
    [JsonProperty("exception")]
    public LavalinkLoadException? Exception { get; set; }
}

public class LavalinkTrackInfo
{
    /// <summary>
    /// The track identifier
    /// </summary>
    [JsonProperty("identifier")]
    public string Identifier { get; set; }
    
    /// <summary>
    /// Whether the track is seekable
    /// </summary>
    [JsonProperty("isSeekable")]
    public bool IsSeekable { get; set; }
    
    /// <summary>
    /// The track author
    /// </summary>
    [JsonProperty("author")]
    public string Author { get; set; }
    
    /// <summary>
    /// The track length in milliseconds
    /// </summary>
    [JsonProperty("length")]
    public int Length { get; set; }
    
    /// <summary>
    /// Whether the track is a stream
    /// </summary>
    [JsonProperty("isStream")]
    public bool IsStream { get; set; }
    
    /// <summary>
    /// The track position in milliseconds
    /// </summary>
    [JsonProperty("position")]
    public string Position { get; set; }
    
    /// <summary>
    /// The track title
    /// </summary>
    [JsonProperty("title")]
    public string Title { get; set; }
    
    /// <summary>
    /// The track uri
    /// </summary>
    [JsonProperty("uri")]
    public string Uri { get; set; }
    
    /// <summary>
    /// The track source name
    /// </summary>
    [JsonProperty("sourceName")]
    public string SourceName { get; set; }

    public override string ToString() => $"[{SourceName}] {Title} by {Author}";
}