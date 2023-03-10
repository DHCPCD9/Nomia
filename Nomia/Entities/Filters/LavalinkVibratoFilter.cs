using Newtonsoft.Json;

namespace Nomia.Entities.Filters;

/// <summary>
/// Similar to tremolo. While tremolo oscillates the volume, vibrato oscillates the pitch.
/// </summary>
public class LavalinkVibratoFilter : ILavalinkFilter
{
    /// <summary>
    /// The vibrato depth (0 to 1.0 where 0.0 is no effect and 1.0 is full effect)
    /// </summary>
    [JsonProperty("depth")]
    public float? Depth { get; set; }
    /// <summary>
    /// The vibrato frequency (0 to 14.0 where 0.0 is no effect and 14.0 is full effect)
    /// </summary>
    [JsonProperty("frequency")]
    public float? Frequency { get; set; }

    public LavalinkVibratoFilter(float? depth = 0, float? frequency = 0)
    {
        Depth = depth;
        Frequency = frequency;
    }

    public void Reset() => (Depth, Frequency) = (0, 0);
}