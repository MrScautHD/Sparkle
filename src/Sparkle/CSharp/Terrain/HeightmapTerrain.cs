using System.Numerics;
using Bliss.CSharp.Materials;
using Sparkle.CSharp.Terrain.Chunks;
using Sparkle.CSharp.Terrain.Generators;

namespace Sparkle.CSharp.Terrain;

public class HeightmapTerrain : ITerrain {
    
    /// <summary>
    /// The generator used to create the initial height data for each terrain chunk.
    /// </summary>
    public IHeightmapGenerator HeightmapGenerator { get; private set; }
    
    /// <summary>
    /// The material used when rendering terrain meshes.
    /// </summary>
    public Material Material { get; private set; }
    
    /// <summary>
    /// The total terrain width along the X axis.
    /// </summary>
    public int Width { get; private set; }
    
    /// <summary>
    /// The total terrain height along the Y axis.
    /// </summary>
    public int Height { get; private set; }
    
    /// <summary>
    /// The total terrain depth along the Z axis.
    /// </summary>
    public int Depth { get; private set; }
    
    /// <summary>
    /// The size of each terrain chunk along the X and Z axes.
    /// </summary>
    public int ChunkSize { get; private set; }
    
    /// <summary>
    /// The density value used as the terrain surface threshold.
    /// </summary>
    public float IsoLevel { get; private set; }
    
    /// <summary>
    /// The number of terrain chunks along the X axis.
    /// </summary>
    private int _chunkCountX;
    
    /// <summary>
    /// The number of terrain chunks along the Z axis.
    /// </summary>
    private int _chunkCountZ;
    
    /// <summary>
    /// A flat list containing all terrain chunks.
    /// </summary>
    private List<IChunk> _chunks;
    
    /// <summary>
    /// A grid used to retrieve chunks by their chunk coordinates.
    /// </summary>
    private HeightmapChunk[,] _chunkGrid;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="HeightmapTerrain"/> class.
    /// </summary>
    /// <param name="generator">The generator used to create initial chunk height data.</param>
    /// <param name="material">The material used for terrain rendering.</param>
    /// <param name="width">The terrain width along the X axis.</param>
    /// <param name="height">The terrain height along the Y axis.</param>
    /// <param name="depth">The terrain depth along the Z axis.</param>
    /// <param name="chunkSize">The size of each chunk along the X and Z axes.</param>
    /// <param name="isoLevel">The density value used as the terrain surface threshold.</param>
    private HeightmapTerrain(IHeightmapGenerator generator, Material material, int width, int height, int depth, int chunkSize, float isoLevel) {
        this.HeightmapGenerator = generator;
        this.Material = material;
        this.Width = width;
        this.Height = height;
        this.Depth = depth;
        this.ChunkSize = chunkSize;
        this.IsoLevel = isoLevel;
        this._chunkCountX = Math.Max(1, (int)Math.Ceiling(width / (float)chunkSize));
        this._chunkCountZ = Math.Max(1, (int)Math.Ceiling(depth / (float)chunkSize));
        this._chunks = new List<IChunk>(this._chunkCountX * this._chunkCountZ);
        this._chunkGrid = new HeightmapChunk[this._chunkCountX, this._chunkCountZ];
    }
    
    /// <summary>
    /// Creates a heightmap terrain and initializes all chunks asynchronously.
    /// </summary>
    /// <param name="generator">The generator used to create initial chunk height data.</param>
    /// <param name="material">The material used for terrain rendering.</param>
    /// <param name="width">The terrain width along the X axis.</param>
    /// <param name="height">The terrain height along the Y axis.</param>
    /// <param name="depth">The terrain depth along the Z axis.</param>
    /// <param name="chunkSize">The size of each chunk along the X and Z axes.</param>
    /// <param name="isoLevel">The density value used as the terrain surface threshold.</param>
    /// <returns>The created and initialized heightmap terrain.</returns>
    public static async Task<HeightmapTerrain> CreateAsync(IHeightmapGenerator generator, Material material, int width, int height, int depth, int chunkSize, float isoLevel = 0.0F) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(depth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chunkSize);
        
