using System.Numerics;
using Bliss.CSharp;
using Bliss.CSharp.Geometry.Meshes;
using Bliss.CSharp.Geometry.Models;
using Bliss.CSharp.Graphics.Rendering;
using Bliss.CSharp.Graphics.Rendering.Renderers.Forward;
using Bliss.CSharp.Materials;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.Scenes;
using Veldrid;

namespace Sparkle.CSharp.Graphics.Rendering;

public class MultiInstanceRenderer : Disposable {
    
    /// <summary>
    /// Gets the meshes managed by this multi-instance renderer.
    /// </summary>
    public IMesh[] Meshes { get; private set; }
    
    /// <summary>
    /// Stores all active instance render proxies used for instanced rendering.
    /// </summary>
    private List<InstancedRenderProxy> _instancedRenderProxies;
    
    /// <summary>
    /// Maps meshes to their corresponding renderable instances.
    /// </summary>
    private Dictionary<IMesh, Renderable> _renderables;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MultiInstanceRenderer"/> class using all meshes from a model.
    /// </summary>
    /// <param name="model">The model whose meshes will be rendered using instancing.</param>
    /// <param name="copyModelMaterials">If true, clones each mesh material to allow independent modification.</param>
    public MultiInstanceRenderer(Model model, bool copyModelMaterials = false) {
        this.Meshes = model.Meshes;
        this._instancedRenderProxies = new List<InstancedRenderProxy>();
        
        // Create renderables.
        this._renderables = new Dictionary<IMesh, Renderable>();
        
        foreach (IMesh mesh in this.Meshes) {
            this._renderables.Add(mesh, new Renderable(mesh, new Transform(), copyModelMaterials, true));
        }
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MultiInstanceRenderer"/> class using a single mesh.
    /// </summary>
    /// <param name="mesh">The mesh to render using instancing.</param>
    /// <param name="copyMeshMaterial">If true, clones the mesh material instead of using the shared reference.</param>
    public MultiInstanceRenderer(IMesh mesh, bool copyMeshMaterial = false) : this(mesh, copyMeshMaterial ? (Material) mesh.Material.Clone() : mesh.Material) { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MultiInstanceRenderer"/> class using a single mesh and material.
    /// </summary>
    /// <param name="mesh">The mesh to render using instancing.</param>
    /// <param name="material">The material used for rendering the mesh.</param>
    public MultiInstanceRenderer(IMesh mesh, Material material) {
        this.Meshes = [mesh];
        this._instancedRenderProxies = new List<InstancedRenderProxy>();
        this._renderables = new Dictionary<IMesh, Renderable>();
        this._renderables.Add(mesh, new Renderable(mesh, new Transform(), material, true));
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
    /// Draws all instanced renderables using the active camera and renderer.
    /// </summary>
    /// <param name="context">The graphics context used for issuing draw commands.</param>
    /// <param name="framebuffer">The framebuffer to render into.</param>
    internal void Draw(GraphicsContext context, Framebuffer framebuffer) {
        Camera3D? cam3D = SceneManager.ActiveCam3D;
        
        if (cam3D == null) {
            return;
        }
        
        Frustum frustum = cam3D.GetFrustum();
        
        foreach (Renderable renderable in this._renderables.Values) {
            
            // Adjust instance storage.
            renderable.ResizeTransformArray((uint) this._instancedRenderProxies.Count);
            
            int visibleCount = 0;
            
            for (int i = 0; i < this._instancedRenderProxies.Count; i++) {
                InstancedRenderProxy renderProxy = this._instancedRenderProxies[i];
                
                // Updates frustum box.
                renderProxy.UpdateFrustumBox();
                
                bool visible = !renderProxy.FrustumCulling || frustum.ContainsOrientedBox(renderProxy.FrustumBox, renderProxy.LerpedGlobalPosition, renderProxy.LerpedRotation);
                
                if (!visible) {
                    continue;
                }
                
                // Set the renderable transforms.
                renderable.SetTransform(visibleCount, new Transform() {
                    Translation = renderProxy.LerpedGlobalPosition,
                    Rotation = renderProxy.LerpedRotation,
                    Scale = renderProxy.LerpedScale
                });
                
                visibleCount++;
            }
            
            if (visibleCount == 0) {
                continue;
            }
            
            renderable.ResizeTransformArray((uint) visibleCount);
            
            SceneManager.ActiveScene?.Renderer.DrawRenderable(renderable);
        }
    }
    
    /// <summary>
    /// Retrieves all currently registered instance render proxies.
    /// </summary>
    /// <returns>A read-only list of instanced render proxies.</returns>
    internal IReadOnlyList<InstancedRenderProxy> GetRenderProxies() {
        return this._instancedRenderProxies;
    }
    
    /// <summary>
    /// Adds a render proxy to the instancing system or throws if the operation fails.
    /// </summary>
    /// <param name="renderProxy">The render proxy to register.</param>
    internal void AddRenderProxy(InstancedRenderProxy renderProxy) {
        if (!this.TryAddRenderProxy(renderProxy)) {
            throw new InvalidOperationException($"Failed to add the render proxy of the entity with the id: [{renderProxy.Entity.Id}], The proxy is either already registered or is bound to a invalid entity.");
        }
    }
    
    /// <summary>
    /// Attempts to add a render proxy to the instancing system.
    /// </summary>
    /// <param name="renderProxy">The render proxy to register.</param>
    /// <returns>True if the proxy was added successfully, otherwise false.</returns>
    internal bool TryAddRenderProxy(InstancedRenderProxy renderProxy) {
        if (this._instancedRenderProxies.Contains(renderProxy)) {
            return false;
        }
        
        if (renderProxy.Entity == null!) {
            return false;
        }
        
        this._instancedRenderProxies.Add(renderProxy);
        return true;
    }
    
    /// <summary>
    /// Removes a render proxy from the instancing system or throws if it does not exist.
    /// </summary>
    /// <param name="renderProxy">The render proxy to remove.</param>
    internal void RemoveRenderProxy(InstancedRenderProxy renderProxy) {
        if (!this.TryRemoveRenderProxy(renderProxy)) {
            throw new Exception($"Failed to remove the render proxy of the entity with the id: [{renderProxy.Entity.Id}], The proxy is might already removed.");
        }
    }
    
    /// <summary>
    /// Attempts to remove a render proxy from the instancing system.
    /// </summary>
    /// <param name="renderProxy">The render proxy to remove.</param>
    /// <returns>True if the proxy was removed successfully, otherwise false.</returns>
    internal bool TryRemoveRenderProxy(InstancedRenderProxy renderProxy) {
        return this._instancedRenderProxies.Remove(renderProxy);
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            foreach (Renderable renderable in this._renderables.Values) {
                renderable.Dispose();
            }
        }
    }
}