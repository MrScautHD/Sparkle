using System.Numerics;
using System.Runtime.InteropServices;

namespace Sparkle.CSharp.Graphics.Rendering.Forward.Pbr.Data;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ShadowData {
    
    /// <summary>
    /// The resolution of the shadow map texture used for shadow rendering.
    /// </summary>
    public float ShadowMapResolution;
    
    /// <summary>
    /// The constant depth bias applied when comparing fragment depth against the shadow map, used to reduce shadow acne.
    /// </summary>
    public float ShadowBias;
    
    /// <summary>
    /// The bias applied along the surface normal when sampling the shadow map, used to reduce shadow acne on surfaces at grazing angles.
    /// </summary>
    public float ShadowNormalBias;
    
    /// <summary>
    /// The maximum allowed value for the total depth bias, preventing excessive offsets that would cause peter-panning.
    /// </summary>
    public float ShadowBiasClamp;
    
    /// <summary>
    /// The ambient light color applied to the shadowed scene where intensity is stored in the W channel.
    /// </summary>
    public Vector4 AmbientColor;
    
    /// <summary>
    /// The combined view and projection matrix of the light used for shadow sampling.
    /// </summary>
    public Matrix4x4 LightVP;
    
    /// <summary>
    /// The world space direction of the shadow casting light, stored as a <see cref="Vector4"/> for GPU alignment.
    /// </summary>
    public Vector4 LightDirection;
    
    /// <summary>
    /// The color and intensity of the shadow casting light where intensity is stored in the W channel.
    /// </summary>
    public Vector4 LightColor;
}