namespace Sparkle.CSharp.Terrain.Generators;

public interface IDensityGenerator : ITerrainGenerator {
    
    /// <summary>
    /// Generates density samples for the chunk at the specified chunk-grid coordinate.
    /// </summary>
    /// <param name="chunkX">The X coordinate of the chunk in the terrain grid.</param>
    /// <param name="chunkZ">The Z coordinate of the chunk in the terrain grid.</param>
    /// <returns>A task containing the generated three-dimensional density samples.</returns>
    Task<float[,,]> GenerateAsync(int chunkX, int chunkZ);
}