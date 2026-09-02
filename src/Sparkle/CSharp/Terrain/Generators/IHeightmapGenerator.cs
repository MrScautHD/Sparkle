namespace Sparkle.CSharp.Terrain.Generators;

public interface IHeightmapGenerator {
    
    /// <summary>
    /// Generates height samples for the chunk at the specified chunk-grid coordinate.
    /// </summary>
    /// <param name="chunkX">The X coordinate of the chunk in the terrain grid.</param>
    /// <param name="chunkZ">The Z coordinate of the chunk in the terrain grid.</param>
    /// <returns>A task containing the generated two-dimensional height samples.</returns>
    Task<float[,]> GenerateAsync(int chunkX, int chunkZ);
}