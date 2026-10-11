using System.Numerics;
using System.Runtime.InteropServices;

namespace Sparkle.CSharp.Graphics.Rendering.Forward.Environment;

[StructLayout(LayoutKind.Sequential)]
public struct EnvironmentData {
    
    /// <summary>
    /// Maximum mip level of the prefiltered environment map.
    /// </summary>
    public float MaxReflectionLod;
    
    /// <summary>
    /// Padding for memory alignment to ensure the correct GPU struct layout.
    /// </summary>
    private Vector3 _padding;
    
    /// <summary>
    /// Environment color tint. W stores intensity.
    /// </summary>
    public Vector4 EnvironmentColor;
}