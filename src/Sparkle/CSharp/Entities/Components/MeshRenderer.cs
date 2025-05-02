using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Geometry;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.Scenes;
using Veldrid;

namespace Sparkle.CSharp.Entities.Components;

public class MeshRenderer : InterpolatedComponent {
    
    /// <summary>
    /// The 3D mesh to render.
    /// </summary>
    public Mesh Mesh { get; private set; }
    
    /// <summary>
    /// The sampler used for texture sampling. Can be null.
    /// </summary>
    public Sampler? Sampler;
    
    /// <summary>
    /// Defines the depth and stencil testing behavior during rendering.
    /// </summary>
    public DepthStencilStateDescription DepthStencilState;
    
    /// <summary>
    /// Defines rasterization settings such as fill mode and culling.
    /// </summary>
    public RasterizerStateDescription RasterizerState;
    
    /// <summary>
    /// Whether to draw the bounding box around the mesh.
    /// </summary>
    public bool DrawBoundingBox;
    
    /// <summary>
    /// The color used for rendering the mesh.
    /// </summary>
    public Color MeshColor;
    
    /// <summary>
    /// The color used for rendering the bounding box.
    /// </summary>
    public Color BoxColor;

    /// <summary>
    /// The mesh's axis-aligned bounding box.
    /// </summary>
    private BoundingBox _box;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MeshRenderer"/> class.
    /// </summary>
    /// <param name="mesh">The mesh to render.</param>
    /// <param name="offsetPosition">The position offset relative to the parent entity.</param>
    /// <param name="sampler">Optional texture sampler for the mesh.</param>
    /// <param name="depthStencilState">Optional depth/stencil settings. Defaults to depth-only testing.</param>
    /// <param name="rasterizerState">Optional rasterizer configuration. Defaults to solid fill with back-face culling.</param>
    /// <param name="drawBoundingBox">If true, the mesh’s bounding box will be rendered.</param>
    /// <param name="meshColor">Optional tint color for the mesh. Defaults to white.</param>
    /// <param name="boxColor">Optional color for the bounding box. Defaults to white.</param>
    public MeshRenderer(Mesh mesh, Vector3 offsetPosition, Sampler? sampler = null, DepthStencilStateDescription? depthStencilState = null, RasterizerStateDescription? rasterizerState = null, bool drawBoundingBox = false, Color? meshColor = null, Color? boxColor = null) : base(offsetPosition) {
        this.Mesh = mesh;
        this.Sampler = sampler;
        this.DepthStencilState = depthStencilState ?? DepthStencilStateDescription.DEPTH_ONLY_LESS_EQUAL;
        this.RasterizerState = rasterizerState ?? RasterizerStateDescription.DEFAULT;
        this.DrawBoundingBox = drawBoundingBox;
        this.MeshColor = meshColor ?? Color.White;
        this.BoxColor = boxColor ?? Color.White;
        this._box = mesh.BoundingBox;
    }
    
    /// <summary>
    /// Updates the bounding box to reflect the current interpolated position.
    /// </summary>
    /// <param name="delta">Time elapsed since the last update call.</param>
    protected internal override void Update(double delta) {
        base.Update(delta);
        
        // Calculate bounding box.
        Vector3 dimension = this._box.Max - this._box.Min;
        Vector3 lerpedPos = this.LerpedGlobalPosition;
        this._box.Min.X = lerpedPos.X - dimension.X / 2.0F;
        this._box.Min.Y = lerpedPos.Y;
        this._box.Min.Z = lerpedPos.Z - dimension.Z / 2.0F;
        
        this._box.Max.X = lerpedPos.X + dimension.X / 2.0F;
        this._box.Max.Y = lerpedPos.Y + dimension.Y;
        this._box.Max.Z = lerpedPos.Z + dimension.Z / 2.0F;
    }
    
    /// <summary>
    /// Draws the mesh and optionally its wireframe and bounding box to the screen.
    /// </summary>
    /// <param name="context">The graphics context used for issuing draw commands.</param>
    /// <param name="framebuffer">The framebuffer to which the mesh should be rendered.</param>
    protected internal override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        base.Draw(context, framebuffer);
        Camera3D? cam3D = SceneManager.ActiveCam3D;
        
        if (cam3D == null) {
            return;
        }
        
        if (cam3D.GetFrustum().ContainsOrientedBox(this._box, this.LerpedGlobalPosition, this.LerpedRotation)) {
            Transform transform = new Transform() {
                Translation = this.LerpedGlobalPosition,
                Rotation = this.LerpedRotation,
                Scale = this.LerpedScale
            };
            
            // Draw the mesh.
            this.Mesh.Draw(context.CommandList, transform, framebuffer.OutputDescription, this.Sampler, this.DepthStencilState, this.RasterizerState, this.MeshColor);

            // Draw the bounding box.
            if (this.DrawBoundingBox) {
                context.ImmediateRenderer.DrawBoundingBox(context.CommandList, framebuffer.OutputDescription, new Transform(), this._box, this.BoxColor);
            }
        }
    }
}