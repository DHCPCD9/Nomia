using Newtonsoft.Json;

namespace Nomia.Entities.Filters;

/// <summary>
/// Mixes both channels (left and right), with a configurable factor on how much each channel affects the other. With the defaults, both channels are kept independent of each other. Setting all factors to 0.5 means both channels get the same audio.
/// </summary>
public class LavalinkChannelMixFilter : ILavalinkFilter
{
    /// <summary>
    /// The left channel volume (0 to 1.0 where 0.0 is no effect and 1.0 is full effect)
    /// </summary>
    [JsonProperty("leftToLeft")]
    public float? LeftToLeft { get; set; }
    /// <summary>
    /// The left channel volume (0 to 1.0 where 0.0 is no effect and 1.0 is full effect)
    /// </summary>
    [JsonProperty("leftToRight")]
    public float? LeftToRight { get; set; }
    /// <summary>
    /// The right channel volume (0 to 1.0 where 0.0 is no effect and 1.0 is full effect)
    /// </summary>
    [JsonProperty("rightToRight")]
    public float? RightToRight { get; set; }
    /// <summary>
    /// The right channel volume (0 to 1.0 where 0.0 is no effect and 1.0 is full effect)
    /// </summary>
    [JsonProperty("rightToLeft")]
    public float? RightToLeft { get; set; }

    public LavalinkChannelMixFilter(float? leftToLeft = 0, float? leftToRight = 0, float? rightToRight = 0, float? rightToLeft = 0)
    {
        this.LeftToLeft = leftToLeft;
        this.LeftToRight = leftToRight;
        this.RightToRight = rightToRight;
        this.RightToLeft = rightToLeft;
    }

    public void Reset() => (LeftToLeft, LeftToRight, RightToRight, RightToLeft) = (0, 0, 0, 0);
}