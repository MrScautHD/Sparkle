namespace Sparkle.CSharp.Terrain.Generators;

public class FlatHeightmapGenerator : IHeightmapGenerator {
    
    /// <summary>
    /// The size of each generated chunk along the X and Z axes.
    /// </summary>
    private int _chunkSize;
    
    /// <summary>
    /// The uniform height assigned to every generated sample.
    /// </summary>
    private float _surfaceHeight;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="FlatHeightmapGenerator"/> class.
    /// </summary>
    /// <param name="chunkSize">The size of each generated chunk along the X and Z axes.</param>
    /// <param name="surfaceHeight">The uniform height assigned to every generated sample.</param>
    public FlatHeightmapGenerator(int chunkSize, float surfaceHeight) {
        this._chunkSize = chunkSize;
        this._surfaceHeight = surfaceHeight;
    }
    
    /// <summary>
    /// Generates a two-dimensional array containing a uniform flat surface.
    /// </summary>
    /// <param name="chunkX">The X coordinate of the chunk in the terrain grid.</param>
    /// <param name="chunkZ">The Z coordinate of the chunk in the terrain grid.</param>
    /// <returns>A completed task containing the generated height samples.</returns>
    public Task<float[,]> GenerateAsync(int chunkX, int chunkZ) {
        float[,] heights = new float[this._chunkSize + 1, this._chunkSize + 1];
        
        for (int localX = 0; localX <= this._chunkSize; localX++) {
            for (int localZ = 0; localZ <= this._chunkSize; localZ++) {
                heights[localX, localZ] = this._surfaceHeight;
            }
        }
        
        return Task.FromResult(heights);
    }
}