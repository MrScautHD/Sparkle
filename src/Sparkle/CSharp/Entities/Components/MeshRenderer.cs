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
    /// Whether to render the mesh as a wireframe.
    /// </summary>
    public bool DrawWires;
    
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
    /// <param name="offsetPos">The offset position of the component relative to the entity.</param>
    /// <param name="sampler">Optional texture sampler for the mesh.</param>
    /// <param name="drawWires">Whether to render the mesh in wireframe mode.</param>
    /// <param name="drawBoundingBox">Whether to render the mesh's bounding box.</param>
    /// <param name="meshColor">Optional color for the mesh. Defaults to white.</param>
    /// <param name="boxColor">Optional color for the bounding box. Defaults to white.</param>
    public MeshRenderer(Mesh mesh, Vector3 offsetPos, Sampler? sampler = null, bool drawWires = false, bool drawBoundingBox = false, Color? meshColor = null, Color? boxColor = null) : base(offsetPos) {
        this.Mesh = mesh;
        this.Sampler = sampler;
        this.DrawWires = drawWires;
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
            RasterizerStateDescription? rasterizerState = null;
            
            if (this.DrawWires) {
                rasterizerState = RasterizerStateDescription.DEFAULT with {
                    CullMode = FaceCullMode.None,
                    FillMode = PolygonFillMode.Wireframe
                };
            }
            
            Transform transform = new Transform() {
                Translation = this.LerpedGlobalPosition,
                Rotation = this.LerpedRotation,
                Scale = this.LerpedScale
            };
            
            // Draw mesh.
            this.Mesh.Draw(context.CommandList, transform, framebuffer.OutputDescription, this.Sampler, null, rasterizerState, this.MeshColor);

            // Draw bounding box.
            if (this.DrawBoundingBox) {
                context.ImmediateRenderer.DrawBoundingBox(context.CommandList, framebuffer.OutputDescription, new Transform(), this._box, this.BoxColor);
            }
        }
    }
}