        HeightmapTerrain terrain = new HeightmapTerrain(generator, material, width, height, depth, chunkSize, isoLevel);
        await terrain.CreateChunks();
        return terrain;
    }
    
    /// <summary>
    /// Returns all chunks that make up this terrain.
    /// </summary>
    /// <returns>A read-only list containing all terrain chunks.</returns>
    public IReadOnlyList<IChunk> GetChunks() {
        return this._chunks;
    }
    
    /// <summary>
    /// Retrieves the chunk at the specified chunk-grid coordinate.
    /// </summary>
    /// <param name="chunkX">The X coordinate of the chunk in the terrain grid.</param>
    /// <param name="chunkZ">The Z coordinate of the chunk in the terrain grid.</param>
    /// <returns>The chunk at the specified coordinate, or <c>null</c> when outside the terrain bounds.</returns>
    public IChunk? GetChunk(int chunkX, int chunkZ) {
        if (chunkX < 0 || chunkX >= this._chunkCountX || chunkZ < 0 || chunkZ >= this._chunkCountZ) {
            return null;
        }
        
        return this._chunkGrid[chunkX, chunkZ];
    }
    
    /// <summary>
    /// Returns all chunks that are currently marked dirty and need a mesh rebuild.
    /// </summary>
    /// <returns>An enumerable containing all dirty terrain chunks.</returns>
    public IEnumerable<IChunk> GetDirtyChunks() {
        foreach (IChunk chunk in this._chunks) {
            if (chunk.IsDirty) {
                yield return chunk;
            }
        }
    }
    
    /// <summary>
    /// Gets the interpolated density at the given terrain position.
    /// Positive values are below the surface, while negative values are above the surface.
    /// </summary>
    /// <param name="position">The terrain-space position to sample.</param>
    /// <returns>The interpolated density value, or -1 when outside the terrain bounds.</returns>
    public float GetDensityAt(Vector3 position) {
        if (!this.Contains(position)) {
            return -1.0F;
        }
        
        int lowerX = (int)MathF.Floor(position.X);
        int lowerZ = (int)MathF.Floor(position.Z);
        int upperX = Math.Min(lowerX + 1, this.Width);
        int upperZ = Math.Min(lowerZ + 1, this.Depth);
        float lowerHeight = float.Lerp(this.GetSurfaceHeight(lowerX, lowerZ), this.GetSurfaceHeight(upperX, lowerZ), position.X - lowerX);
        float upperHeight = float.Lerp(this.GetSurfaceHeight(lowerX, upperZ), this.GetSurfaceHeight(upperX, upperZ), position.X - lowerX);
        
        return float.Lerp(lowerHeight, upperHeight, position.Z - lowerZ) - position.Y;
    }
    
    /// <summary>
    /// Calculates the raw density at an integer terrain coordinate from the stored surface height.
    /// </summary>
    /// <param name="x">The X coordinate to sample.</param>
    /// <param name="y">The Y coordinate to sample.</param>
    /// <param name="z">The Z coordinate to sample.</param>
    /// <returns>The calculated density value, or -1 when outside the terrain bounds.</returns>
    public float GetRawDensityAt(int x, int y, int z) {
        if (x < 0 || x > this.Width || y < 0 || y > this.Height || z < 0 || z > this.Depth) {
            return -1.0F;
        }
        
        return this.GetSurfaceHeight(x, z) - y;
    }
    
    /// <summary>
    /// Fills the terrain with a completely solid or empty height state based on the specified density.
    /// </summary>
    /// <param name="density">The density used to determine whether the terrain is solid or empty.</param>
    public void Fill(float density) {
        float surfaceHeight = density >= this.IsoLevel ? this.Height : -1.0F;
        this.ApplyFlatSurface(surfaceHeight);
    }
    
    /// <summary>
    /// Applies a flat horizontal surface height across the entire terrain and marks all chunks dirty.
    /// </summary>
    /// <param name="surfaceHeight">The new surface height.</param>
    public void ApplyFlatSurface(float surfaceHeight) {
        foreach (HeightmapChunk chunk in this._chunks.Cast<HeightmapChunk>()) {
            for (int localX = 0; localX < chunk.Heights.GetLength(0); localX++) {
                for (int localZ = 0; localZ < chunk.Heights.GetLength(1); localZ++) {
                    chunk.Heights[localX, localZ] = surfaceHeight;
                }
            }
            
            chunk.MarkDirty();
        }
    }
    
    /// <summary>
    /// Applies a circular height brush to the terrain surface and marks affected chunks dirty.
    /// Positive strength raises terrain, while negative strength lowers terrain.
    /// </summary>
    /// <param name="center">The terrain-space center of the brush.</param>
    /// <param name="radius">The radius of the brush.</param>
    /// <param name="strength">The height delta applied at the center of the brush.</param>
    /// <returns><c>true</c> when at least one height sample was modified; otherwise <c>false</c>.</returns>
    public bool ApplyBrush(Vector3 center, float radius, float strength) {
        if (radius <= 0.0F || MathF.Abs(strength) <= float.Epsilon) {
            return false;
        }
        
        int minimumX = Math.Max(0, (int)MathF.Floor(center.X - radius));
        int maximumX = Math.Min(this.Width, (int)MathF.Ceiling(center.X + radius));
        int minimumZ = Math.Max(0, (int)MathF.Floor(center.Z - radius));
        int maximumZ = Math.Min(this.Depth, (int)MathF.Ceiling(center.Z + radius));
        float radiusSquared = radius * radius;
        bool changed = false;
        
        for (int worldX = minimumX; worldX <= maximumX; worldX++) {
            for (int worldZ = minimumZ; worldZ <= maximumZ; worldZ++) {
                float offsetX = worldX - center.X;
                float offsetZ = worldZ - center.Z;
                float distanceSquared = offsetX * offsetX + offsetZ * offsetZ;
                if (distanceSquared > radiusSquared) {
                    continue;
                }
                
                float height = this.GetSurfaceHeight(worldX, worldZ) + strength * (1.0F - MathF.Sqrt(distanceSquared) / radius);
                this.SetSurfaceHeight(worldX, worldZ, height);
                changed = true;
            }
        }
        
        return changed;
    }
    
    /// <summary>
    /// Clears the terrain using the specified density value.
    /// </summary>
    /// <param name="density">The density used to determine the cleared height state.</param>
    public void Clear(float density = -1.0F) {
        this.Fill(density);
    }
    
    /// <summary>
    /// Calculates the terrain surface normal at the given position from neighboring height samples.
    /// </summary>
    /// <param name="position">The terrain-space position to sample.</param>
    /// <returns>The normalized surface normal.</returns>
    public Vector3 CalculateNormal(Vector3 position) {
        int x = Math.Clamp((int)MathF.Round(position.X), 0, this.Width);
        int z = Math.Clamp((int)MathF.Round(position.Z), 0, this.Depth);
        int previousX = Math.Max(x - 1, 0);
        int nextX = Math.Min(x + 1, this.Width);
        int previousZ = Math.Max(z - 1, 0);
        int nextZ = Math.Min(z + 1, this.Depth);
        
        Vector3 normal = new Vector3(this.GetSurfaceHeight(previousX, z) - this.GetSurfaceHeight(nextX, z), nextX - previousX + nextZ - previousZ, this.GetSurfaceHeight(x, previousZ) - this.GetSurfaceHeight(x, nextZ));
        
        if (normal.LengthSquared() <= 1.0E-10F) {
            return Vector3.UnitY;
        }
        
        return Vector3.Normalize(normal);
    }
    
    /// <summary>
    /// Casts a ray through the terrain and returns the first point where it crosses the surface.
    /// </summary>
    /// <param name="origin">The terrain-space ray origin.</param>
    /// <param name="direction">The ray direction.</param>
    /// <param name="maxDistance">The maximum distance to march along the ray.</param>
    /// <param name="stepSize">The distance between density samples.</param>
    /// <param name="hitPosition">The interpolated terrain-space hit position.</param>
    /// <param name="hitNormal">The surface normal at the hit position.</param>
    /// <returns><c>true</c> when the ray intersects the terrain surface; otherwise <c>false</c>.</returns>
    public bool RaycastSurface(Vector3 origin, Vector3 direction, float maxDistance, float stepSize, out Vector3 hitPosition, out Vector3 hitNormal) {
        hitPosition = Vector3.Zero;
        hitNormal = Vector3.UnitY;
        if (maxDistance <= 0.0F || stepSize <= 0.0F || direction.LengthSquared() <= 1.0E-10F) {
            return false;
        }
        
        Vector3 rayDirection = Vector3.Normalize(direction);
        Vector3 previousPoint = origin;
        float previousDensity = this.GetDensityAt(previousPoint) - this.IsoLevel;
        
        for (float distance = stepSize; distance <= maxDistance; distance += stepSize) {
            Vector3 currentPoint = origin + rayDirection * distance;
            float currentDensity = this.GetDensityAt(currentPoint) - this.IsoLevel;
            if (previousDensity * currentDensity <= 0.0F) {
                Vector3 lowerPoint = previousPoint;
                Vector3 upperPoint = currentPoint;
                float lowerDensity = previousDensity;
                
                for (int iteration = 0; iteration < 8; iteration++) {
                    Vector3 midpoint = (lowerPoint + upperPoint) * 0.5F;
                    float midpointDensity = this.GetDensityAt(midpoint) - this.IsoLevel;
                    if (lowerDensity * midpointDensity <= 0.0F) {
                        upperPoint = midpoint;
                    }
                    else {
                        lowerPoint = midpoint;
                        lowerDensity = midpointDensity;
                    }
                }
                
                hitPosition = (lowerPoint + upperPoint) * 0.5F;
                hitNormal = this.CalculateNormal(hitPosition);
                return true;
            }
            previousPoint = currentPoint;
            previousDensity = currentDensity;
        }
        
        return false;
    }
    
    /// <summary>
    /// Checks whether the given terrain-space position lies inside the terrain bounds.
    /// </summary>
    /// <param name="position">The position to check.</param>
    /// <returns><c>true</c> when the position lies inside the terrain bounds; otherwise <c>false</c>.</returns>
    public bool Contains(Vector3 position) {
        return position.X >= 0.0F && position.X <= this.Width &&
               position.Y >= 0.0F && position.Y <= this.Height &&
               position.Z >= 0.0F && position.Z <= this.Depth;
    }
    
    /// <summary>
    /// Gets the stored surface height at the specified terrain coordinate.
    /// </summary>
    /// <param name="x">The terrain X coordinate.</param>
    /// <param name="z">The terrain Z coordinate.</param>
    /// <returns>The stored surface height, or -1 when outside the terrain bounds.</returns>
    public float GetSurfaceHeight(int x, int z) {
        if (!this.TryGetHeightLocation(x, z, out HeightmapChunk? chunk, out int localX, out int localZ)) {
            return -1.0F;
        }
        
        return chunk!.GetHeightAt(localX, localZ);
    }
    
    /// <summary>
    /// Sets the surface height at the specified terrain coordinate and synchronizes shared chunk-edge samples.
    /// </summary>
    /// <param name="x">The terrain X coordinate.</param>
    /// <param name="z">The terrain Z coordinate.</param>
    /// <param name="height">The new surface height.</param>
    public void SetSurfaceHeight(int x, int z, float height) {
        if (!this.TryGetHeightLocation(x, z, out HeightmapChunk? chunk, out int localX, out int localZ)) {
            return;
        }
        
        chunk!.SetHeightAt(localX, localZ, height);
        this.SetMirroredHeight(chunk.ChunkX - 1, chunk.ChunkZ, x, z, height);
        this.SetMirroredHeight(chunk.ChunkX + 1, chunk.ChunkZ, x, z, height);
        this.SetMirroredHeight(chunk.ChunkX, chunk.ChunkZ - 1, x, z, height);
        this.SetMirroredHeight(chunk.ChunkX, chunk.ChunkZ + 1, x, z, height);
    }
    
    /// <summary>
    /// Retrieves a neighboring chunk relative to the specified chunk-grid offsets.
    /// </summary>
    /// <param name="chunk">The chunk from which the neighbor is determined.</param>
    /// <param name="offsetX">The chunk offset along the X axis.</param>
    /// <param name="offsetZ">The chunk offset along the Z axis.</param>
    /// <returns>The neighboring chunk, or <c>null</c> when outside the terrain bounds.</returns>
    public IChunk? GetNeighborChunk(IChunk chunk, int offsetX, int offsetZ) {
        return this.GetChunk(chunk.ChunkX + offsetX, chunk.ChunkZ + offsetZ);
    }
    
    /// <summary>
    /// Marks every terrain chunk as dirty so all chunk meshes will be rebuilt.
    /// </summary>
    public void MarkAllChunksDirty() {
        foreach (IChunk chunk in this._chunks) {
            chunk.MarkDirty();
        }
    }
    
    /// <summary>
    /// Resolves a terrain coordinate to its owning chunk and chunk-local coordinate.
    /// </summary>
    /// <param name="x">The terrain X coordinate.</param>
    /// <param name="z">The terrain Z coordinate.</param>
    /// <param name="chunk">The chunk containing the coordinate.</param>
    /// <param name="localX">The resolved chunk-local X coordinate.</param>
    /// <param name="localZ">The resolved chunk-local Z coordinate.</param>
    /// <returns><c>true</c> when the coordinate resolves to a height sample; otherwise <c>false</c>.</returns>
    private bool TryGetHeightLocation(int x, int z, out HeightmapChunk? chunk, out int localX, out int localZ) {
        chunk = null;
        localX = 0;
        localZ = 0;
        
        if (x < 0 || x > this.Width || z < 0 || z > this.Depth) {
            return false;
        }
        
        int chunkX = Math.Min(x / this.ChunkSize, this._chunkCountX - 1);
        int chunkZ = Math.Min(z / this.ChunkSize, this._chunkCountZ - 1);
        
        chunk = this._chunkGrid[chunkX, chunkZ];
        localX = x - chunkX * this.ChunkSize;
        localZ = z - chunkZ * this.ChunkSize;
        
        return localX < chunk.Heights.GetLength(0) && localZ < chunk.Heights.GetLength(1);
    }
    
    /// <summary>
    /// Updates a duplicated height sample when the specified world coordinate belongs to a neighboring chunk edge.
    /// </summary>
    /// <param name="chunkX">The neighboring chunk X coordinate.</param>
    /// <param name="chunkZ">The neighboring chunk Z coordinate.</param>
    /// <param name="worldX">The terrain X coordinate of the height sample.</param>
    /// <param name="worldZ">The terrain Z coordinate of the height sample.</param>
    /// <param name="height">The new height value.</param>
    private void SetMirroredHeight(int chunkX, int chunkZ, int worldX, int worldZ, float height) {
        if (this.GetChunk(chunkX, chunkZ) is not HeightmapChunk chunk) {
            return;
        }
        
        int localX = worldX - chunkX * this.ChunkSize;
        int localZ = worldZ - chunkZ * this.ChunkSize;
        
        if (localX >= 0 && localX < chunk.Heights.GetLength(0) &&
            localZ >= 0 && localZ < chunk.Heights.GetLength(1)) {
            chunk.SetHeightAt(localX, localZ, height);
        }
    }
    
    /// <summary>
    /// Generates the height data and creates every terrain chunk asynchronously.
    /// </summary>
    private async Task CreateChunks() {
        int totalChunkCount = this._chunkCountX * this._chunkCountZ;
        IChunk[] createdChunks = new IChunk[totalChunkCount];
        
        ParallelOptions parallelOptions = new ParallelOptions {
            MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 1)
        };
        
        await Parallel.ForEachAsync(Enumerable.Range(0, totalChunkCount), parallelOptions, async (chunkIndex, cancellationToken) => {
            int chunkX = chunkIndex % this._chunkCountX;
            int chunkZ = chunkIndex / this._chunkCountX;
            
            int chunkStartX = chunkX * this.ChunkSize;
            int chunkStartZ = chunkZ * this.ChunkSize;
            
            int chunkWidth = Math.Min(this.ChunkSize, this.Width - chunkStartX);
            int chunkDepth = Math.Min(this.ChunkSize, this.Depth - chunkStartZ);
            
            float[,] heights = await this.HeightmapGenerator.GenerateAsync(chunkX, chunkZ);
            
            if (heights.GetLength(0) < chunkWidth + 1 || heights.GetLength(1) < chunkDepth + 1) {
                throw new InvalidOperationException($"Height generator returned insufficient data for chunk ({chunkX}, {chunkZ}).");
            }
            
            HeightmapChunk chunk = new HeightmapChunk(this, new Vector3(chunkStartX, 0.0F, chunkStartZ), chunkWidth, this.Height, chunkDepth, chunkX, chunkZ, heights);
            
            this._chunkGrid[chunkX, chunkZ] = chunk;
            createdChunks[chunkIndex] = chunk;
        });
        
        this._chunks.AddRange(createdChunks);
    }
}