using System.Numerics;
using Bliss.CSharp.Geometry;
using Bliss.CSharp.Graphics.Rendering.Renderers.Forward;
using Bliss.CSharp.Materials;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.Scenes;
using Veldrid;

namespace Sparkle.CSharp.Graphics.Rendering;

public class MultiInstanceRenderer {
    
    /// <summary>
    /// Gets the meshes managed by this multi-instance renderer.
    /// </summary>
    public Mesh[] Meshes { get; private set; }
    
    /// <summary>
    /// Stores all active instance render proxies used for instanced rendering.
    /// </summary>
    private List<InstancedRenderProxy> _instancedRenderProxies;
    
    /// <summary>
    /// Maps meshes to their corresponding renderable instances.
    /// </summary>
    private Dictionary<Mesh, Renderable> _renderables;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MultiInstanceRenderer"/> class using all meshes from a model.
    /// </summary>
    /// <param name="model">The model whose meshes will be rendered using instancing.</param>
    /// <param name="copyModelMaterials">If true, clones each mesh material to allow independent modification.</param>
    public MultiInstanceRenderer(Model model, bool copyModelMaterials = false) {
        this.Meshes = model.Meshes;
        this._instancedRenderProxies = new List<InstancedRenderProxy>();
        
        // Create renderables.
        this._renderables = new Dictionary<Mesh, Renderable>();
        foreach (Mesh mesh in this.Meshes) {
            this._renderables.Add(mesh, new Renderable(mesh, new Transform(), copyModelMaterials));
        }
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MultiInstanceRenderer"/> class using a single mesh.
    /// </summary>
    /// <param name="mesh">The mesh to render using instancing.</param>
    /// <param name="copyMeshMaterial">If true, clones the mesh material instead of using the shared reference.</param>
    public MultiInstanceRenderer(Mesh mesh, bool copyMeshMaterial = false) : this(mesh, copyMeshMaterial ? (Material) mesh.Material.Clone() : mesh.Material) { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MultiInstanceRenderer"/> class using a single mesh and material.
    /// </summary>
    /// <param name="mesh">The mesh to render using instancing.</param>
    /// <param name="material">The material used for rendering the mesh.</param>
    public MultiInstanceRenderer(Mesh mesh, Material material) {
        this.Meshes = [mesh];
        this._instancedRenderProxies = new List<InstancedRenderProxy>();
        this._renderables = new Dictionary<Mesh, Renderable>();
        this._renderables.Add(mesh, new Renderable(mesh, new Transform(), material));
    }
    
    /// <summary>
    /// Retrieves a reference to the material used by the renderable associated with the specified mesh.
    /// </summary>
    /// <param name="mesh">The mesh whose renderable material is requested.</param>
    /// <returns>A reference to the material assigned to the mesh renderable.</returns>
    public ref Material GetRenderableMaterialByMesh(Mesh mesh) {
        return ref this._renderables[mesh].Material;
    }
    
    /// <summary>
    /// Retrieves the bone matrix array associated with the renderable for the specified mesh.
    /// </summary>
    /// <param name="mesh">The mesh whose bone matrices are requested.</param>
    /// <returns>The bone matrix array if the mesh is skinned, otherwise null.</returns>
    public Matrix4x4[]? GetRenderableBoneMatricesByMesh(Mesh mesh) {
        return this._renderables[mesh].BoneMatrices;
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
        
        foreach (Renderable renderable in this._renderables.Values) {
            
            // Adjust instance storage.
            this.AdjustInstanceStorage(renderable);
            
            // Set transform.
            for (int i = 0; i < this._instancedRenderProxies.Count; i++) {
                InstancedRenderProxy renderProxy = this._instancedRenderProxies[i];
                
                // Updates frustum box.
                renderProxy.UpdateFrustumBox();
                
                if (cam3D.GetFrustum().ContainsOrientedBox(renderProxy.FrustumBox, renderProxy.LerpedGlobalPosition, renderProxy.LerpedRotation)) {
                    renderable.Transforms[i] = new Transform() {
                        Translation = renderProxy.LerpedGlobalPosition,
                        Rotation = renderProxy.LerpedRotation,
                        Scale = renderProxy.LerpedScale
                    };
                }
                
                // Draw the box.
                if (renderProxy.DrawBox) {
                    Transform boxTransform = new Transform() {
                        Translation = renderProxy.LerpedGlobalPosition,
                        Rotation = renderProxy.LerpedRotation,
                        Scale = renderProxy.LerpedScale
                    };
                    
                    // Draw box.
                    context.ImmediateRenderer.DrawBoundingBox(context.CommandList, framebuffer.OutputDescription, boxTransform, renderProxy.BaseBox, renderProxy.BoxColor);
                }
            }
            
            // Draw.
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
    
    /// <summary>
    /// Ensures the renderable transform array matches the current instance count.
    /// </summary>
    /// <param name="renderable">The renderable whose instance storage should be adjusted.</param>
    private void AdjustInstanceStorage(Renderable renderable) {
        int requiredCount = this._instancedRenderProxies.Count;
        
        if (renderable.Transforms.Length != requiredCount) {
            Array.Resize(ref renderable.Transforms, requiredCount);
        }
    }
}