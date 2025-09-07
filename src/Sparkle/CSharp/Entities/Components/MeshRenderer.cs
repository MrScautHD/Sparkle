using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Geometry;
using Bliss.CSharp.Graphics.Rendering.Renderers.Forward.Renderables;
using Bliss.CSharp.Materials;
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
    /// A reference to the material used for rendering the mesh.
    /// </summary>
    public ref Material Material => ref this._renderable.Material;
    
    /// <summary>
    /// A reference to the bone matrices for skeletal animation, if applicable.
    /// </summary>
    public ref Matrix4x4[]? BoneMatrics => ref this._renderable.BoneMatrices;
    
    /// <summary>
    /// Whether to draw the bounding box around the mesh.
    /// </summary>
    public bool DrawBoundingBox;
    
    /// <summary>
    /// The color used for rendering the bounding box.
    /// </summary>
    public Color BoxColor;
    
    /// <summary>
    /// The mesh's axis-aligned bounding box.
    /// </summary>
    private BoundingBox _box;
    
    /// <summary>
    /// An internal renderable object used by the MeshRenderer to manage rendering operations.
    /// </summary>
    private Renderable _renderable;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MeshRenderer"/> class.
    /// </summary>
    /// <param name="mesh">The <see cref="Mesh"/> to render.</param>
    /// <param name="offsetPosition">The initial position offset applied to the transform.</param>
    /// <param name="copyMeshMaterial">If true, creates a cloned copy of the mesh’s material; otherwise, uses the original mesh material reference.</param>
    /// <param name="drawBoundingBox">If true, enables bounding box visualization for the mesh.</param>
    /// <param name="boxColor">Optional color to render the bounding box. Defaults to <see cref="Color.White"/> if not provided.</param>
    public MeshRenderer(Mesh mesh, Vector3 offsetPosition, bool copyMeshMaterial = false, bool drawBoundingBox = false, Color? boxColor = null) : this(mesh, offsetPosition, copyMeshMaterial ? (Material) mesh.Material.Clone() : mesh.Material, drawBoundingBox, boxColor) { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MeshRenderer"/> class.
    /// </summary>
    /// <param name="mesh">The <see cref="Mesh"/> to render.</param>
    /// <param name="offsetPosition">The initial position offset applied to the transform.</param>
    /// <param name="material">The <see cref="Material"/> used for rendering the mesh.</param>
    /// <param name="drawBoundingBox">If true, enables bounding box visualization for the mesh.</param>
    /// <param name="boxColor">Optional color to render the bounding box. Defaults to <see cref="Color.White"/> if not provided.</param>
    public MeshRenderer(Mesh mesh, Vector3 offsetPosition, Material material, bool drawBoundingBox = false, Color? boxColor = null) : base(offsetPosition) {
        this.Mesh = mesh;
        this.DrawBoundingBox = drawBoundingBox;
        this.BoxColor = boxColor ?? Color.White;
        this._box = mesh.BoundingBox;
        this._renderable = new Renderable(this.Mesh, new Transform(), material);
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
            this._renderable.Transform = transform;
            this.Entity.Scene.ForwardRenderer.DrawRenderable(this._renderable);

            // Draw the bounding box.
            if (this.DrawBoundingBox) {
                context.ImmediateRenderer.DrawBoundingBox(context.CommandList, framebuffer.OutputDescription, new Transform(), this._box, this.BoxColor);
            }
        }
    }
}