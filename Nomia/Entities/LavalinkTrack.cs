using System;
using System.Collections.Generic;
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
    [EnumMember(Value = "track")]
    TrackLoaded,
    
    /// <summary>
    /// Playlist loaded
    /// </summary>
    [EnumMember(Value = "playlist")]
    PlaylistLoaded,
    
    /// <summary>
    /// No matches found
    /// </summary>
    [EnumMember(Value = "empty")]
    NoMatches,
    
    /// <summary>
    /// Load failed
    /// </summary>
    [EnumMember(Value = "error")]
    LoadFailed,
    
    /// <summary>
    /// Search result
    /// </summary>
    [EnumMember(Value = "search")]
    SearchResult
}

public enum LavalinkTrackEndReason
{
    /// <summary>
    /// Track has finished playing
    /// </summary>
    [EnumMember(Value = "finished")]
    Finished,
    
    /// <summary>
    /// Load failed
    /// </summary>
    [EnumMember(Value = "loadFailed")]
    LoadFailed,
    
    /// <summary>
    /// Track has been stopped
    /// </summary>
    [EnumMember(Value = "stopped")]
    Stopped,
    
    /// <summary>
    /// Track has been replaced
    /// </summary>
    [EnumMember(Value = "replaced")]
    Replaced,
    
    /// <summary>
    /// Track has been cleaned up
    /// </summary>
    [EnumMember(Value = "cleanup")]
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
    
    /// <summary>
    /// Additional track info provided by plugins
    /// </summary>
    [JsonProperty("pluginInfo")]
    public object PluginInfo { get; set; }
    
    /// <summary>
    /// Additional track data provided via the <see cref="https://lavalink.dev/api/rest#update-player">Update Player</see> endpoint
    /// </summary>
    [JsonProperty("userData")]
    public object UserData { get; set; }
}

public class LavalinkPlaylistInfo {
    /// <summary>
    /// Name of playlist
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }
    /// <summary>
    /// 0-indexed index of selected track
    /// </summary>
    [JsonProperty("selectedTrack")]
    public int SelectedTrack { get; set; }
}

/// <summary>
/// Lavalink exception object.
/// </summary>
public class LavalinkException
{
    /// <summary>
    /// The message of the exception
    /// </summary>
    [JsonProperty("message")]
    public string Message { get; set; }
    /// <summary>
    /// The severity of the exception
    /// </summary>
    [JsonProperty("severity")]
    public string Severity { get; set; }
    /// <summary>
    /// The cause of the exception
    /// </summary>
    [JsonProperty("cause")]
    public string Cause { get; set; }
}

public class LavalinkTrackLoadedResult
{
    [JsonProperty("encoded")]
    public String Encoded { get; }
    
    [JsonProperty("info")]
    public LavalinkTrackInfo Info { get; }
}

/// <summary>
/// Loadable response type from lavalink
/// </summary>
public class LavalinkLoadable
{
    /// <summary>
    /// The type of the result
    /// </summary>
    [JsonProperty("loadType")]
    public LavalinkLoadType LoadType { get; set; }
}

/// <summary>
/// Lavalink load type that contains search response
/// </summary>
public class LavalinkSearchLoadedType : LavalinkLoadable
{
    [JsonProperty("data")]
    public IReadOnlyList<LavalinkTrack> Tracks { get; set; }
}


public class LavalinkPlaylistLoadedData : LavalinkLoadable
{
    /// <summary>
    /// Information about loaded playlist
    /// </summary>
    [JsonProperty("info")]
    public LavalinkPlaylistInfo Info { get; set; }
    /// <summary>
    /// Additional information from plugins
    /// </summary>
    [JsonProperty("pluginInfo")]
    public object PluginInfo { get; set; }
    /// <summary>
    /// Loaded tracks from playlist
    /// </summary>
    [JsonProperty("tracks")]
    public IReadOnlyList<LavalinkTrack> Tracks { get; set; }
}

public class LavalinkPlaylistLoadedType : LavalinkLoadable
{
    /// <summary>
    /// Information about loaded playlist
    /// </summary>
    [JsonProperty("data")]
    public LavalinkPlaylistLoadedData Data { get; set; }
}

/// <summary>
/// Contains response of successful track loading
/// </summary>
public class LavalinkTrackLoadedType : LavalinkLoadable
{
    /// <summary>
    /// Loaded track.
    /// </summary>
    [JsonProperty("data")]
    public LavalinkTrack Data { get; set; }
}

/// <summary>
/// Lavalink object that contains exception of loaded track
/// </summary>
public class LavalinkLoadFailedType : LavalinkLoadable
{
    /// <summary>
    /// Exception that was thrown by lavalink.
    /// </summary>
    [JsonProperty("data")]
    public LavalinkException Data { get; set; }
}

/// <summary>
/// Lavalink object with no data
/// </summary>
public class LavalinkEmptyLoadType : LavalinkLoadable
{
}

public class LavalinkLoadResult
{
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

    [JsonProperty("position")] internal long _position;
    
    /// <summary>
    /// The track position
    /// </summary>
    public TimeSpan Position => TimeSpan.FromMilliseconds(_position);
    
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
    /// The track artwork url
    /// </summary>
    [JsonProperty("artworkUrl")]
    public string ArtworkUrl { get; set; }
    
    /// <summary>
    /// The track <see cref="https://en.wikipedia.org/wiki/International_Standard_Recording_Code">ISRC</see>
    /// </summary>
    [JsonProperty("isrc")]
    public string Isrc { get; set; }
    
    /// <summary>
    /// The track source name
    /// </summary>
    [JsonProperty("sourceName")]
    public string SourceName { get; set; }
    
    public override string ToString() => $"[{SourceName}] {Title} by {Author}";
}