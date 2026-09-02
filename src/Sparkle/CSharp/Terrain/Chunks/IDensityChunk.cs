namespace Sparkle.CSharp.Terrain.Chunks;

public interface IDensityChunk : IChunk<IDensityChunk> {
    
    /// <summary>
    /// The chunk-local density samples.
    /// </summary>
    float[,,] Densities { get; }
    
    /// <summary>
    /// Retrieves the density at the specified chunk-local coordinate.
    /// </summary>
    /// <param name="localX">The local X coordinate.</param>
    /// <param name="localY">The local Y coordinate.</param>
    /// <param name="localZ">The local Z coordinate.</param>
    /// <returns>The density at the specified coordinate.</returns>
    float GetDensityAt(int localX, int localY, int localZ);
    
    /// <summary>
    /// Sets the density at the specified chunk-local coordinate and marks the chunk as dirty.
    /// </summary>
    /// <param name="localX">The local X coordinate.</param>
    /// <param name="localY">The local Y coordinate.</param>
    /// <param name="localZ">The local Z coordinate.</param>
    /// <param name="density">The new density value.</param>
    void SetDensityAt(int localX, int localY, int localZ, float density);
}