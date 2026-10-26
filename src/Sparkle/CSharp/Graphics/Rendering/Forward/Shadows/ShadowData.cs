using System.Numerics;
using System.Runtime.InteropServices;

namespace Sparkle.CSharp.Graphics.Rendering.Forward.Shadows;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ShadowData {
    
    /// <summary>
    /// The resolution of each shadow map cascade layer.
    /// </summary>
    public float ShadowMapResolution;
    
    /// <summary>
    /// The constant depth bias applied when comparing fragment depth against the shadow map.
    /// </summary>
    public float ShadowBias;
    
    /// <summary>
    /// The bias applied along the surface normal when sampling the shadow map.
    /// </summary>
    public float ShadowNormalBias;
    
    /// <summary>
    /// The maximum allowed value for the total depth bias.
    /// </summary>
    public float ShadowBiasClamp;
    
    /// <summary>
    /// The ambient light color applied to the shadowed scene where intensity is stored in the W channel.
    /// </summary>
    public Vector4 AmbientColor;
    
    /// <summary>
    /// Cascade 0 light view-projection matrix.
    /// </summary>
    public Matrix4x4 LightVP0;
    
    /// <summary>
    /// Cascade 1 light view-projection matrix.
    /// </summary>
    public Matrix4x4 LightVP1;
    
    /// <summary>
    /// Cascade 2 light view-projection matrix.
    /// </summary>
    public Matrix4x4 LightVP2;
    
    /// <summary>
    /// Cascade 3 light view-projection matrix.
    /// </summary>
    public Matrix4x4 LightVP3;
    
    /// <summary>
    /// View-space far split distance for each cascade.
    /// </summary>
    public Vector4 CascadeSplits;
    
    /// <summary>
    /// The world space direction of the shadow casting light.
    /// </summary>
    public Vector4 LightDirection;
    
    /// <summary>
    /// The color and intensity of the shadow casting light where intensity is stored in the W channel.
    /// </summary>
    public Vector4 LightColor;
}