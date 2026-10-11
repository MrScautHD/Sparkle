namespace Sparkle.CSharp.Graphics.Rendering.Forward.Reflections;

public enum ReflectionQuality {
    
    /// <summary>
    /// A quarter of the screen resolution (0.25x). Best performance, with the blurriest reflections.
    /// </summary>
    Low,
    
    /// <summary>
    /// Half of the screen resolution (0.5x). Balanced quality and performance.
    /// </summary>
    Medium,
    
    /// <summary>
    /// Three quarters of the screen resolution (0.75x). Sharper reflections at a higher cost.
    /// </summary>
    High,
    
    /// <summary>
    /// The full screen resolution (1.0x). Sharpest reflections and the highest cost.
    /// </summary>
    Ultra
}