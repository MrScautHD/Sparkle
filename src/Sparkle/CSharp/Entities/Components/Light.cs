using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Graphics.Rendering.Renderers;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.Graphics.Rendering.Forward;
using Sparkle.CSharp.Graphics.Rendering.Gizmos;
using Veldrith;

namespace Sparkle.CSharp.Entities.Components;

public class Light : InterpolatedComponent, IDebugDrawable {
    
    /// <summary>
    /// Whether the light is enabled.
    /// </summary>
    public bool Enabled;
    
    /// <summary>
    /// The type of the light (Directional, Point, or Spot).
    /// </summary>
    public LightType LightType;
    
    /// <summary>
    /// The color of the emitted light.
    /// </summary>
    public Color Color;
    
    /// <summary>
    /// The brightness of the light.
    /// </summary>
    public float Intensity;
    
    /// <summary>
    /// The maximum distance the light reaches (used for point and spot lights).
    /// </summary>
    public float Range;
    
    /// <summary>
    /// The inner cone angle of the spotlight, within which the light is at full intensity (used for spot lights).
    /// </summary>
    public float InnerSpotAngle;
    
    /// <summary>
    /// The outer cone angle of the spotlight, at which the light fades out completely (used for spot lights).
    /// </summary>
    public float OuterSpotAngle;
    
    /// <summary>
    /// Whether the light casts shadows.
    /// </summary>
    public bool CastShadows;
    
    /// <summary>
    /// Specifies whether debug visuals should be drawn for the object.
    /// </summary>
    public bool DebugDrawEnabled { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Light"/> class.
    /// </summary>
    /// <param name="offsetPosition">The position of the light relative to its entity.</param>
    /// <param name="lightType">The type of the light (Directional, Point, or Spot).</param>
    /// <param name="color">The color of the light. Defaults to <see cref="Color.White"/> if <c>null</c>.</param>
    /// <param name="intensity">The brightness of the light.</param>
    /// <param name="range">The maximum distance the light reaches (used for point and spot lights).</param>
    /// <param name="innerSpotAngle">The inner cone angle of the spotlight (used for spot lights).</param>
    /// <param name="outerSpotAngle">The outer cone angle of the spotlight (used for spot lights).</param>
    /// <param name="castShadows">Whether the light casts shadows.</param>
    public Light(Vector3 offsetPosition, LightType lightType, Color? color = null, float intensity = 1.0F, float range = 10.0F, float innerSpotAngle = 20.0F, float outerSpotAngle = 30.0F, bool castShadows = false) : base(offsetPosition) {
        this.Enabled = true;
        this.LightType = lightType;
        this.Color = color ?? Color.White;
        this.Intensity = intensity;
        this.Range = range;
        this.InnerSpotAngle = innerSpotAngle;
        this.OuterSpotAngle = outerSpotAngle;
        this.CastShadows = castShadows;
    }
    
    /// <summary>
    /// Creates a directional light, which illuminates the whole scene from a single direction and has no position or range.
    /// </summary>
    /// <param name="color">The color of the light.</param>
    /// <param name="intensity">The brightness of the light.</param>
    /// <returns>A new directional <see cref="Light"/>.</returns>
    public static Light CreateDirectional(Color color, float intensity = 1.0F) {
        return new Light(Vector3.Zero, LightType.Directional, color, intensity, 0.0F);
    }
    
    /// <summary>
    /// Creates a point light, which emits light in all directions from a single position.
    /// </summary>
    /// <param name="offsetPosition">The position of the light relative to its entity.</param>
    /// <param name="color">The color of the light.</param>
    /// <param name="intensity">The brightness of the light.</param>
    /// <param name="range">The maximum distance the light reaches.</param>
    /// <returns>A new point <see cref="Light"/>.</returns>
    public static Light CreatePoint(Vector3 offsetPosition, Color color, float intensity = 1.0F, float range = 10.0F) {
        return new Light(offsetPosition, LightType.Point, color, intensity, range);
    }
    
    /// <summary>
    /// Creates a spot light, which emits light in a cone from a single position.
    /// </summary>
    /// <param name="offsetPosition">The position of the light relative to its entity.</param>
    /// <param name="color">The color of the light.</param>
    /// <param name="intensity">The brightness of the light.</param>
    /// <param name="range">The maximum distance the light reaches.</param>
    /// <param name="innerSpotAngle">The inner cone angle, within which the light is at full intensity.</param>
    /// <param name="outerSpotAngle">The outer cone angle, at which the light fades out completely.</param>
    /// <returns>A new spot <see cref="Light"/>.</returns>
    public static Light CreateSpot(Vector3 offsetPosition, Color color, float intensity = 1.0F, float range = 10.0F, float innerSpotAngle = 20.0F, float outerSpotAngle = 30.0F) {
        return new Light(offsetPosition, LightType.Spot, color, intensity, range, innerSpotAngle, outerSpotAngle);
    }
    
    /// <summary>
    /// Draws the component and submits this light to the scene's renderer if it implements <see cref="ILightReceiver"/>.
    /// </summary>
    /// <param name="context">The graphics context used for drawing.</param>
    /// <param name="framebuffer">The framebuffer being rendered to.</param>
    protected internal override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        base.Draw(context, framebuffer);
        
        if (this.Entity.Scene.Renderer is ILightReceiver lightReceiver) {
            lightReceiver.SubmitLight(this);
        }
    }
    
    /// <summary>
    /// Renders debug visualization for the current light component using the specified renderer.
    /// </summary>
    /// <param name="immediateRenderer">The renderer used to draw debug primitives such as spheres and other shapes.</param>
    public void DrawDebug(ImmediateRenderer immediateRenderer) {
        Transform sphereTransform = new Transform() {
            Translation = this.LerpedGlobalPosition,
            Rotation = this.LerpedRotation,
            Scale = this.LerpedScale
        };
        
        immediateRenderer.DrawSphere(sphereTransform, 2, 20, 20, this.Color);
    }
    
    protected override void Dispose(bool disposing) { }
}