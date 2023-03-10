using Newtonsoft.Json;

namespace Nomia.Entities.Filters;

/// <summary>
/// Changes the speed, pitch, and rate. All default to 1.0.
/// </summary>
public class LavalinkTimescaleFilter : ILavalinkFilter
{
    /// <summary>
    /// Playback speed (0.5 to 2.0 where 1.0 is normal speed)
    /// </summary>
    [JsonProperty("speed")]
    public float? Speed { get; set; }
    /// <summary>
    /// The pitch (0.5 to 2.0 where 1.0 is normal pitch)
    /// </summary>
    [JsonProperty("pitch")]
    public float? Pitch { get; set; }
    /// <summary>
    /// The rate (0.5 to 2.0 where 1.0 is normal rate)
    /// </summary>
    [JsonProperty("rate")]
    public float? Rate { get; set; }

    public LavalinkTimescaleFilter(float speed = 1, float pitch = 1, float rate = 1)
    {
        Speed = speed;
        Pitch = pitch;
        Rate = rate;
    }

    public void Reset() => Speed = Pitch = Rate = 1.0f;
}