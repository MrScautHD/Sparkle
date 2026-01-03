using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Geometry;
using Bliss.CSharp.Graphics.Rendering.Renderers.Forward;
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
    public Matrix4x4[]? BoneMatrics => this._renderable.BoneMatrices;
    
    /// <summary>
    /// Whether the bounding box should be rendered for the associated mesh.
    /// </summary>
    public bool DrawBox;
    
    /// <summary>
    /// The color used for rendering the bounding box of the mesh.
    /// </summary>
    public Color BoxColor;
    
    /// <summary>
    /// The original bounding box of the mesh before being transformed.
    /// </summary>
    private BoundingBox _baseBox;
    
    /// <summary>
    /// The bounding box of the mesh used for visibility checks.
    /// </summary>
    private BoundingBox _frustumBox;
    
    /// <summary>
    /// An internal renderable object used by the MeshRenderer to manage rendering operations.
    /// </summary>
    private Renderable _renderable;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MeshRenderer"/> class.
    /// </summary>
    /// <param name="mesh">The mesh to be rendered.</param>
    /// <param name="offsetPosition">The positional offset applied to the renderer.</param>
    /// <param name="copyMeshMaterial">Whether to clone the mesh material for independent modification.</param>
    /// <param name="drawBox">Whether to render the bounding box for debugging.</param>
    /// <param name="boxColor">The color used to render the bounding box.</param>
    public MeshRenderer(Mesh mesh, Vector3 offsetPosition, bool copyMeshMaterial = false, bool drawBox = false, Color? boxColor = null) : this(mesh, offsetPosition, copyMeshMaterial ? (Material) mesh.Material.Clone() : mesh.Material, drawBox, boxColor) { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MeshRenderer"/> class.
    /// </summary>
    /// <param name="mesh">The mesh to be rendered.</param>
    /// <param name="offsetPosition">The positional offset applied to the renderer.</param>
    /// <param name="material">The material used to render the mesh.</param>
    /// <param name="drawBox">Whether to render the bounding box for debugging.</param>
    /// <param name="boxColor">The color used to render the bounding box.</param>
    public MeshRenderer(Mesh mesh, Vector3 offsetPosition, Material material, bool drawBox = false, Color? boxColor = null) : base(offsetPosition) {
        this.Mesh = mesh;
        this.DrawBox = drawBox;
        this.BoxColor = boxColor ?? Color.White;
        this._baseBox = this._frustumBox = mesh.GenBoundingBox();
        this._renderable = new Renderable(this.Mesh, new Transform(), material);
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
        
        // Updates frustum box.
        this.UpdateFrustumBox();
        
        if (cam3D.GetFrustum().ContainsOrientedBox(this._frustumBox, this.LerpedGlobalPosition, this.LerpedRotation)) {
            Transform meshTransform = new Transform() {
                Translation = this.LerpedGlobalPosition,
                Rotation = this.LerpedRotation,
                Scale = this.LerpedScale
            };
            
            // Draw the mesh.
            this._renderable.Transforms[0] = meshTransform;
            this.Entity.Scene.Renderer.DrawRenderable(this._renderable);
            
            // Draw the bounding box.
            if (this.DrawBox) {
                Transform boxTransform = new Transform() {
                    Translation = this.LerpedGlobalPosition,
                    Rotation = this.LerpedRotation,
                    Scale = this.LerpedScale
                };
                
                // Draw box.
                context.ImmediateRenderer.DrawBoundingBox(context.CommandList, framebuffer.OutputDescription, boxTransform, this._baseBox, this.BoxColor);
            }
        }
    }
    
    /// <summary>
    /// Updates the frustum-aligned bounding box.
    /// </summary>
    private void UpdateFrustumBox() {
        
        // Calculate original dimensions and center offset in local space.
        Vector3 originalCenter = (this._baseBox.Min + this._baseBox.Max) / 2.0F;
        Vector3 originalDimension = this._baseBox.Max - this._baseBox.Min;
        
        // Scale everything.
        Vector3 dimension = originalDimension * this.LerpedScale;
        Vector3 centerOffset = originalCenter * this.LerpedScale;
        
        // Normalize X and Z dimensions to the maximum width.
        float maxSide = Math.Max(dimension.X, dimension.Z);
        dimension.X = maxSide;
        dimension.Z = maxSide;
        
        // Calculate bounding box position using the lerped global position and the center offset.
        Vector3 lerpedPos = this.LerpedGlobalPosition;
        Vector3 finalCenter = lerpedPos + centerOffset;
        
        this._frustumBox.Min.X = finalCenter.X - dimension.X / 2.0F;
        this._frustumBox.Min.Y = lerpedPos.Y + (this._baseBox.Min.Y * this.LerpedScale.Y);
        this._frustumBox.Min.Z = finalCenter.Z - dimension.Z / 2.0F;
        
        this._frustumBox.Max.X = finalCenter.X + dimension.X / 2.0F;
        this._frustumBox.Max.Y = lerpedPos.Y + (this._baseBox.Max.Y * this.LerpedScale.Y);
        this._frustumBox.Max.Z = finalCenter.Z + dimension.Z / 2.0F;
    }
}