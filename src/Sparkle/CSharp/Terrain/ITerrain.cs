using System.Numerics;
using Bliss.CSharp.Materials;

namespace Sparkle.CSharp.Terrain;

public interface ITerrain {
    
    /// <summary>
    /// The generator used to produce density data for each terrain column.
    /// </summary>
    IChunkGenerator ChunkGenerator { get; }
    
    /// <summary>
    /// The material used for rendering the terrain, which defines its visual appearance and surface properties.
    /// </summary>
    Material Material { get; }
    
    /// <summary>
    /// The total width of the terrain in voxels.
    /// </summary>
    int Width { get; }
    
    /// <summary>
    /// The total height of the terrain in voxels.
    /// </summary>
    int Height { get; }
    
    /// <summary>
    /// The total depth of the terrain in voxels.
    /// </summary>
    int Depth { get; }
    
    /// <summary>
    /// The size of each chunk in voxels along every axis.
    /// </summary>
    int ChunkSize { get; }
    
    /// <summary>
    /// The density threshold at which the marching cubes algorithm places a surface.
    /// </summary>
    float IsoLevel { get; }
    
    /// <summary>
    /// Returns all chunks that subdivide the terrain volume.
    /// </summary>
    IReadOnlyList<IChunk> GetChunks();
    
    /// <summary>
    /// Returns all chunks that are currently marked dirty and need a mesh rebuild.
    /// </summary>
    IEnumerable<IChunk> GetDirtyChunks();
    
    /// <summary>
    /// Returns the trilinearly interpolated density at <paramref name="position"/>, or -1 when out of bounds.
    /// </summary>
    /// <param name="position">The world-space position to sample.</param>
    float GetDensityAt(Vector3 position);
    
    /// <summary>
    /// Returns the raw density value at the integer voxel coordinate, or -1 when out of bounds.
    /// </summary>
    /// <param name="x">The X voxel coordinate.</param>
    /// <param name="y">The Y voxel coordinate.</param>
    /// <param name="z">The Z voxel coordinate.</param>
    float GetRawDensityAt(int x, int y, int z);
    
    /// <summary>
    /// Sets every voxel in the terrain to <paramref name="density"/> and marks all chunks dirty.
    /// </summary>
    /// <param name="density">The density value to assign to every voxel.</param>
    void Fill(float density);
    
    /// <summary>
    /// Fills the terrain with a flat horizontal surface at <paramref name="surfaceHeight"/> and marks all chunks dirty.
    /// </summary>
    /// <param name="surfaceHeight">The Y position of the flat surface.</param>
    void ApplyFlatSurface(float surfaceHeight);
    
    /// <summary>
    /// Adds a spherical density falloff centered at <paramref name="center"/> to all voxels within <paramref name="radius"/>,
    /// then marks affected chunks dirty. Returns <c>true</c> if any voxel was modified.
    /// </summary>
    /// <param name="center">The world-space center of the brush.</param>
    /// <param name="radius">The radius of the brush in voxels.</param>
    /// <param name="strength">The density delta applied at the brush center. Negative values remove material.</param>
    bool ApplyBrush(Vector3 center, float radius, float strength);
    
    /// <summary>
    /// Resets the entire terrain to <paramref name="density"/> and marks all chunks dirty.
    /// </summary>
    /// <param name="density">The density value to fill with. Defaults to -1.</param>
    void Clear(float density = -1.0F);
    
    /// <summary>
    /// Computes the surface normal at <paramref name="position"/> by sampling the density gradient.
    /// </summary>
    /// <param name="position">The world-space position to evaluate the normal at.</param>
    /// <returns>The normalized surface normal, or <see cref="Vector3.UnitY"/> when the gradient is zero.</returns>
    Vector3 CalculateNormal(Vector3 position);
    
    /// <summary>
    /// Casts a ray through the terrain and returns the first point where it crosses the iso-surface.
    /// </summary>
    /// <param name="origin">The world-space ray origin.</param>
    /// <param name="direction">The ray direction (does not need to be normalized).</param>
    /// <param name="maxDistance">The maximum distance to march along the ray.</param>
    /// <param name="stepSize">The distance between each density sample along the ray.</param>
    /// <param name="hitPosition">The interpolated world-space surface hit position.</param>
    /// <param name="hitNormal">The surface normal at the hit position.</param>
    /// <returns><c>true</c> if the ray hit the surface; otherwise <c>false</c>.</returns>
    bool RaycastSurface(Vector3 origin, Vector3 direction, float maxDistance, float stepSize, out Vector3 hitPosition, out Vector3 hitNormal);
    
    /// <summary>
    /// Returns <c>true</c> when <paramref name="position"/> lies within the terrain bounds.
    /// </summary>
    /// <param name="position">The world-space position to test.</param>
    bool Contains(Vector3 position);
    
    /// <summary>
    /// Marks every chunk in the terrain as dirty.
    /// </summary>
    void MarkAllChunksDirty();
}
