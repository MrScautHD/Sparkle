using System.Numerics;
using System.Runtime.InteropServices;

namespace Sparkle.CSharp.Graphics.Rendering.Forward.Reflections;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SsrData {
    
    /// <summary>
    /// X = framebuffer width, Y = framebuffer height.
    /// </summary>
    public Vector2 ScreenSize;
    
    /// <summary>
    /// Maximum ray-march distance in view/world units.
    /// </summary>
    public float MaxDistance;
    
    /// <summary>
    /// Maximum number of ray-march steps.
    /// Stored as float for uniform-buffer alignment simplicity.
    /// </summary>
    public float MaxSteps;
    
    /// <summary>
    /// Ray step length multiplier.
    /// </summary>
    public float StepSize;
    
    /// <summary>
    /// Thickness used when comparing ray depth against scene depth.
    /// </summary>
    public float Thickness;
    
    /// <summary>
    /// Overall SSR reflection strength.
    /// </summary>
    public float Strength;
    
    /// <summary>
    /// Edge fade size. Higher values hide SSR edges more aggressively.
    /// </summary>
    public float EdgeFade;
    
    /// <summary>
    /// Projection matrix of the active camera.
    /// </summary>
    public Matrix4x4 Projection;
    
    /// <summary>
    /// View matrix of the active camera.
    /// </summary>
    public Matrix4x4 View;
    
    /// <summary>
    /// View-projection matrix.
    /// </summary>
    public Matrix4x4 ViewProjection;
    
    /// <summary>
    /// Inverse projection matrix of the active camera.
    /// </summary>
    public Matrix4x4 InverseProjection;
    
    /// <summary>
    /// Inverse view matrix of the active camera.
    /// </summary>
    public Matrix4x4 InverseView;
    
    /// <summary>
    /// Inverse view-projection matrix.
    /// </summary>
    public Matrix4x4 InverseViewProjection;
}