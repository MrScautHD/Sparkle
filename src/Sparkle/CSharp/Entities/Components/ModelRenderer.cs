using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Geometry;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.Scenes;
using Veldrid;

namespace Sparkle.CSharp.Entities.Components;

public class ModelRenderer : InterpolatedComponent {

    /// <summary>
    /// The model to be rendered.
    /// </summary>
    public Model Model { get; private set; }
    
    /// <summary>
    /// The sampler used for texturing the model, can be null.
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
    /// Indicates whether the bounding box around the model should be rendered.
    /// </summary>
    public bool DrawBoundingBox;
    
    /// <summary>
    /// The color applied to the model. Defaults to white if not provided.
    /// </summary>
    public Color ModelColor;

    /// <summary>
    /// The color used for rendering the bounding box of the model.
    /// </summary>
    public Color BoxColor;

    /// <summary>
    /// The bounding box of the model used for visibility checks.
    /// </summary>
    private BoundingBox _box;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelRenderer"/> class.
    /// </summary>
    /// <param name="model">The 3D model to render.</param>
    /// <param name="offsetPosition">The position offset relative to the parent entity.</param>
    /// <param name="sampler">Optional texture sampler to apply to the model.</param>
    /// <param name="depthStencilState">Optional depth/stencil configuration. Defaults to depth-only testing.</param>
    /// <param name="rasterizerState">Optional rasterization configuration. Defaults to solid fill with back-face culling.</param>
    /// <param name="drawBoundingBox">If true, renders the bounding box around the model.</param>
    /// <param name="modelColor">Optional color to tint the model. Defaults to white.</param>
    /// <param name="boxColor">Optional color for the bounding box. Defaults to white.</param>
    public ModelRenderer(Model model, Vector3 offsetPosition, Sampler? sampler = null, DepthStencilStateDescription? depthStencilState = null, RasterizerStateDescription? rasterizerState = null, bool drawBoundingBox = false, Color? modelColor = null, Color? boxColor = null) : base(offsetPosition) {
        this.Model = model;
        this.Sampler = sampler;
        this.DepthStencilState = depthStencilState ?? DepthStencilStateDescription.DEPTH_ONLY_LESS_EQUAL;
        this.RasterizerState = rasterizerState ?? RasterizerStateDescription.DEFAULT;
        this.DrawBoundingBox = drawBoundingBox;
        this.ModelColor = modelColor ?? Color.White;
        this.BoxColor = boxColor ?? Color.White;
        this._box = model.BoundingBox;
    }

    /// <summary>
    /// Updates the bounding box of the model based on its position and dimensions.
    /// </summary>
    /// <param name="delta">The time delta since the last update.</param>
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
    /// Renders the model associated with this component to the specified framebuffer if it is within the camera frustum.
    /// </summary>
    /// <param name="context">The graphics context used for rendering.</param>
    /// <param name="framebuffer">The framebuffer to render into.</param>
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
            
            // Draw the model.
            this.Model.Draw(context.CommandList, transform, framebuffer.OutputDescription, this.Sampler, this.DepthStencilState, this.RasterizerState, this.ModelColor);

            // Draw the bounding box.
            if (this.DrawBoundingBox) {
                context.ImmediateRenderer.DrawBoundingBox(context.CommandList, framebuffer.OutputDescription, new Transform(), this._box, this.BoxColor);
            }
        }
    }
}