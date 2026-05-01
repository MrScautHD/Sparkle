namespace Sparkle.CSharp.Terrain;

public interface IChunkGenerator {
    
    /// <summary>
    /// Asynchronously generates a 3D density array for the chunk at the given
    /// column coordinates. The returned array is indexed as
    /// <c>[localX, localY, localZ]</c>, where values above the terrain's iso-level
    /// are considered solid and values below are considered empty.
    /// </summary>
    /// <param name="x">The chunk column index along the X axis.</param>
    /// <param name="y">The chunk column index along the Z axis.</param>
    /// <returns>
    /// A task that resolves to a <c>float[,,]</c> density array sized
    /// <c>[chunkSize + 1, height + 1, chunkSize + 1]</c> to include shared
    /// border vertices with neighbouring chunks.
    /// </returns>
    Task<float[,,]> GenerateAsync(int x, int y);
}