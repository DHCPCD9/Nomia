using Newtonsoft.Json;

namespace Nomia.Entities.Filters;

/// <summary>
/// Uses amplification to create a shuddering effect, where the volume quickly oscillates. https://en.wikipedia.org/wiki/File:Fuse_Electronics_Tremolo_MK-III_Quick_Demo.ogv
/// </summary>
public class LavalinkTremoloFilter : ILavalinkFilter
{
    /// <summary>
    /// The tremolo depth (0 to 1.0 where 0.0 is no effect and 1.0 is full effect)
    /// </summary>
    [JsonProperty("depth")]
    public float? Depth { get; set; }

    /// <summary>
    /// The tremolo frequency (0 to 20.0 where 0.0 is no effect and 20.0 is full effect)
    /// </summary>
    [JsonProperty("frequency")]
    public float? Frequency { get; set; }

    public LavalinkTremoloFilter(float? depth = 0, float? frequency = 0)
    {
        Depth = depth;
        Frequency = frequency;
    }

    public void Reset() => (Depth, Frequency) = (0, 0);
}