using Newtonsoft.Json;

namespace Nomia.Entities.Filters;

/// <summary>
/// Distortion effect. It can generate some pretty unique audio effects.
/// </summary>
public class LavalinkDistortionFilter : ILavalinkFilter
{
    /// <summary>
    /// The distortion level (0 to 1.0 where 0.0 is no effect and 1.0 is full effect)
    /// </summary>
    [JsonProperty("sinOffset")]
    public float? SinOffset { get; set; }

    /// <summary>
    /// The distortion level (0 to 1.0 where 0.0 is no effect and 1.0 is full effect)
    /// </summary>
    [JsonProperty("sinScale")]
    public float? SinScale { get; set; }

    /// <summary>
    /// The distortion level (0 to 1.0 where 0.0 is no effect and 1.0 is full effect)
    /// </summary>
    [JsonProperty("cosOffset")]
    public float? CosOffset { get; set; }

    /// <summary>
    /// The distortion level (0 to 1.0 where 0.0 is no effect and 1.0 is full effect)
    /// </summary>
    [JsonProperty("cosScale")]
    public float? CosScale { get; set; }

    /// <summary>
    /// The distortion level (0 to 1.0 where 0.0 is no effect and 1.0 is full effect)
    /// </summary>
    [JsonProperty("tanOffset")]
    public float? TanOffset { get; set; }

    /// <summary>
    /// The distortion level (0 to 1.0 where 0.0 is no effect and 1.0 is full effect)
    /// </summary>
    [JsonProperty("tanScale")]
    public float? TanScale { get; set; }

    /// <summary>
    /// The distortion level (0 to 1.0 where 0.0 is no effect and 1.0 is full effect)
    /// </summary>
    [JsonProperty("offset")]
    public float? Offset { get; set; }

    /// <summary>
    /// The distortion level (0 to 1.0 where 0.0 is no effect and 1.0 is full effect)
    /// </summary>
    [JsonProperty("scale")]
    public float? Scale { get; set; }

    public LavalinkDistortionFilter(float? sinOffset = 0, float? sinScale = 0, float? cosOffset = 0,
        float? cosScale = 0, float? tanOffset = 0, float? tanScale = 0, float? offset = 0, float? scale = 0)
    {
        SinOffset = sinOffset;
        SinScale = sinScale;
        CosOffset = cosOffset;
        CosScale = cosScale;
        TanOffset = tanOffset;
        TanScale = tanScale;
        Offset = offset;
        Scale = scale;
    }

    public void Reset() => SinOffset = SinScale =
        CosOffset = CosScale = TanOffset = TanScale = Offset = Scale = 0;
}