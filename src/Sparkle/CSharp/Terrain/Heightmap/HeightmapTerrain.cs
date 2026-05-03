using System.Numerics;
using Bliss.CSharp.Materials;

namespace Sparkle.CSharp.Terrain.Heightmap;

public class HeightmapTerrain : ITerrain {
    
    /// <summary>
    /// The generator used to create the initial chunk density data.
    /// </summary>
    public IChunkGenerator ChunkGenerator { get; private set; }
    
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
    /// The density value used as the surface threshold.
    /// </summary>
    public float IsoLevel { get; private set; }
    
    /// <summary>
    /// Stores the surface height for every X/Z terrain coordinate.
    /// </summary>
    private float[,] _surfaceHeights;
    
    /// <summary>
    /// Number of chunks along the X axis.
    /// </summary>
    private int _chunkCountX;
    
    /// <summary>
    /// Number of chunks along the Z axis.
    /// </summary>
    private int _chunkCountZ;
    
    /// <summary>
    /// Flat list containing all terrain chunks.
    /// </summary>
    private List<IChunk> _chunks;
    
    /// <summary>
    /// Grid lookup for terrain chunks by chunk X/Z coordinates.
    /// </summary>
    private HeightmapChunk[,] _chunkGrid;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="HeightmapTerrain"/> class.
    /// </summary>
    /// <param name="chunkGenerator">The generator used to create initial chunk density data.</param>
    /// <param name="material">The material used for terrain rendering.</param>
    /// <param name="width">The terrain width along the X axis.</param>
    /// <param name="height">The terrain height along the Y axis.</param>
    /// <param name="depth">The terrain depth along the Z axis.</param>
    /// <param name="chunkSize">The size of each chunk along the X and Z axes.</param>
    /// <param name="isoLevel">The density value used as the surface threshold.</param>
    private HeightmapTerrain(IChunkGenerator chunkGenerator, Material material, int width, int height, int depth, int chunkSize, float isoLevel) {
        this.ChunkGenerator = chunkGenerator;
        this.Material = material;
        this.Width = width;
        this.Height = height;
        this.Depth = depth;
        this.ChunkSize = chunkSize;
        this.IsoLevel = isoLevel;
        this._surfaceHeights = new float[width + 1, depth + 1];
        
        this._chunkCountX = Math.Max(1, (int) Math.Ceiling(width / (float) chunkSize));
        this._chunkCountZ = Math.Max(1, (int) Math.Ceiling(depth / (float) chunkSize));
        
        this._chunks = new List<IChunk>(this._chunkCountX * this._chunkCountZ);
        this._chunkGrid = new HeightmapChunk[this._chunkCountX, this._chunkCountZ];
    }
    
    /// <summary>
    /// Creates a heightmap terrain and initializes all chunks asynchronously.
    /// </summary>
    /// <param name="chunkGenerator">The generator used to create initial chunk density data.</param>
    /// <param name="material">The material used for terrain rendering.</param>
    /// <param name="width">The terrain width along the X axis.</param>
    /// <param name="height">The terrain height along the Y axis.</param>
    /// <param name="depth">The terrain depth along the Z axis.</param>
    /// <param name="chunkSize">The size of each chunk along the X and Z axes.</param>
    /// <param name="isoLevel">The density value used as the surface threshold.</param>
    /// <returns>The created and initialized heightmap terrain.</returns>
    public static async Task<HeightmapTerrain> CreateAsync(IChunkGenerator chunkGenerator, Material material, int width, int height, int depth, int chunkSize, float isoLevel = 0.0F) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(depth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chunkSize);
        
