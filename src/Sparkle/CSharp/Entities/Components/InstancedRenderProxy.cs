using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Geometry;
using Bliss.CSharp.Materials;
using Sparkle.CSharp.Graphics.Rendering;

namespace Sparkle.CSharp.Entities.Components;

public class InstancedRenderProxy : InterpolatedComponent {
    
    /// <summary>
    /// Gets the multi-instance renderer that owns this render proxy.
    /// </summary>
    public MultiInstanceRenderer MultiInstanceRenderer { get; private set; }
    
    /// <summary>
    /// Determines whether the bounding box for this instance should be drawn.
    /// </summary>
    public bool DrawBox;
    
    /// <summary>
    /// Specifies the color used when rendering the bounding box.
    /// </summary>
    public Color BoxColor;
    
    /// <summary>
    /// Stores the base local-space bounding box shared by all instances.
    /// </summary>
    internal BoundingBox BaseBox;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="InstancedRenderProxy"/> class.
    /// </summary>
    /// <param name="multiInstanceRenderer">The multi-instance renderer responsible for drawing this proxy.</param>
    /// <param name="offsetPosition">The local position offset applied to the proxy transform.</param>
    /// <param name="drawBox">If true, enables rendering of the instance bounding box.</param>
    /// <param name="boxColor">Optional color used to render the bounding box.</param>
    public InstancedRenderProxy(MultiInstanceRenderer multiInstanceRenderer, Vector3 offsetPosition, bool drawBox = false, Color? boxColor = null) : base(offsetPosition) {
        this.MultiInstanceRenderer = multiInstanceRenderer;
        this.DrawBox = drawBox;
        this.BoxColor = boxColor ?? Color.White;
        this.BaseBox = this.GenBoundingBox();
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
    /// Retrieves a reference to the material used by the renderable associated with the specified mesh.
    /// </summary>
    /// <param name="mesh">The mesh whose renderable material is requested.</param>
    /// <returns>A reference to the material used by the mesh renderable.</returns>
    public ref Material GetRenderableMaterialByMesh(Mesh mesh) {
        return ref this.MultiInstanceRenderer.GetRenderableMaterialByMesh(mesh);
    }
    
    /// <summary>
    /// Retrieves the bone matrix array used by the renderable associated with the specified mesh.
    /// </summary>
    /// <param name="mesh">The mesh whose bone matrices are requested.</param>
    /// <returns>The bone matrix array if the mesh is skinned, otherwise null.</returns>
    public Matrix4x4[]? GetRenderableBoneMatricesByMesh(Mesh mesh) {
        return this.MultiInstanceRenderer.GetRenderableBoneMatricesByMesh(mesh);
    }
    
    /// <summary>
    /// Generates a bounding box that encapsulates all meshes in the MultiInstanceRenderer.
    /// </summary>
    /// <returns>A <see cref="BoundingBox"/> representing the smallest box that contains all meshes.
    /// </returns>
    private BoundingBox GenBoundingBox() {
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        
        foreach (Mesh mesh in this.MultiInstanceRenderer.Meshes) {
            BoundingBox meshBox = mesh.GenBoundingBox();
            
            min = Vector3.Min(min, meshBox.Min);
            max = Vector3.Max(max, meshBox.Max);
        }
        
        return new BoundingBox(min, max);
    }
    
    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        
        if (disposing) {
            
            // Remove the render proxy from the multi-instance renderer.
            this.MultiInstanceRenderer.RemoveRenderProxy(this);
        }
    }
}