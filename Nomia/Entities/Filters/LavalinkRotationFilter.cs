using Newtonsoft.Json;

namespace Nomia.Entities.Filters;

/// <summary>
/// Rotates the sound around the stereo channels/user headphones aka Audio Panning.
/// </summary>
public class LavalinkRotationFilter : ILavalinkFilter
{
    /// <summary>
    /// The frequency of the audio rotating around the listener in Hz.
    /// </summary>
    [JsonProperty("rotationHz")]
    public float? RotationHz { get; set; }

    public LavalinkRotationFilter(float? rotationHz = 0)
    {
        RotationHz = rotationHz;
    }

    public void Reset() => RotationHz = 0;
}