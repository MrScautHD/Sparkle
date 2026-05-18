using Bliss.CSharp.Geometry;
using Bliss.CSharp.Graphics.VertexTypes;

namespace Sparkle.CSharp.Terrain.Regions;

public readonly struct TerrainRegionBuildResult {
    
    /// <summary>
    /// The key identifying the terrain region this result belongs to.
    /// </summary>
    public readonly TerrainRegionKey Key;
    
    /// <summary>
    /// The array of vertices making up the terrain region geometry, or null if no geometry was produced.
    /// </summary>
    public readonly Vertex3D[]? Vertices;
    
    /// <summary>
    /// The array of indices defining the triangle topology of the terrain region, or null if no geometry was produced.
    /// </summary>
    public readonly uint[]? Indices;
    
    /// <summary>
    /// The local-space bounding box enclosing the terrain region geometry.
    /// </summary>
    public readonly BoundingBox LocalBounds;
    
    /// <summary>
    /// The total number of vertices in the terrain region geometry.
    /// </summary>
    public readonly int VertexCount;
    
    /// <summary>
    /// The total number of indices in the terrain region geometry.
    /// </summary>
    public readonly int IndexCount;
    
    /// <summary>
    /// Indicates whether this result contains valid geometry data.
    /// </summary>
    public readonly bool HasGeometry;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TerrainRegionBuildResult"/> struct.
    /// </summary>
    /// <param name="key">The key identifying the terrain region.</param>
    /// <param name="vertices">The vertex data, or null for an empty result.</param>
    /// <param name="indices">The index data, or null for an empty result.</param>
    /// <param name="localBounds">The local-space bounding box of the geometry.</param>
    /// <param name="vertexCount">The number of vertices.</param>
    /// <param name="indexCount">The number of indices.</param>
    /// <param name="hasGeometry">Whether this result contains valid geometry.</param>
    private TerrainRegionBuildResult(TerrainRegionKey key, Vertex3D[]? vertices, uint[]? indices, BoundingBox localBounds, int vertexCount, int indexCount, bool hasGeometry) {
        this.Key = key;
        this.Vertices = vertices;
        this.Indices = indices;
        this.LocalBounds = localBounds;
        this.VertexCount = vertexCount;
        this.IndexCount = indexCount;
        this.HasGeometry = hasGeometry;
    }
    
    /// <summary>
    /// Creates a <see cref="TerrainRegionBuildResult"/> with valid geometry data.
    /// </summary>
    /// <param name="key">The key identifying the terrain region.</param>
    /// <param name="vertices">The vertex data for the region mesh.</param>
    /// <param name="indices">The index data for the region mesh.</param>
    /// <param name="localBounds">The local-space bounding box of the geometry.</param>
    /// <param name="vertexCount">The number of vertices.</param>
    /// <param name="indexCount">The number of indices.</param>
    /// <returns>A <see cref="TerrainRegionBuildResult"/> containing the provided geometry data.</returns>
    public static TerrainRegionBuildResult Create(TerrainRegionKey key, Vertex3D[] vertices, uint[] indices, BoundingBox localBounds, int vertexCount, int indexCount) {
        return new TerrainRegionBuildResult(key, vertices, indices, localBounds, vertexCount, indexCount, true);
    }
    
    /// <summary>
    /// Creates an empty <see cref="TerrainRegionBuildResult"/> with no geometry, used when a region
    /// produces no visible triangles.
    /// </summary>
    /// <param name="key">The key identifying the terrain region.</param>
    /// <returns>An empty <see cref="TerrainRegionBuildResult"/> with <see cref="HasGeometry"/> set to false.</returns>
    public static TerrainRegionBuildResult Empty(TerrainRegionKey key) {
        return new TerrainRegionBuildResult(key, null, null, default, 0, 0, false);
    }
}