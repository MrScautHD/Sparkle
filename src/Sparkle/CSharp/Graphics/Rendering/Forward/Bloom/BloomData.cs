using System.Numerics;
using System.Runtime.InteropServices;

namespace Sparkle.CSharp.Graphics.Rendering.Forward.Bloom;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct BloomData {
    
    /// <summary>
    /// X = source width, Y = source height.
    /// </summary>
    public Vector2 ScreenSize;
    
    /// <summary>
    /// Number of blur samples. Kept for API tuning, but the shader uses a fixed optimized kernel.
    /// </summary>
    public float Samples;
    
    /// <summary>
    /// Blur spread.
    /// </summary>
    public float Quality;
    
    /// <summary>
    /// Final bloom strength.
    /// </summary>
    public float Strength;
    
    /// <summary>
    /// Optional threshold. Values below this are suppressed.
    /// </summary>
    public float Threshold;
    
    /// <summary>
    /// Blur direction: (1, 0) horizontal, (0, 1) vertical.
    /// </summary>
    public Vector2 Direction;
}