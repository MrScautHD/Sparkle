using System.Numerics;
using System.Runtime.InteropServices;

namespace Sparkle.CSharp.Graphics.Rendering.Forward.Lighting.Pbr;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PbrClusterData {
    
    /// <summary>
    /// X = framebuffer width, Y = framebuffer height.
    /// </summary>
    public Vector2 ScreenSize;
    
    /// <summary>
    /// Maximum amount of light indices each cluster can store.
    /// </summary>
    public uint MaxLightsPerCluster;
    
    /// <summary>
    /// Number of clusters on the X axis.
    /// </summary>
    public uint ClusterCountX;
    
    /// <summary>
    /// Number of clusters on the Y axis.
    /// </summary>
    public uint ClusterCountY;
    
    /// <summary>
    /// Number of depth slices.
    /// </summary>
    public uint ClusterCountZ;
    
    /// <summary>
    /// Camera near plane used for cluster depth slicing.
    /// </summary>
    public float NearPlane;
    
    /// <summary>
    /// Camera far plane used for cluster depth slicing.
    /// </summary>
    public float FarPlane;
    
    /// <summary>
    /// Active camera projection matrix.
    /// </summary>
    public Matrix4x4 Projection;
    
    /// <summary>
    /// Active camera view matrix.
    /// </summary>
    public Matrix4x4 View;
    
    /// <summary>
    /// Inverse active camera projection matrix.
    /// </summary>
    public Matrix4x4 InverseProjection;
    
    /// <summary>
    /// Inverse active camera view matrix.
    /// </summary>
    public Matrix4x4 InverseView;
}