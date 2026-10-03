using Sparkle.CSharp.Entities.Components;

namespace Sparkle.CSharp.Graphics.Rendering.Forward;

public interface ILightReceiver {
    
    /// <summary>
    /// Submits a light to the renderer for processing or rendering.
    /// </summary>
    /// <param name="light">The light instance to be submitted. This can include various properties such as color, intensity, range, and type of light.</param>
    void SubmitLight(Light light);
}