        HeightmapTerrain terrain = new HeightmapTerrain(chunkGenerator, material, width, height, depth, chunkSize, isoLevel);
        await terrain.CreateChunks();
        return terrain;
    }
    
    /// <summary>
    /// Returns all chunks that make up this terrain.
    /// </summary>
    public IReadOnlyList<IChunk> GetChunks() {
        return this._chunks;
    }
    
    /// <summary>
    /// Returns all chunks that are currently marked dirty and need a mesh rebuild.
    /// </summary>
    public IEnumerable<IChunk> GetDirtyChunks() {
        foreach (IChunk chunk in this._chunks) {
            if (chunk.IsDirty) {
                yield return chunk;
            }
        }
    }
    
    /// <summary>
    /// Gets the interpolated density at the given terrain position.
    /// Positive values are below the surface, negative values are above the surface.
    /// </summary>
    /// <param name="position">The terrain-space position to sample.</param>
    /// <returns>The interpolated density value, or -1 when outside the terrain bounds.</returns>
    public float GetDensityAt(Vector3 position) {
        if (!this.Contains(position)) {
            return -1.0F;
        }
        
        int minX = (int) MathF.Floor(position.X);
        int minZ = (int) MathF.Floor(position.Z);
        
        int maxX = Math.Min(minX + 1, this.Width);
        int maxZ = Math.Min(minZ + 1, this.Depth);
        
        float xInterpolation = position.X - minX;
        float zInterpolation = position.Z - minZ;
        
        float heightMinXMinZ = this._surfaceHeights[minX, minZ];
        float heightMaxXMinZ = this._surfaceHeights[maxX, minZ];
        float heightMinXMaxZ = this._surfaceHeights[minX, maxZ];
        float heightMaxXMaxZ = this._surfaceHeights[maxX, maxZ];
        
        float heightAtMinZ = float.Lerp(heightMinXMinZ, heightMaxXMinZ, xInterpolation);
        float heightAtMaxZ = float.Lerp(heightMinXMaxZ, heightMaxXMaxZ, xInterpolation);
        float interpolatedSurfaceHeight = float.Lerp(heightAtMinZ, heightAtMaxZ, zInterpolation);
        
        return interpolatedSurfaceHeight - position.Y;
    }
    
    /// <summary>
    /// Gets the raw density value at an integer terrain coordinate.
    /// </summary>
    /// <param name="x">The X coordinate to sample.</param>
    /// <param name="y">The Y coordinate to sample.</param>
    /// <param name="z">The Z coordinate to sample.</param>
    /// <returns>The raw density value, or -1 when outside the terrain bounds.</returns>
    public float GetRawDensityAt(int x, int y, int z) {
        if (x < 0 || x > this.Width || y < 0 || y > this.Height || z < 0 || z > this.Depth) {
            return -1.0F;
        }
        
        return this._surfaceHeights[x, z] - y;
    }
    
    /// <summary>
    /// Fills the whole terrain with a solid or empty density state and marks all chunks dirty.
    /// </summary>
    /// <param name="density">The density value used to decide whether the terrain becomes filled or empty.</param>
    public void Fill(float density) {
        float targetHeight = density >= this.IsoLevel ? this.Height : -1.0F;
        
        for (int x = 0; x <= this.Width; x++) {
            for (int z = 0; z <= this.Depth; z++) {
                this._surfaceHeights[x, z] = targetHeight;
            }
        }
        
        this.MarkAllChunksDirty();
    }
    
    /// <summary>
    /// Applies a flat horizontal surface height across the entire terrain and marks all chunks dirty.
    /// </summary>
    /// <param name="surfaceHeight">The new surface height.</param>
    public void ApplyFlatSurface(float surfaceHeight) {
        for (int x = 0; x <= this.Width; x++) {
            for (int z = 0; z <= this.Depth; z++) {
                this._surfaceHeights[x, z] = surfaceHeight;
            }
        }
        
        this.MarkAllChunksDirty();
    }
    
    /// <summary>
    /// Applies a circular height brush to the terrain surface and marks affected chunks dirty.
    /// Positive strength raises terrain, negative strength lowers terrain.
    /// </summary>
    /// <param name="center">The terrain-space brush center.</param>
    /// <param name="radius">The brush radius.</param>
    /// <param name="strength">The height delta applied at the brush center.</param>
    /// <returns><c>true</c> when at least one height value was changed.</returns>
    public bool ApplyBrush(Vector3 center, float radius, float strength) {
        if (radius <= 0.0F || MathF.Abs(strength) <= float.Epsilon) {
            return false;
        }
        
        int minX = Math.Max(0, (int) MathF.Floor(center.X - radius));
        int maxX = Math.Min(this.Width, (int) MathF.Ceiling(center.X + radius));
        int minZ = Math.Max(0, (int) MathF.Floor(center.Z - radius));
        int maxZ = Math.Min(this.Depth, (int) MathF.Ceiling(center.Z + radius));
        
        float radiusSquared = radius * radius;
        
        bool changed = false;
        int changedMinX = int.MaxValue;
        int changedMaxX = int.MinValue;
        int changedMinZ = int.MaxValue;
        int changedMaxZ = int.MinValue;
        
        for (int x = minX; x <= maxX; x++) {
            for (int z = minZ; z <= maxZ; z++) {
                float currentHeight = this._surfaceHeights[x, z];
                Vector3 delta = new Vector3(x, currentHeight, z) - center;
                float distanceSquared = delta.LengthSquared();
                
                if (distanceSquared > radiusSquared) {
                    continue;
                }
                
                float falloff = 1.0F - MathF.Sqrt(distanceSquared) / radius;
                this._surfaceHeights[x, z] += strength * falloff;
                changed = true;
                changedMinX = Math.Min(changedMinX, x);
                changedMaxX = Math.Max(changedMaxX, x);
                changedMinZ = Math.Min(changedMinZ, z);
                changedMaxZ = Math.Max(changedMaxZ, z);
            }
        }
        
        if (!changed) {
            return false;
        }
        
        int chunkMinX = Math.Clamp(changedMinX / this.ChunkSize, 0, this._chunkCountX - 1);
        int chunkMaxX = Math.Clamp(changedMaxX / this.ChunkSize, 0, this._chunkCountX - 1);
        int chunkMinZ = Math.Clamp(changedMinZ / this.ChunkSize, 0, this._chunkCountZ - 1);
        int chunkMaxZ = Math.Clamp(changedMaxZ / this.ChunkSize, 0, this._chunkCountZ - 1);
        
        for (int chunkX = chunkMinX; chunkX <= chunkMaxX; chunkX++) {
            for (int chunkZ = chunkMinZ; chunkZ <= chunkMaxZ; chunkZ++) {
                this._chunkGrid[chunkX, chunkZ].MarkDirty();
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Clears the terrain by filling it with the given density value.
    /// </summary>
    /// <param name="density">The density value used for clearing. Defaults to empty terrain.</param>
    public void Clear(float density = -1.0F) {
        this.Fill(density);
    }
    
    /// <summary>
    /// Calculates an approximate terrain surface normal at the given position.
    /// </summary>
    /// <param name="position">The terrain-space position to sample.</param>
    /// <returns>The normalized surface normal.</returns>
    public Vector3 CalculateNormal(Vector3 position) {
        int sampleX = Math.Clamp((int) MathF.Round(position.X), 0, this.Width);
        int sampleZ = Math.Clamp((int) MathF.Round(position.Z), 0, this.Depth);
        
        int previousSampleX = Math.Max(0, sampleX - 1);
        int previousSampleZ = Math.Max(0, sampleZ - 1);
        
        int nextSampleX = Math.Min(this.Width, sampleX + 1);
        int nextSampleZ = Math.Min(this.Depth, sampleZ + 1);
        
        float heightDeltaX = this._surfaceHeights[nextSampleX, sampleZ] - this._surfaceHeights[previousSampleX, sampleZ];
        float heightDeltaZ = this._surfaceHeights[sampleX, nextSampleZ] - this._surfaceHeights[sampleX, previousSampleZ];
        
        Vector3 surfaceNormal = new Vector3(-heightDeltaX, 2.0F, -heightDeltaZ);
        
        if (surfaceNormal.LengthSquared() <= 1.0E-10F) {
            return Vector3.UnitY;
        }
        
        return Vector3.Normalize(surfaceNormal);
    }
    
    /// <summary>
    /// Raycasts against the heightmap surface and returns the first surface hit.
    /// </summary>
    /// <param name="origin">The terrain-space ray origin.</param>
    /// <param name="direction">The ray direction.</param>
    /// <param name="maxDistance">The maximum ray distance.</param>
    /// <param name="stepSize">The distance between density samples.</param>
    /// <param name="hitPosition">The resulting hit position when the ray hits the surface.</param>
    /// <param name="hitNormal">The resulting surface normal when the ray hits the surface.</param>
    /// <returns><c>true</c> when the ray hits the terrain surface.</returns>
    public bool RaycastSurface(Vector3 origin, Vector3 direction, float maxDistance, float stepSize, out Vector3 hitPosition, out Vector3 hitNormal) {
        hitPosition = Vector3.Zero;
        hitNormal = Vector3.UnitY;
        
        if (maxDistance <= 0.0F || stepSize <= 0.0F || direction.LengthSquared() <= 1.0E-10F) {
            return false;
        }
        
        Vector3 rayDirection = Vector3.Normalize(direction);
        Vector3 previousSamplePoint = origin;
        float previousSampleDensity = this.GetDensityAt(previousSamplePoint) - this.IsoLevel;
        
        for (float rayDistance = stepSize; rayDistance <= maxDistance; rayDistance += stepSize) {
            Vector3 currentSamplePoint = origin + rayDirection * rayDistance;
            float currentSampleDensity = this.GetDensityAt(currentSamplePoint) - this.IsoLevel;
            
            if (previousSampleDensity * currentSampleDensity <= 0.0F) {
                Vector3 lowerPoint = previousSamplePoint;
                Vector3 upperPoint = currentSamplePoint;
                float lowerDensity = previousSampleDensity;
                
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
            
            previousSamplePoint = currentSamplePoint;
            previousSampleDensity = currentSampleDensity;
        }
        
        return false;
    }
    
    /// <summary>
    /// Checks whether the given terrain-space position lies inside the terrain bounds.
    /// </summary>
    /// <param name="position">The position to check.</param>
    /// <returns><c>true</c> when the position is inside the terrain bounds.</returns>
    public bool Contains(Vector3 position) {
        return position.X >= 0.0F && position.X <= this.Width &&
               position.Y >= 0.0F && position.Y <= this.Height &&
               position.Z >= 0.0F && position.Z <= this.Depth;
    }
    
    /// <summary>
    /// Gets the stored surface height at the given X/Z coordinate.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="z">The Z coordinate.</param>
    /// <returns>The surface height, or -1 when outside the terrain bounds.</returns>
    public float GetSurfaceHeight(int x, int z) {
        if (x < 0 || x > this.Width || z < 0 || z > this.Depth) {
            return -1.0F;
        }
        
        return this._surfaceHeights[x, z];
    }
    
    /// <summary>
    /// Gets a neighboring chunk relative to the given chunk using chunk-grid offsets.
    /// </summary>
    /// <param name="chunk">The source chunk.</param>
    /// <param name="offsetX">Neighbor offset in chunk units along X.</param>
    /// <param name="offsetZ">Neighbor offset in chunk units along Z.</param>
    /// <returns>The neighboring chunk, or <c>null</c> when outside terrain bounds.</returns>
    public IChunk? GetNeighborChunk(IChunk chunk, int offsetX, int offsetZ) {
        int chunkX;
        int chunkZ;
        
        if (chunk is HeightmapChunk heightmapChunk) {
            chunkX = heightmapChunk.ChunkX;
            chunkZ = heightmapChunk.ChunkZ;
        }
        else {
            chunkX = (int) chunk.Position.X / this.ChunkSize;
            chunkZ = (int) chunk.Position.Z / this.ChunkSize;
        }
        
        int neighborChunkX = chunkX + offsetX;
        int neighborChunkZ = chunkZ + offsetZ;
        
        if (neighborChunkX < 0 || neighborChunkX >= this._chunkCountX ||
            neighborChunkZ < 0 || neighborChunkZ >= this._chunkCountZ) {
            return null;
        }
        
        return this._chunkGrid[neighborChunkX, neighborChunkZ];
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
    /// Creates all heightmap chunks and initializes the surface height data from generated density volumes.
    /// </summary>
    private async Task CreateChunks() {
        int totalChunkCount = this._chunkCountX * this._chunkCountZ;
        IChunk[] chunks = new IChunk[totalChunkCount];
        
        ParallelOptions options = new ParallelOptions {
            MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 1)
        };
        
        await Parallel.ForEachAsync(Enumerable.Range(0, totalChunkCount), options, async (index, cancellationToken) => {
            int chunkX = index % this._chunkCountX;
            int chunkZ = index / this._chunkCountX;
            
            int startX = chunkX * this.ChunkSize;
            int startZ = chunkZ * this.ChunkSize;
            
            int chunkWidth = Math.Min(this.ChunkSize, this.Width - startX);
            int chunkDepth = Math.Min(this.ChunkSize, this.Depth - startZ);
            
            float[,,] chunkData = await this.ChunkGenerator.GenerateAsync(chunkX, chunkZ);
            
            for (int localX = 0; localX <= chunkWidth; localX++) {
                if (chunkX < this._chunkCountX - 1 && localX == chunkWidth) {
                    continue;
                }
                
                int worldX = startX + localX;
                
                for (int localZ = 0; localZ <= chunkDepth; localZ++) {
                    if (chunkZ < this._chunkCountZ - 1 && localZ == chunkDepth) {
                        continue;
                    }
                    
                    int worldZ = startZ + localZ;
                    float previousDensity = chunkData[localX, this.Height, localZ];
                    float surfaceHeight = -1.0F;
                    
                    for (int y = this.Height - 1; y >= 0; y--) {
                        float density = chunkData[localX, y, localZ];
                        
                        if (density >= this.IsoLevel) {
                            if (MathF.Abs(previousDensity - density) < 1.0E-6F) {
                                surfaceHeight = y;
                            }
                            else {
                                float t = (this.IsoLevel - density) / (previousDensity - density);
                                surfaceHeight = y + t;
                            }
                            
                            break;
                        }
                        
                        previousDensity = density;
                    }
                    
                    this._surfaceHeights[worldX, worldZ] = surfaceHeight;
                }
            }
            
            HeightmapChunk chunk = new HeightmapChunk(this, new Vector3(startX, 0.0F, startZ), chunkX, chunkZ, chunkWidth, this.Height, chunkDepth);
            this._chunkGrid[chunkX, chunkZ] = chunk;
            chunks[index] = chunk;
        });
        
        this._chunks.AddRange(chunks);
    }
}
