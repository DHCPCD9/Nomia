using System.Runtime.Serialization;

namespace Nomia;

public enum LavalinkEventType
{
    /// <summary>
    /// Track started
    /// </summary>
    [EnumMember(Value = "TrackStartEvent")]
    TrackStartEvent,
    
    /// <summary>
    /// Track ended
    /// </summary>
    [EnumMember(Value = "TrackEndEvent")]
    TrackEndEvent,
    
    /// <summary>
    /// Track exception
    /// </summary>
    [EnumMember(Value = "TrackExceptionEvent")]
    TrackExceptionEvent,
    
    /// <summary>
    /// Track stuck
    /// </summary>
    [EnumMember(Value = "TrackStuckEvent")]
    TrackStuckEvent,
    
    /// <summary>
    /// Websocket closed
    /// </summary>
    [EnumMember(Value = "WebSocketClosedEvent")]
    WebSocketClosedEvent,
}