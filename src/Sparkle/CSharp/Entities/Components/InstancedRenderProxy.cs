using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Geometry;
using Bliss.CSharp.Geometry.Meshes;
using Bliss.CSharp.Graphics.Rendering.Renderers;
using Bliss.CSharp.Materials;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics.Rendering;
using Sparkle.CSharp.Graphics.Rendering.Gizmos;

namespace Sparkle.CSharp.Entities.Components;

public class InstancedRenderProxy : InterpolatedComponent, IDebugDrawable {
    
    /// <summary>
    /// Gets the multi-instance renderer that owns this render proxy.
    /// </summary>
    public MultiInstanceRenderer MultiInstanceRenderer { get; private set; }
    
    /// <summary>
    /// Enables or disables frustum culling for this render proxy.
    /// </summary>
    public bool FrustumCulling;
    
    /// <summary>
    /// Specifies the color used when rendering the bounding box.
    /// </summary>
    public Color BoxColor;
    
    /// <summary>
    /// Gets or sets a value indicating whether debug drawing is enabled.
    /// </summary>
    public bool DebugDrawEnabled { get; set; }
    
    /// <summary>
    /// Stores the base local-space bounding box shared by all instances.
    /// </summary>
    internal BoundingBox BaseBox;
    
    /// <summary>
    /// Stores the dynamically adjusted bounding box used for frustum culling.
    /// </summary>
    internal BoundingBox FrustumBox;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="InstancedRenderProxy"/> class.
    /// </summary>
    /// <param name="multiInstanceRenderer">The multi-instance renderer responsible for drawing this proxy.</param>
    /// <param name="offsetPosition">The local position offset applied to the proxy transform.</param>
    /// <param name="frustumCulling">Whether the render proxy should be skipped when outside the camera frustum.</param>
    /// <param name="boxColor">Optional color used to render the bounding box.</param>
    public InstancedRenderProxy(MultiInstanceRenderer multiInstanceRenderer, Vector3 offsetPosition, bool frustumCulling = true, Color? boxColor = null) : base(offsetPosition) {
        this.MultiInstanceRenderer = multiInstanceRenderer;
        this.FrustumCulling = frustumCulling;
        this.BoxColor = boxColor ?? Color.White;
        this.BaseBox = this.FrustumBox = this.GenBoundingBox();
    }
    
    /// <summary>
    /// Initializes the render proxy and registers it with the multi-instance renderer.
    /// </summary>
    protected internal override void Init() {
        base.Init();
        
        // Add the render proxy to the multi-instance renderer.
        this.MultiInstanceRenderer.AddRenderProxy(this);
        
        // Add this to the scene. (It gets automatically handled)
        this.Entity.Scene.AddMultiInstanceRenderer(this.MultiInstanceRenderer);
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
        
        immediateRenderer.DrawBoundingBox(boxTransform, this.BaseBox, this.BoxColor);
    }
    
    /// <summary>
    /// Updates the frustum-aligned bounding box based on the interpolated transform state.
    /// </summary>
    protected internal void UpdateFrustumBox() {
        
        // Calculate original dimensions and center offset in local space.
        Vector3 originalCenter = (this.BaseBox.Min + this.BaseBox.Max) / 2.0F;
        Vector3 originalDimension = this.BaseBox.Max - this.BaseBox.Min;
        
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
        
        this.FrustumBox.Min.X = finalCenter.X - dimension.X / 2.0F;
        this.FrustumBox.Min.Y = lerpedPos.Y + (this.BaseBox.Min.Y * this.LerpedScale.Y);
        this.FrustumBox.Min.Z = finalCenter.Z - dimension.Z / 2.0F;
        
        this.FrustumBox.Max.X = finalCenter.X + dimension.X / 2.0F;
        this.FrustumBox.Max.Y = lerpedPos.Y + (this.BaseBox.Max.Y * this.LerpedScale.Y);
        this.FrustumBox.Max.Z = finalCenter.Z + dimension.Z / 2.0F;
    }
    
    /// <summary>
    /// Retrieves the material associated with the renderable representation of the specified mesh.
    /// </summary>
    /// <param name="mesh">The mesh for which to retrieve the renderable material.</param>
    /// <returns>The material associated with the given mesh's renderable representation.</returns>
    public Material GetRenderableMaterialByMesh(IMesh mesh) {
        return this.MultiInstanceRenderer.GetRenderableMaterialByMesh(mesh);
    }
    
    /// <summary>
    /// Sets the material for the renderable associated with the specified mesh.
    /// </summary>
    /// <param name="mesh">The mesh whose renderable's material should be updated.</param>
    /// <param name="material">The new material to assign to the renderable.</param>
    public void SetRenderableMaterialByMesh(IMesh mesh, Material material) {
        this.MultiInstanceRenderer.SetRenderableMaterialByMesh(mesh, material);
    }
    
    /// <summary>
    /// Checks if the renderable associated with the specified mesh contains bones.
    /// </summary>
    /// <param name="mesh">The mesh for which the bone data is being queried.</param>
    /// <returns>True if the renderable associated with the mesh contains bones; otherwise, false.</returns>
    public bool HasRenderableBonesByMesh(IMesh mesh) {
        return this.MultiInstanceRenderer.HasRenderableBonesByMesh(mesh);
    }
    
    /// <summary>
    /// Retrieves the collection of bone transformation matrices for a specific mesh's renderable representation.
    /// </summary>
    /// <param name="mesh">The mesh for which bone transformation matrices are to be retrieved.</param>
    /// <returns>A read-only span containing the bone transformation matrices for the specified mesh.</returns>
    public ReadOnlySpan<Matrix4x4> GetRenderableBoneMatricesByMesh(IMesh mesh) {
        return this.MultiInstanceRenderer.GetRenderableBoneMatricesByMesh(mesh);
    }
    
    /// <summary>
    /// Sets the transformation matrix for a specific bone of a renderable mesh within the model.
    /// </summary>
    /// <param name="mesh">The mesh whose renderable representation contains the bone to be updated.</param>
    /// <param name="index">The index of the bone in the specified mesh.</param>
    /// <param name="value">The transformation matrix to be assigned to the bone.</param>
    public void SetRenderableBoneMatrixByMesh(IMesh mesh, int index, Matrix4x4 value) {
        this.MultiInstanceRenderer.SetRenderableBoneMatrixByMesh(mesh, index, value);
    }
    
    /// <summary>
    /// Clears all the bone matrices associated with a specific mesh in the model renderer.
    /// </summary>
    /// <param name="mesh">The mesh whose bone matrices should be cleared.</param>
    public void ClearRenderableBoneMatricesByMesh(IMesh mesh) {
        this.MultiInstanceRenderer.ClearRenderableBoneMatricesByMesh(mesh);
    }
    
    /// <summary>
    /// Generates a bounding box that encapsulates all meshes in the MultiInstanceRenderer.
    /// </summary>
    /// <returns>A <see cref="BoundingBox"/> representing the smallest box that contains all meshes.
    /// </returns>
    private BoundingBox GenBoundingBox() {
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        
        foreach (IMesh mesh in this.MultiInstanceRenderer.Meshes) {
            BoundingBox meshBox = mesh.GenBoundingBox();
            
            min = Vector3.Min(min, meshBox.Min);
            max = Vector3.Max(max, meshBox.Max);
        }
        
        return new BoundingBox(min, max);
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            
            // Remove the render proxy from the multi-instance renderer.
            this.MultiInstanceRenderer.RemoveRenderProxy(this);
        }
    }
}