using Newtonsoft.Json;

namespace Nomia.Entities;

public class LavalinkNodeReadyPayload
{
    [JsonProperty("sessionId")] public string SessionId { get; set; }
    [JsonProperty("resumed")] public bool Resumed { get; set; }
}