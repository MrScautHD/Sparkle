using Bliss.CSharp;
using Bliss.CSharp.Geometry;
using Bliss.CSharp.Geometry.Meshes;
using Bliss.CSharp.Graphics.Rendering.Renderers.Forward;

namespace Sparkle.CSharp.Terrain.Regions;

public class TerrainRegionBatch : Disposable {
    
    public IMesh? Mesh { get; private set; }
    
    public Renderable? Renderable { get; private set; }
    
    public BoundingBox LocalBounds { get; private set; }
    
    public int VertexCount { get; private set; }
    
    public int IndexCount { get; private set; }
    
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