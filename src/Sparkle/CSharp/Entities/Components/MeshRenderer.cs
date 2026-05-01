using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Geometry;
using Bliss.CSharp.Geometry.Meshes;
using Bliss.CSharp.Graphics.Rendering.Renderers;
using Bliss.CSharp.Graphics.Rendering.Renderers.Forward;
using Bliss.CSharp.Materials;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.Graphics.Rendering.Gizmos;
using Sparkle.CSharp.Scenes;
using Veldrid;

namespace Sparkle.CSharp.Entities.Components;

public class MeshRenderer : InterpolatedComponent, IDebugDrawable {
    
    /// <summary>
    /// The 3D mesh to render.
    /// </summary>
    public IMesh Mesh { get; private set; }
    
    /// <summary>
    /// The material used for rendering the renderable.
    /// </summary>
    public Material Material {
        get => this._renderable.Material;
        set => this._renderable.Material = value;
    }
    
    /// <summary>
    /// Indicates whether the associated mesh supports skeletal animation by containing bone data.
    /// </summary>
    public bool HasBones => this._renderable.HasBones;
    
    /// <summary>
    /// Enables or disables frustum culling for this mesh.
    /// </summary>
    public bool FrustumCulling;
    
    /// <summary>
    /// The color used for rendering the bounding box of the mesh.
    /// </summary>
    public Color BoxColor;
    
    /// <summary>
    /// Gets or sets a value indicating whether debug drawing is enabled.
    /// </summary>
    public bool DebugDrawEnabled { get; set; }
    
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
    /// <param name="frustumCulling">Whether the mesh should be skipped when outside the camera frustum.</param>
    /// <param name="boxColor">The color used to render the bounding box.</param>
    public MeshRenderer(IMesh mesh, Vector3 offsetPosition, bool copyMeshMaterial = false, bool frustumCulling = true, Color? boxColor = null) : this(mesh, offsetPosition, copyMeshMaterial ? (Material) mesh.Material.Clone() : mesh.Material, frustumCulling, boxColor) { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MeshRenderer"/> class.
    /// </summary>
    /// <param name="mesh">The mesh to be rendered.</param>
    /// <param name="offsetPosition">The positional offset applied to the renderer.</param>
    /// <param name="material">The material used to render the mesh.</param>
    /// <param name="frustumCulling">Whether the mesh should be skipped when outside the camera frustum.</param>
    /// <param name="boxColor">The color used to render the bounding box.</param>
    public MeshRenderer(IMesh mesh, Vector3 offsetPosition, Material material, bool frustumCulling = true, Color? boxColor = null) : base(offsetPosition) {
        this.Mesh = mesh;
        this.FrustumCulling = frustumCulling;
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
        
        bool shouldRender = true;
        
        if (this.FrustumCulling) {
            this.UpdateFrustumBox();
            shouldRender = cam3D.GetFrustum().ContainsOrientedBox(this._frustumBox, this.LerpedGlobalPosition, this.LerpedRotation);
        }
        
        if (shouldRender) {
            Transform meshTransform = new Transform() {
                Translation = this.LerpedGlobalPosition,
                Rotation = this.LerpedRotation,
                Scale = this.LerpedScale
            };
            
            // Draw the mesh.
            this._renderable.SetTransform(0, meshTransform);
            this.Entity.Scene.Renderer.DrawRenderable(this._renderable);
        }
    }
    
    /// <summary>
    /// Renders debug visualization.
    /// </summary>
    /// <param name="immediateRenderer">The renderer used to draw debug primitives.</param>
    public void DrawDebug(ImmediateRenderer immediateRenderer) {
        Transform boxTransform = new Transform() {
            Translation = this.LerpedGlobalPosition,
            Rotation = this.LerpedRotation,
            Scale = this.LerpedScale
        };
        
        immediateRenderer.DrawBoundingBox(boxTransform, this._baseBox, this.BoxColor);
    }
    
    /// <summary>
    /// Replaces the base/frustum bounds used for culling.
    /// Useful for dynamic meshes where generated geometry changes over time.
    /// </summary>
    /// <param name="bounds">New local-space bounding box.</param>
    public void SetCullingBounds(BoundingBox bounds) {
        this._baseBox = bounds;
        this._frustumBox = bounds;
    }
    
    /// <summary>
    /// Retrieves the current bone transformation matrices used for animating the mesh.
    /// </summary>
    /// <returns>A read-only span containing the bone transformation matrices.</returns>
    public ReadOnlySpan<Matrix4x4> GetBoneMatrices() {
        return this._renderable.GetBoneMatrices();
    }
    
    /// <summary>
    /// Updates the matrix for a specific bone in the mesh renderer.
    /// </summary>
    /// <param name="index">The index of the bone to update.</param>
    /// <param name="value">The transformation matrix to set for the bone.</param>
    public void SetBoneMatrix(int index, Matrix4x4 value) {
        this._renderable.SetBoneMatrix(index, value);
    }
    
    /// <summary>
    /// Resets all bone matrices associated with the mesh renderer to their default state.
    /// </summary>
    public void ClearBoneMatrices() {
        this._renderable.ClearBoneMatrices();
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

    protected override void Dispose(bool disposing) {
        if (disposing) {
            this._renderable.Dispose();
        }
    }
}
