using Newtonsoft.Json;

namespace Nomia.Entities.Filters;

/// <summary>
/// Uses equalization to eliminate part of a band, usually targeting vocals.
/// </summary>
public class LavalinkKaraokeFilter : ILavalinkFilter
{

    /// <summary>
    /// The level (0 to 1.0 where 0.0 is no effect and 1.0 is full effect)
    /// </summary>
    [JsonProperty("level")]
    public float? Level { get; set; }
    /// <summary>
    /// The mono level (0 to 1.0 where 0.0 is no effect and 1.0 is full effect)
    /// </summary>
    [JsonProperty("monoLevel")]
    public float? MonoLevel { get; set; }
    /// <summary>
    /// The filter band
    /// </summary>
    [JsonProperty("filterBand")]
    public float? FilterBand { get; set; }
    /// <summary>
    /// The filter width
    /// </summary>
    [JsonProperty("filterWidth")]
    public float? FilterWidth { get; set; }

    public LavalinkKaraokeFilter(float level = 0, float monoLevel = 0, float filterBand = 0, float filterWidth = 0)
    {
        Level = level;
        MonoLevel = monoLevel;
        FilterBand = filterBand;
        FilterWidth = filterWidth;
    }

    public void Reset() => (Level, MonoLevel, FilterBand, FilterWidth) = (0, 0, 0, 0);
}