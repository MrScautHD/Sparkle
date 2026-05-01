namespace Sparkle.CSharp.Terrain;

public interface IChunkGenerator {
    
    /// <summary>
    /// Generates a 3D density volume asynchronously for a chunk of terrain.
    /// </summary>
    /// <param name="x">The X coordinate of the chunk in the terrain grid. This parameter is used to determine the chunk's position.</param>
    /// <param name="y">The Z coordinate of the chunk in the terrain grid. This parameter is used to determine the chunk's position.</param>
    /// <returns>A task representing the asynchronous operation that returns a 3D float array containing density values for the chunk.</returns>
    Task<float[,,]> GenerateAsync(int x, int y);
}