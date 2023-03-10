using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Nomia.Entities;

public enum LavalinkSearchType
{
    /// <summary>
    /// A search on YouTube
    /// </summary>
    [EnumMember(Value = "ytsearch")]
    Youtube,
    /// <summary>
    /// A search on Soundcloud
    /// </summary>
    [EnumMember(Value = "scsearch")]
    Soundcloud,
    /// <summary>
    /// Use a direct link to a track
    /// </summary>
    Raw
}

public enum LavalinkLoadType
{
    /// <summary>
    /// Track loaded
    /// </summary>
    [EnumMember(Value = "TRACK_LOADED")]
    TrackLoaded,
    
    /// <summary>
    /// Playlist loaded
    /// </summary>
    [EnumMember(Value = "PLAYLIST_LOADED")]
    PlaylistLoaded,
    
    /// <summary>
    /// No matches found
    /// </summary>
    [EnumMember(Value = "NO_MATCHES")]
    NoMatches,
    
    /// <summary>
    /// Load failed
    /// </summary>
    [EnumMember(Value = "LOAD_FAILED")]
    LoadFailed,
    
    /// <summary>
    /// Search result
    /// </summary>
    [EnumMember(Value = "SEARCH_RESULT")]
    SearchResult
}

public enum LavalinkTrackEndReason
{
    /// <summary>
    /// Track has finished playing
    /// </summary>
    [EnumMember(Value = "FINISHED")]
    Finished,
    
    /// <summary>
    /// Load failed
    /// </summary>
    [EnumMember(Value = "LOAD_FAILED")]
    LoadFailed,
    
    /// <summary>
    /// Track has been stopped
    /// </summary>
    [EnumMember(Value = "STOPPED")]
    Stopped,
    
    /// <summary>
    /// Track has been replaced
    /// </summary>
    [EnumMember(Value = "REPLACED")]
    Replaced,
    
    /// <summary>
    /// Track has been cleaned up
    /// </summary>
    [EnumMember(Value = "CLEANUP")]
    Cleanup
}

public class LavalinkTrack
{
    /// <summary>
    /// The track encoded as base64
    /// </summary>
    [JsonProperty("encoded")]
    public string Encoded { get; set; }

    /// <summary>
    /// The track info
    /// </summary>
    [JsonProperty("info")]
    public LavalinkTrackInfo Info { get; set; }
}

public class LavalinkPlaylistInfo {
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("selectedTrack")]
    public int SelectedTrack { get; set; }
}

public class LavalinkException
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
    public LavalinkException? Exception { get; set; }
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
    
    [JsonProperty("length")]
    internal long _length;
    
    /// <summary>
    /// The track length in milliseconds
    /// </summary>
    public TimeSpan Length => TimeSpan.FromMilliseconds(_length);
    
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