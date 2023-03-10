using Newtonsoft.Json;

namespace Nomia.Entities;

public class LavalinkNodeReadyPayload
{
    /// <summary>
    /// The session id of the node
    /// </summary>
    [JsonProperty("sessionId")] public string SessionId { get; set; }
    
    /// <summary>
    /// Whether the node has resumed
    /// </summary>
    [JsonProperty("resumed")] public bool Resumed { get; set; }
}