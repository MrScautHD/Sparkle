using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Geometry;
using Bliss.CSharp.Geometry.Meshes;
using Bliss.CSharp.Geometry.Models;
using Bliss.CSharp.Graphics.Rendering.Renderers;
using Bliss.CSharp.Graphics.Rendering.Renderers.Forward;
using Bliss.CSharp.Materials;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.Graphics.Rendering.Gizmos;
using Sparkle.CSharp.Scenes;
using Veldrid;

namespace Sparkle.CSharp.Entities.Components;

public class ModelRenderer : InterpolatedComponent, IDebugDrawable {
    
    /// <summary>
    /// The model to be rendered.
    /// </summary>
    public Model Model { get; private set; }
    
    /// <summary>
    /// Enables or disables frustum culling for this model.
    /// </summary>
    public bool FrustumCulling;
    
    /// <summary>
    /// The color used for rendering the bounding box of the model.
    /// </summary>
    public Color BoxColor;
    
    /// <summary>
    /// Gets or sets a value indicating whether debug drawing is enabled.
    /// </summary>
    public bool DebugDrawEnabled { get; set; }

    /// <summary>
    /// The original bounding box of the model before being transformed.
    /// </summary>
    private BoundingBox _baseBox;
    
    /// <summary>
    /// The bounding box of the model used for visibility checks.
    /// </summary>
    private BoundingBox _frustumBox;
    
    /// <summary>
    /// A collection that maps each mesh in the model to its corresponding renderable representation.
    /// </summary>
    private Dictionary<IMesh, Renderable> _renderables;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ModelRenderer"/> class.
    /// </summary>
    /// <param name="model">The model containing the meshes to be rendered.</param>
    /// <param name="offsetPosition">The positional offset applied to the model renderer.</param>
    /// <param name="copyModelMaterials">Whether to clone the model's materials so they can be modified independently.</param>
    /// <param name="frustumCulling">Whether the model should be skipped when outside the camera frustum.</param>
    /// <param name="boxColor">The color used to render the bounding box.</param>
    public ModelRenderer(Model model, Vector3 offsetPosition, bool copyModelMaterials = false, bool frustumCulling = true, Color? boxColor = null) : base(offsetPosition) {
        this.Model = model;
        this.FrustumCulling = frustumCulling;
        this.BoxColor = boxColor ?? Color.White;
        this._baseBox = this._frustumBox = model.GenBoundingBox();
        this._renderables = new Dictionary<IMesh, Renderable>();
        
        foreach (IMesh mesh in this.Model.Meshes) {
            this._renderables.Add(mesh, new Renderable(mesh, new Transform(), copyModelMaterials));
        }
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
        
        bool shouldRender = true;
        
        if (this.FrustumCulling) {
            this.UpdateFrustumBox();
            shouldRender = cam3D.GetFrustum().ContainsOrientedBox(this._frustumBox, this.LerpedGlobalPosition, this.LerpedRotation);
        }
        
        if (shouldRender) {
            Transform modelTransform = new Transform() {
                Translation = this.LerpedGlobalPosition,
                Rotation = this.LerpedRotation,
                Scale = this.LerpedScale
            };
            
            // Draw the model.
            foreach (Renderable renderable in this._renderables.Values) {
                renderable.SetTransform(0, modelTransform);
                this.Entity.Scene.Renderer.DrawRenderable(renderable);
            }
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
    /// Useful for dynamic models where generated geometry changes over time.
    /// </summary>
    /// <param name="bounds">New local-space bounding box.</param>
    public void SetCullingBounds(BoundingBox bounds) {
        this._baseBox = bounds;
        this._frustumBox = bounds;
    }
    
    /// <summary>
    /// Retrieves the material associated with the renderable representation of the specified mesh.
    /// </summary>
    /// <param name="mesh">The mesh for which to retrieve the renderable material.</param>
    /// <returns>The material associated with the given mesh's renderable representation.</returns>
    public Material GetRenderableMaterialByMesh(IMesh mesh) {
        return this._renderables[mesh].Material;
    }
    
    /// <summary>
    /// Sets the material for the renderable associated with the specified mesh.
    /// </summary>
    /// <param name="mesh">The mesh whose renderable's material should be updated.</param>
    /// <param name="material">The new material to assign to the renderable.</param>
    public void SetRenderableMaterialByMesh(IMesh mesh, Material material) {
        this._renderables[mesh].Material = material;
    }
    
    /// <summary>
    /// Checks if the renderable associated with the specified mesh contains bones.
    /// </summary>
    /// <param name="mesh">The mesh for which the bone data is being queried.</param>
    /// <returns>True if the renderable associated with the mesh contains bones; otherwise, false.</returns>
    public bool HasRenderableBonesByMesh(IMesh mesh) {
        return this._renderables[mesh].HasBones;
    }
    
    /// <summary>
    /// Retrieves the collection of bone transformation matrices for a specific mesh's renderable representation.
    /// </summary>
    /// <param name="mesh">The mesh for which bone transformation matrices are to be retrieved.</param>
    /// <returns>A read-only span containing the bone transformation matrices for the specified mesh.</returns>
    public ReadOnlySpan<Matrix4x4> GetRenderableBoneMatricesByMesh(IMesh mesh) {
        return this._renderables[mesh].GetBoneMatrices();
    }
    
    /// <summary>
    /// Sets the transformation matrix for a specific bone of a renderable mesh within the model.
    /// </summary>
    /// <param name="mesh">The mesh whose renderable representation contains the bone to be updated.</param>
    /// <param name="index">The index of the bone in the specified mesh.</param>
    /// <param name="value">The transformation matrix to be assigned to the bone.</param>
    public void SetRenderableBoneMatrixByMesh(IMesh mesh, int index, Matrix4x4 value) {
        this._renderables[mesh].SetBoneMatrix(index, value);
    }
    
    /// <summary>
    /// Clears all the bone matrices associated with a specific mesh in the model renderer.
    /// </summary>
    /// <param name="mesh">The mesh whose bone matrices should be cleared.</param>
    public void ClearRenderableBoneMatricesByMesh(IMesh mesh) {
        this._renderables[mesh].ClearBoneMatrices();
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
            foreach (Renderable renderable in this._renderables.Values) {
                renderable.Dispose();
            }
        }
    }
}