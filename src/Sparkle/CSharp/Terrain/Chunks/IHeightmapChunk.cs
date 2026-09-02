namespace Sparkle.CSharp.Terrain.Chunks;

public interface IHeightmapChunk : IChunk<IHeightmapChunk> {
    
    /// <summary>
    /// The chunk-local height samples, including the positive X and Z edge samples.
    /// </summary>
    float[,] Heights { get; }
    
    /// <summary>
    /// Retrieves the height at the specified chunk-local coordinate.
    /// </summary>
    /// <param name="localX">The local X coordinate.</param>
    /// <param name="localZ">The local Z coordinate.</param>
    /// <returns>The height at the specified coordinate.</returns>
    float GetHeightAt(int localX, int localZ);
    
    /// <summary>
    /// Sets the height at the specified chunk-local coordinate and marks the chunk as dirty.
    /// </summary>
    /// <param name="localX">The local X coordinate.</param>
    /// <param name="localZ">The local Z coordinate.</param>
    /// <param name="height">The new height value.</param>
    void SetHeightAt(int localX, int localZ, float height);
}