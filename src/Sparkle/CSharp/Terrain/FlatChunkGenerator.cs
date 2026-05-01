namespace Sparkle.CSharp.Terrain;

public class FlatChunkGenerator : IChunkGenerator {
    
    /// <summary>
    /// The size of each chunk in voxels along the X and Z axes.
    /// </summary>
    private int _chunkSize;
    
    /// <summary>
    /// The total height of the density volume in voxels.
    /// </summary>
    private int _height;
    
    /// <summary>
    /// The Y position of the flat surface within the density volume.
    /// </summary>
    private float _surfaceHeight;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="FlatChunkGenerator"/> class.
    /// </summary>
    /// <param name="chunkSize">The size of each chunk in voxels along the X and Z axes.</param>
    /// <param name="height">The total height of the density volume in voxels.</param>
    /// <param name="surfaceHeight">The Y position of the flat surface.</param>
    public FlatChunkGenerator(int chunkSize, int height, float surfaceHeight) {
        this._chunkSize = chunkSize;
        this._height = height;
        this._surfaceHeight = surfaceHeight;
    }
    
    /// <summary>
    /// Returns a completed task containing a density volume with a flat surface at <see cref="_surfaceHeight"/>.
    /// </summary>
    /// <param name="x">The chunk X coordinate (unused, terrain is uniform).</param>
    /// <param name="y">The chunk Z coordinate (unused, terrain is uniform).</param>
    public Task<float[,,]> GenerateAsync(int x, int y) {
        float[,,] densities = new float[this._chunkSize + 1, this._height + 1, this._chunkSize + 1];
        
        for (int localX = 0; localX <= this._chunkSize; localX++) {
            for (int localY = 0; localY <= this._height; localY++) {
                float density = this._surfaceHeight - localY;
                
                for (int localZ = 0; localZ <= this._chunkSize; localZ++) {
                    densities[localX, localY, localZ] = density;
                }
            }
        }
        
        return Task.FromResult(densities);
    }
}