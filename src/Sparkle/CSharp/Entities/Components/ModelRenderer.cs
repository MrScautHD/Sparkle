using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Geometry;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.Scenes;
using Veldrid;

namespace Sparkle.CSharp.Entities.Components;

public class ModelRenderer : Component {

    /// <summary>
    /// The model to be rendered.
    /// </summary>
    public Model Model { get; private set; }
    
    /// <summary>
    /// The sampler used for texturing the model, can be null.
    /// </summary>
    public Sampler? Sampler;
    
    /// <summary>
    /// The color applied to the model. Defaults to white if not provided.
    /// </summary>
    public Color Color;
    
    /// <summary>
    /// Indicates whether the model should be rendered as a wireframe.
    /// </summary>
    public bool DrawWires;

    /// <summary>
    /// The bounding box of the model used for visibility checks.
    /// </summary>
    private BoundingBox _box;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ModelRenderer"/> class.
    /// </summary>
    /// <param name="model">The model to render.</param>
    /// <param name="offsetPos">The position offset of the model relative to its parent entity.</param>
    /// <param name="sampler">Optional sampler to apply to the model's textures.</param>
    /// <param name="drawWires">Whether to draw the model in wireframe mode.</param>
    /// <param name="color">Optional color to apply to the model.</param>
    public ModelRenderer(Model model, Vector3 offsetPos, Sampler? sampler = null, bool drawWires = false, Color? color = null) : base(offsetPos) {
        this.Model = model;
        this.Sampler = sampler;
        this.Color = color ?? Color.White;
        this.DrawWires = drawWires;
        this._box = model.BoundingBox;
    }
    
    /// <summary>
    /// Updates the bounding box of the model based on its position and dimensions.
    /// </summary>
    /// <param name="delta">The time delta since the last update.</param>
    protected internal override void Update(double delta) {
        base.Update(delta);
        
        Vector3 dimension = this._box.Max - this._box.Min;
        this._box.Min.X = this.GlobalPos.X - dimension.X / 2;
        this._box.Min.Y = this.GlobalPos.Y;
        this._box.Min.Z = this.GlobalPos.Z - dimension.Z / 2;
        
        this._box.Max.X = this.GlobalPos.X + dimension.X / 2;
        this._box.Max.Y = this.GlobalPos.Y + dimension.Y;
        this._box.Max.Z = this.GlobalPos.Z + dimension.Z / 2;
    }

    protected internal override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        base.Draw(context, framebuffer);
        Camera3D? cam3D = SceneManager.ActiveCam3D;
        
        if (cam3D == null) {
            return;
        }
        
        if (cam3D.GetFrustum().ContainsOrientedBox(this._box, this.GlobalPos, this.Entity.Transform.Rotation)) {
            RasterizerStateDescription? rasterizerState = null;
            
            if (this.DrawWires) {
                rasterizerState = RasterizerStateDescription.DEFAULT with {
                    CullMode = FaceCullMode.None,
                    FillMode = PolygonFillMode.Wireframe
                };
            }
            
            this.Model.Draw(context.CommandList, new Transform() { Translation = this.GlobalPos }, framebuffer.OutputDescription, this.Sampler, null, rasterizerState, this.Color);
        }
    }
}