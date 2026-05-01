using System.Numerics;

namespace Sparkle.CSharp.Terrain.Heightmap;

public class HeightmapTerrain : ITerrain {
    
    public IChunkGenerator ChunkGenerator { get; }
    
    public int Width { get; }
    
    public int Height { get; }
    
    public int Depth { get; }
    
    public int ChunkSize { get; }
    
    public float IsoLevel { get; }
    
    private readonly float[,] _surfaceHeights;
    private readonly List<IChunk> _chunks;
    private readonly HashSet<IChunk> _dirtyChunks;
    private readonly object _dirtyChunksLock;
    private readonly HeightmapChunk[,] _chunkGrid;
    private readonly int _chunkCountX;
    private readonly int _chunkCountZ;
    
    private HeightmapTerrain(IChunkGenerator chunkGenerator, int width, int height, int depth, int chunkSize, float isoLevel) {
        this.ChunkGenerator = chunkGenerator;
        this.Width = width;
        this.Height = height;
        this.Depth = depth;
        this.ChunkSize = chunkSize;
        this.IsoLevel = isoLevel;
        this._surfaceHeights = new float[width + 1, depth + 1];
        
        this._chunkCountX = Math.Max(1, (int) Math.Ceiling(width / (float) chunkSize));
        this._chunkCountZ = Math.Max(1, (int) Math.Ceiling(depth / (float) chunkSize));
        
        this._chunks = new List<IChunk>(this._chunkCountX * this._chunkCountZ);
        this._dirtyChunks = new HashSet<IChunk>();
        this._dirtyChunksLock = new object();
        this._chunkGrid = new HeightmapChunk[this._chunkCountX, this._chunkCountZ];
    }
    
    public static async Task<HeightmapTerrain> CreateAsync(IChunkGenerator chunkGenerator, int width, int height, int depth, int chunkSize, float isoLevel = 0.0F) {
        if (width <= 0) {
            throw new ArgumentOutOfRangeException(nameof(width));
        }
        
        if (height <= 0) {
            throw new ArgumentOutOfRangeException(nameof(height));
        }
        
        if (depth <= 0) {
            throw new ArgumentOutOfRangeException(nameof(depth));
        }
        
        if (chunkSize <= 0) {
            throw new ArgumentOutOfRangeException(nameof(chunkSize));
        }
        
        HeightmapTerrain terrain = new HeightmapTerrain(chunkGenerator, width, height, depth, chunkSize, isoLevel);
        await terrain.CreateChunks();
        return terrain;
    }
    
    public IReadOnlyList<IChunk> GetChunks() {
        return this._chunks;
    }
    
    public IEnumerable<IChunk> GetDirtyChunks() {
        lock (this._dirtyChunksLock) {
            return this._dirtyChunks.ToArray();
        }
    }
    
    public float GetDensityAt(Vector3 position) {
        if (!this.Contains(position)) {
            return -1.0F;
        }
        
        int x0 = (int) MathF.Floor(position.X);
        int z0 = (int) MathF.Floor(position.Z);
        
        int x1 = Math.Min(x0 + 1, this.Width);
        int z1 = Math.Min(z0 + 1, this.Depth);
        
        float tx = position.X - x0;
        float tz = position.Z - z0;
        
        float h00 = this._surfaceHeights[x0, z0];
        float h10 = this._surfaceHeights[x1, z0];
        float h01 = this._surfaceHeights[x0, z1];
        float h11 = this._surfaceHeights[x1, z1];
        float h0 = Lerp(h00, h10, tx);
        float h1 = Lerp(h01, h11, tx);
        float h = Lerp(h0, h1, tz);
        
        return h - position.Y;
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
        
        for (int x = minX; x <= maxX; x++) {
            for (int z = minZ; z <= maxZ; z++) {
                float currentHeight = this._surfaceHeights[x, z];
                Vector3 delta = new Vector3(x, currentHeight, z) - center;
                float distanceSquared = delta.LengthSquared();
                
                if (distanceSquared > radiusSquared) {
                    continue;
                }
                
                float falloff = 1.0F - (MathF.Sqrt(distanceSquared) / radius);
                this._surfaceHeights[x, z] += strength * falloff;
                changed = true;
            }
        }
        
        if (!changed) {
            return false;
        }
        
        int chunkMinX = Math.Clamp(minX / this.ChunkSize, 0, this._chunkCountX - 1);
        int chunkMaxX = Math.Clamp(maxX / this.ChunkSize, 0, this._chunkCountX - 1);
        int chunkMinZ = Math.Clamp(minZ / this.ChunkSize, 0, this._chunkCountZ - 1);
        int chunkMaxZ = Math.Clamp(maxZ / this.ChunkSize, 0, this._chunkCountZ - 1);
        
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
        int x = Math.Clamp((int) MathF.Round(position.X), 0, this.Width);
        int z = Math.Clamp((int) MathF.Round(position.Z), 0, this.Depth);
        
        int x0 = Math.Max(0, x - 1);
        int x1 = Math.Min(this.Width, x + 1);
        int z0 = Math.Max(0, z - 1);
        int z1 = Math.Min(this.Depth, z + 1);
        
        float dHx = this._surfaceHeights[x1, z] - this._surfaceHeights[x0, z];
        float dHz = this._surfaceHeights[x, z1] - this._surfaceHeights[x, z0];
        
        Vector3 normal = new Vector3(-dHx, 2.0F, -dHz);
        
        if (normal.LengthSquared() <= 1.0E-10F) {
            return Vector3.UnitY;
        }
        
        return Vector3.Normalize(normal);
    }
    
