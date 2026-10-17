using System.Numerics;
using System.Runtime.InteropServices;

namespace Sparkle.CSharp.Graphics.Rendering.Forward.Fog;

[StructLayout(LayoutKind.Sequential)]
public struct ExponentialHeightFogData {
    
    /// <summary>
    /// RGB = fog color, W = fog intensity.
    /// </summary>
    public Vector4 Color;
    
    /// <summary>
    /// X = density, Y = height falloff, Z = base height, W = start distance.
    /// </summary>
    public Vector4 Parameters;
    
    /// <summary>
    /// X = end distance, YZW = padding.
    /// </summary>
    public Vector4 DistanceParameters;
    
    /// <summary>
    /// Camera world position. W = sky fog distance.
    /// </summary>
    public Vector4 CameraPosition;
    
    /// <summary>
    /// Inverse projection matrix of the active camera.
    /// </summary>
    public Matrix4x4 InverseProjection;
    
    /// <summary>
    /// Inverse view matrix of the active camera.
    /// </summary>
    public Matrix4x4 InverseView;
    
    /// <summary>
    /// Inverse view-projection matrix of the active camera.
    /// </summary>
    public Matrix4x4 InverseViewProjection;
}