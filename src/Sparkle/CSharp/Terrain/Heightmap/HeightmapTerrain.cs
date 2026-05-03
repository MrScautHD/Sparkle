using System.Numerics;
using Bliss.CSharp.Materials;

namespace Sparkle.CSharp.Terrain.Heightmap;

public class HeightmapTerrain : ITerrain {
    
    public IChunkGenerator ChunkGenerator { get; private set; }
    
    public Material Material { get; private set; }

    public int Width { get; private set; }
    
    public int Height { get; private set; }
    
    public int Depth { get; private set; }
    
    public int ChunkSize { get; private set; }
    
    public float IsoLevel { get; private set; }
    
    private float[,] _surfaceHeights;
    
    private int _chunkCountX;
    
    private int _chunkCountZ;
    
    private List<IChunk> _chunks;
    
    private HeightmapChunk[,] _chunkGrid;
    
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
    
    public static async Task<HeightmapTerrain> CreateAsync(IChunkGenerator chunkGenerator, Material material, int width, int height, int depth, int chunkSize, float isoLevel = 0.0F) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(depth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chunkSize);
        
        HeightmapTerrain terrain = new HeightmapTerrain(chunkGenerator, material, width, height, depth, chunkSize, isoLevel);
        await terrain.CreateChunks();
        return terrain;
    }
    
    public IReadOnlyList<IChunk> GetChunks() {
        return this._chunks;
    }
    
    public IEnumerable<IChunk> GetDirtyChunks() {
        foreach (IChunk chunk in this._chunks) {
            if (chunk.IsDirty) {
                yield return chunk;
            }
        }
    }
    
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
    
    public float GetRawDensityAt(int x, int y, int z) {
        if (x < 0 || x > this.Width || y < 0 || y > this.Height || z < 0 || z > this.Depth) {
            return -1.0F;
        }
        
        return this._surfaceHeights[x, z] - y;
    }
    
    public void Fill(float density) {
        float targetHeight = density >= this.IsoLevel ? this.Height : -1.0F;
        
        for (int x = 0; x <= this.Width; x++) {
            for (int z = 0; z <= this.Depth; z++) {
                this._surfaceHeights[x, z] = targetHeight;
            }
        }
        
        this.MarkAllChunksDirty();
    }
    
    public void ApplyFlatSurface(float surfaceHeight) {
        for (int x = 0; x <= this.Width; x++) {
            for (int z = 0; z <= this.Depth; z++) {
                this._surfaceHeights[x, z] = surfaceHeight;
            }
        }
        
        this.MarkAllChunksDirty();
    }
    
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
    
    public void Clear(float density = -1.0F) {
        this.Fill(density);
    }
    
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
    
    public bool Contains(Vector3 position) {
        return position.X >= 0.0F && position.X <= this.Width &&
               position.Y >= 0.0F && position.Y <= this.Height &&
               position.Z >= 0.0F && position.Z <= this.Depth;
    }
    
    public float GetSurfaceHeight(int x, int z) {
        if (x < 0 || x > this.Width || z < 0 || z > this.Depth) {
            return -1.0F;
        }
        
        return this._surfaceHeights[x, z];
    }
    
    public void MarkAllChunksDirty() {
        foreach (IChunk chunk in this._chunks) {
            chunk.MarkDirty();
        }
    }
    
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
            
            HeightmapChunk chunk = new HeightmapChunk(this, new Vector3(startX, 0.0F, startZ), chunkWidth, this.Height, chunkDepth);
            this._chunkGrid[chunkX, chunkZ] = chunk;
            chunks[index] = chunk;
        });
        
        this._chunks.AddRange(chunks);
    }
}