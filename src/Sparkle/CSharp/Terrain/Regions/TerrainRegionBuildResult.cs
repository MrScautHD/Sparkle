using Bliss.CSharp.Geometry;
using Bliss.CSharp.Graphics.VertexTypes;

namespace Sparkle.CSharp.Terrain.Regions;

public readonly struct TerrainRegionBuildResult {
    
    public readonly TerrainRegionKey Key;
    
    public readonly Vertex3D[]? Vertices;
    
    public readonly uint[]? Indices;
    
    public readonly BoundingBox LocalBounds;
    
    public readonly int VertexCount;
    
    public readonly int IndexCount;
    
    public readonly bool HasGeometry;
    
    private TerrainRegionBuildResult(TerrainRegionKey key, Vertex3D[]? vertices, uint[]? indices, BoundingBox localBounds, int vertexCount, int indexCount, bool hasGeometry) {
        this.Key = key;
        this.Vertices = vertices;
        this.Indices = indices;
        this.LocalBounds = localBounds;
        this.VertexCount = vertexCount;
        this.IndexCount = indexCount;
        this.HasGeometry = hasGeometry;
    }
    
    public static TerrainRegionBuildResult Create(TerrainRegionKey key, Vertex3D[] vertices, uint[] indices, BoundingBox localBounds, int vertexCount, int indexCount) {
        return new TerrainRegionBuildResult(key, vertices, indices, localBounds, vertexCount, indexCount, true);
    }
    
    public static TerrainRegionBuildResult Empty(TerrainRegionKey key) {
        return new TerrainRegionBuildResult(key, null, null, default, 0, 0, false);
    }
}