    public bool RaycastSurface(Vector3 origin, Vector3 direction, float maxDistance, float stepSize, out Vector3 hitPosition, out Vector3 hitNormal) {
        hitPosition = Vector3.Zero;
        hitNormal = Vector3.UnitY;
        
        if (maxDistance <= 0.0F || stepSize <= 0.0F || direction.LengthSquared() <= 1.0E-10F) {
            return false;
        }
        
        Vector3 dir = Vector3.Normalize(direction);
        float distance = 0.0F;
        Vector3 previousPoint = origin;
        float previousDensity = this.GetDensityAt(previousPoint) - this.IsoLevel;
        
        while (distance <= maxDistance) {
            distance += stepSize;
            Vector3 currentPoint = origin + dir * distance;
            float currentDensity = this.GetDensityAt(currentPoint) - this.IsoLevel;
            
            if (previousDensity * currentDensity <= 0.0F) {
                Vector3 a = previousPoint;
                Vector3 b = currentPoint;
                
                for (int i = 0; i < 8; i++) {
                    Vector3 mid = (a + b) * 0.5F;
                    float midDensity = this.GetDensityAt(mid) - this.IsoLevel;
                    
                    if (previousDensity * midDensity <= 0.0F) {
                        b = mid;
                        currentDensity = midDensity;
                    }
                    else {
                        a = mid;
                        previousDensity = midDensity;
                    }
                }
                
                hitPosition = (a + b) * 0.5F;
                hitNormal = this.CalculateNormal(hitPosition);
                return true;
            }
            
            previousPoint = currentPoint;
            previousDensity = currentDensity;
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

    internal void NotifyChunkDirty(IChunk chunk) {
        lock (this._dirtyChunksLock) {
            this._dirtyChunks.Add(chunk);
        }
    }

    internal void NotifyChunkClean(IChunk chunk) {
        lock (this._dirtyChunksLock) {
            this._dirtyChunks.Remove(chunk);
        }
    }
    
    private async Task CreateChunks() {
        int totalChunkCount = this._chunkCountX * this._chunkCountZ;
        IChunk[] chunkArray = new IChunk[totalChunkCount];
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
                    this._surfaceHeights[worldX, worldZ] = FindSurfaceHeight(chunkData, localX, localZ, this.Height, this.IsoLevel);
                }
            }
            
            HeightmapChunk chunk = new HeightmapChunk(this, new Vector3(startX, 0.0F, startZ), chunkWidth, this.Height, chunkDepth);
            this._chunkGrid[chunkX, chunkZ] = chunk;
            chunkArray[index] = chunk;
        });
        
        this._chunks.AddRange(chunkArray);
    }
    
    private static float Lerp(float a, float b, float t) {
        return a + ((b - a) * t);
    }
    
    private static float FindSurfaceHeight(float[,,] chunkData, int localX, int localZ, int maxY, float isoLevel) {
        float previousDensity = chunkData[localX, maxY, localZ];
        
        for (int y = maxY - 1; y >= 0; y--) {
            float density = chunkData[localX, y, localZ];
            
            if (density >= isoLevel) {
                if (MathF.Abs(previousDensity - density) < 1.0E-6F) {
                    return y;
                }
                
                float t = (isoLevel - density) / (previousDensity - density);
                return y + t;
            }
            
            previousDensity = density;
        }
        
        return -1.0F;
    }
}
