using Bliss.CSharp;
using Bliss.CSharp.Geometry;
using Bliss.CSharp.Geometry.Meshes;
using Bliss.CSharp.Graphics.Rendering.Renderers.Forward;

namespace Sparkle.CSharp.Terrain.Regions;

public class TerrainRegionBatch : Disposable {
    
    /// <summary>
    /// The mesh containing the geometry data for this terrain batch.
    /// </summary>
    public IMesh? Mesh { get; private set; }
    
    /// <summary>
    /// The renderable used to draw this terrain batch.
    /// </summary>
    public Renderable? Renderable { get; private set; }
    
    /// <summary>
    /// The local-space bounding box enclosing the geometry of this terrain batch.
    /// </summary>
    public BoundingBox LocalBounds { get; private set; }
    
    /// <summary>
    /// The total number of vertices in this terrain batch.
    /// </summary>
    public int VertexCount { get; private set; }
    
    /// <summary>
    /// The total number of indices in this terrain batch.
    /// </summary>
    public int IndexCount { get; private set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TerrainRegionBatch"/> class.
    /// </summary>
    /// <param name="mesh">The mesh containing the geometry data for this batch.</param>
    /// <param name="renderable">The renderable used to draw this batch.</param>
    /// <param name="localBounds">The local-space bounding box of the batch geometry.</param>
    /// <param name="vertexCount">The number of vertices in the batch.</param>
    /// <param name="indexCount">The number of indices in the batch.</param>
    public TerrainRegionBatch(IMesh mesh, Renderable renderable, BoundingBox localBounds, int vertexCount, int indexCount) {
        this.Mesh = mesh;
        this.Renderable = renderable;
        this.LocalBounds = localBounds;
        this.VertexCount = vertexCount;
        this.IndexCount = indexCount;
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            this.Renderable?.Dispose();
            this.Renderable = null;
            this.Mesh?.Dispose();
            this.Mesh = null;
        }
    }
}