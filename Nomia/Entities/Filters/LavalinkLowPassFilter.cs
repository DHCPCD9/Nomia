using Newtonsoft.Json;

namespace Nomia.Entities.Filters;

/// <summary>
/// Higher frequencies get suppressed, while lower frequencies pass through this filter, thus the name low pass. Any smoothing values equal to, or less than 1.0 will disable the filter.
/// </summary>
public class LavalinkLowPassFilter : ILavalinkFilter
{
    /// <summary>
    /// The smoothing factor (0 to 1.0 where 0.0 is no effect and 1.0 is full effect)
    /// </summary>
    [JsonProperty("smoothing")]
    public float? Smoothing { get; set; }

    public LavalinkLowPassFilter(float? smoothing = 0)
    {
        Smoothing = smoothing;
    }

    public void Reset() => Smoothing = 0;
}