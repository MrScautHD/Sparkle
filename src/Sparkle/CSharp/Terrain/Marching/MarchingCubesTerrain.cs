using System.Numerics;

namespace Sparkle.CSharp.Terrain.Marching;

public class MarchingCubesTerrain : ITerrain {
    
    /// <summary>
    /// The generator used to produce density data for each terrain column.
    /// </summary>
    public IChunkGenerator ChunkGenerator { get; private set; }
    
    /// <summary>
    /// The total width of the terrain in voxels.
    /// </summary>
    public int Width { get; private set; }
    
    /// <summary>
    /// The total height of the terrain in voxels.
    /// </summary>
    public int Height { get; private set; }
    
    /// <summary>
    /// The total depth of the terrain in voxels.
    /// </summary>
    public int Depth { get; private set; }
    
    /// <summary>
    /// The size of each chunk in voxels along every axis.
    /// </summary>
    public int ChunkSize { get; private set; }
    
    /// <summary>
    /// The density threshold at which the marching cubes algorithm places a surface.
    /// </summary>
    public float IsoLevel { get; private set; }
    
    /// <summary>
    /// The flat density volume covering the full terrain extent, including shared edges.
    /// </summary>
    private float[,,] _densities;
    
    /// <summary>
    /// All chunks that subdivide the terrain volume.
    /// </summary>
    private List<IChunk> _chunks;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MarchingCubesTerrain"/> class.
    /// </summary>
    /// <param name="chunkGenerator">The generator used to produce density data.</param>
    /// <param name="width">The total width of the terrain in voxels.</param>
    /// <param name="height">The total height of the terrain in voxels.</param>
    /// <param name="depth">The total depth of the terrain in voxels.</param>
    /// <param name="chunkSize">The size of each chunk in voxels along every axis.</param>
    /// <param name="isoLevel">The density threshold at which a surface is placed.</param>
    private MarchingCubesTerrain(IChunkGenerator chunkGenerator, int width, int height, int depth, int chunkSize, float isoLevel) {
        this.ChunkGenerator = chunkGenerator;
        this.Width = width;
        this.Height = height;
        this.Depth = depth;
        this.ChunkSize = chunkSize;
        this.IsoLevel = isoLevel;
        this._densities = new float[width + 1, height + 1, depth + 1];
        this._chunks = new List<IChunk>();
    }
    
    /// <summary>
    /// Creates and initializes a new <see cref="MarchingCubesTerrain"/> by generating density data for all chunks asynchronously.
    /// </summary>
    /// <param name="chunkGenerator">The generator used to produce density data.</param>
    /// <param name="width">The total width of the terrain in voxels.</param>
    /// <param name="height">The total height of the terrain in voxels.</param>
    /// <param name="depth">The total depth of the terrain in voxels.</param>
    /// <param name="chunkSize">The size of each chunk in voxels along every axis. Defaults to 16.</param>
    /// <param name="isoLevel">The density threshold at which a surface is placed. Defaults to 0.</param>
    /// <returns>A fully initialized <see cref="MarchingCubesTerrain"/> ready for rendering.</returns>
    public static async Task<MarchingCubesTerrain> CreateAsync(IChunkGenerator chunkGenerator, int width, int height, int depth, int chunkSize = 16, float isoLevel = 0.0F) {
        MarchingCubesTerrain terrain = new MarchingCubesTerrain(chunkGenerator, width, height, depth, chunkSize, isoLevel);
        await terrain.CreateChunks();
        return terrain;
    }
    
    /// <summary>
    /// Returns all chunks that subdivide the terrain volume.
    /// </summary>
    public IReadOnlyList<IChunk> GetChunks() {
        return this._chunks;
    }
    
    /// <summary>
    /// Returns all chunks that are currently marked dirty and need a mesh rebuild.
    /// </summary>
    public IEnumerable<IChunk> GetDirtyChunks() {
        return this._chunks.Where(chunk => chunk.IsDirty);
    }
    
    /// <summary>
    /// Returns the trilinearly interpolated density at <paramref name="position"/>, or -1 when out of bounds.
    /// </summary>
    /// <param name="position">The world-space position to sample.</param>
    public float GetDensityAt(Vector3 position) {
        if (position.X < 0 || position.X >= this.Width ||
            position.Y < 0 || position.Y >= this.Height ||
            position.Z < 0 || position.Z >= this.Depth) {
            return -1.0F;
        }
        
        int xMin = (int) MathF.Floor(position.X);
        int yMin = (int) MathF.Floor(position.Y);
        int zMin = (int) MathF.Floor(position.Z);
        int xMax = xMin + 1;
        int yMax = yMin + 1;
        int zMax = zMin + 1;
        
        float weightX = position.X - xMin;
        float weightY = position.Y - yMin;
        float weightZ = position.Z - zMin;
        
        float xLerpBottomBack = float.Lerp(this._densities[xMin, yMin, zMin], this._densities[xMax, yMin, zMin], weightX);
        float xLerpTopBack = float.Lerp(this._densities[xMin, yMax, zMin], this._densities[xMax, yMax, zMin], weightX);
        float xLerpBottomFront = float.Lerp(this._densities[xMin, yMin, zMax], this._densities[xMax, yMin, zMax], weightX);
        float xLerpTopFront = float.Lerp(this._densities[xMin, yMax, zMax], this._densities[xMax, yMax, zMax], weightX);
        
        float yLerpBack = float.Lerp(xLerpBottomBack, xLerpTopBack, weightY);
        float yLerpFront = float.Lerp(xLerpBottomFront, xLerpTopFront, weightY);
        
        return float.Lerp(yLerpBack, yLerpFront, weightZ);
    }
    
    /// <summary>
    /// Returns the raw density value at the integer voxel coordinate, or -1 when out of bounds.
    /// </summary>
    /// <param name="x">The X voxel coordinate.</param>
    /// <param name="y">The Y voxel coordinate.</param>
    /// <param name="z">The Z voxel coordinate.</param>
    public float GetRawDensityAt(int x, int y, int z) {
        if (x < 0 || x > this.Width || y < 0 || y > this.Height || z < 0 || z > this.Depth) {
            return -1.0F;
        }
        
        return this._densities[x, y, z];
    }
    
    /// <summary>
    /// Sets every voxel in the terrain to <paramref name="density"/> and marks all chunks dirty.
    /// </summary>
    /// <param name="density">The density value to assign to every voxel.</param>
    public void Fill(float density) {
        for (int x = 0; x <= this.Width; x++) {
            for (int y = 0; y <= this.Height; y++) {
                for (int z = 0; z <= this.Depth; z++) {
                    this._densities[x, y, z] = density;
                }
            }
        }
        
        this.MarkAllChunksDirty();
    }
    
    /// <summary>
    /// Fills the terrain with a flat horizontal surface at <paramref name="surfaceHeight"/> and marks all chunks dirty.
    /// </summary>
    /// <param name="surfaceHeight">The Y position of the flat surface.</param>
    public void ApplyFlatSurface(float surfaceHeight) {
        for (int x = 0; x <= this.Width; x++) {
            for (int y = 0; y <= this.Height; y++) {
                float density = surfaceHeight - y;
                
                for (int z = 0; z <= this.Depth; z++) {
                    this._densities[x, y, z] = density;
                }
            }
        }
        
        this.MarkAllChunksDirty();
    }
    
    /// <summary>
    /// Adds a spherical density falloff centered at <paramref name="center"/> to all voxels within <paramref name="radius"/>,
    /// then marks affected chunks dirty. Returns <c>true</c> if any voxel was modified.
    /// </summary>
    /// <param name="center">The world-space center of the brush.</param>
    /// <param name="radius">The radius of the brush in voxels.</param>
    /// <param name="strength">The density delta applied at the brush center. Negative values remove material.</param>
    public bool ApplyBrush(Vector3 center, float radius, float strength) {
        if (radius <= 0.0F || strength == 0.0F) {
            return false;
        }
        
        int minX = Math.Max(0, (int) MathF.Floor(center.X - radius));
        int minY = Math.Max(0, (int) MathF.Floor(center.Y - radius));
        int minZ = Math.Max(0, (int) MathF.Floor(center.Z - radius));
        int maxX = Math.Min(this.Width, (int) MathF.Ceiling(center.X + radius));
        int maxY = Math.Min(this.Height, (int) MathF.Ceiling(center.Y + radius));
        int maxZ = Math.Min(this.Depth, (int) MathF.Ceiling(center.Z + radius));
        
        float radiusSquared = radius * radius;
        float invRadius = 1.0F / radius;
        
        int extentX = maxX - minX + 1;
        int extentY = maxY - minY + 1;
        int extentZ = maxZ - minZ + 1;
        int totalCells = extentX * extentY * extentZ;
        
        bool changed = false;
        
        if (totalCells >= 2000) {
            changed = Parallel.For(minX, maxX + 1, () => false, (x, _, localChanged) => {
                    float dx = x - center.X;
                    float dxSq = dx * dx;
                    
                    for (int y = minY; y <= maxY; y++) {
                        float dy = y - center.Y;
                        float dySq = dy * dy;
                        
                        if (dxSq + dySq > radiusSquared) {
                            continue;
                        }
                        
                        for (int z = minZ; z <= maxZ; z++) {
                            float dz = z - center.Z;
                            float distanceSquared = dxSq + dySq + dz * dz;
                            
                            if (distanceSquared > radiusSquared) {
                                continue;
                            }
                            
                            float falloff = 1.0F - MathF.Sqrt(distanceSquared) * invRadius;
                            this._densities[x, y, z] += falloff * strength;
                            localChanged = true;
                        }
                    }
                    
                    return localChanged;
                }, localChanged => {
                    if (localChanged) {
                        changed = true;
                    }
                }
            ).IsCompleted && changed;
        }
        else {
            for (int x = minX; x <= maxX; x++) {
                float dx = x - center.X;
                float dxSq = dx * dx;
                
                for (int y = minY; y <= maxY; y++) {
                    float dy = y - center.Y;
                    float dySq = dy * dy;
                    
                    if (dxSq + dySq > radiusSquared) {
                        continue;
                    }
                    
                    for (int z = minZ; z <= maxZ; z++) {
                        float dz = z - center.Z;
                        float distanceSquared = dxSq + dySq + dz * dz;
                        
                        if (distanceSquared > radiusSquared) {
                            continue;
                        }
                        
                        float falloff = 1.0F - MathF.Sqrt(distanceSquared) * invRadius;
                        this._densities[x, y, z] += falloff * strength;
                        changed = true;
                    }
                }
            }
        }
        
        if (changed) {
            this.MarkDirtyChunks(center, radius);
        }
        
        return changed;
    }
    
    /// <summary>
    /// Resets the entire terrain to <paramref name="density"/> and marks all chunks dirty.
    /// </summary>
    /// <param name="density">The density value to fill with. Defaults to -1.</param>
    public void Clear(float density = -1.0F) {
        this.Fill(density);
    }
    
    /// <summary>
    /// Computes the surface normal at <paramref name="position"/> by sampling the density gradient.
    /// </summary>
    /// <param name="position">The world-space position to evaluate the normal at.</param>
    /// <returns>The normalized surface normal, or <see cref="Vector3.UnitY"/> when the gradient is zero.</returns>
    public Vector3 CalculateNormal(Vector3 position) {
        const float threshold = 0.25F;
        
        float dx = this.GetDensityAt(position + Vector3.UnitX * threshold) - this.GetDensityAt(position - Vector3.UnitX * threshold);
        float dy = this.GetDensityAt(position + Vector3.UnitY * threshold) - this.GetDensityAt(position - Vector3.UnitY * threshold);
        float dz = this.GetDensityAt(position + Vector3.UnitZ * threshold) - this.GetDensityAt(position - Vector3.UnitZ * threshold);
        
        Vector3 gradient = new Vector3(dx, dy, dz);
        
        if (gradient.LengthSquared() <= float.Epsilon) {
            return Vector3.UnitY;
        }
        
        return -Vector3.Normalize(gradient);
    }
    
    /// <summary>
    /// Casts a ray through the terrain and returns the first point where it crosses the iso-surface.
    /// </summary>
    /// <param name="origin">The world-space ray origin.</param>
    /// <param name="direction">The ray direction (does not need to be normalized).</param>
    /// <param name="maxDistance">The maximum distance to march along the ray.</param>
    /// <param name="stepSize">The distance between each density sample along the ray.</param>
    /// <param name="hitPosition">The interpolated world-space surface hit position.</param>
    /// <param name="hitNormal">The surface normal at the hit position.</param>
    /// <returns><c>true</c> if the ray hit the surface; otherwise <c>false</c>.</returns>
    public bool RaycastSurface(Vector3 origin, Vector3 direction, float maxDistance, float stepSize, out Vector3 hitPosition, out Vector3 hitNormal) {
        hitPosition = default;
        hitNormal = Vector3.UnitY;
        
        if (direction.LengthSquared() < float.Epsilon || maxDistance <= 0 || stepSize <= 0) {
            return false;
        }
        
        Vector3 rayDir = Vector3.Normalize(direction);
        Vector3 previousPos = origin;
        
        float previousDensity = this.GetDensityAt(previousPos) - this.IsoLevel;
        bool wasSolid = previousDensity >= 0;
        
        for (float distance = stepSize; distance <= maxDistance; distance += stepSize) {
            Vector3 currentPos = origin + rayDir * distance;
            float currentDensity = this.GetDensityAt(currentPos) - this.IsoLevel;
            bool isSolid = currentDensity >= 0;
            
            if (wasSolid != isSolid) {
                Vector3 posA = previousPos;
                Vector3 posB = currentPos;
                float densityA = previousDensity;
                
                for (int i = 0; i < 8; i++) {
                    Vector3 midPos = (posA + posB) * 0.5F;
                    float midDensity = this.GetDensityAt(midPos) - this.IsoLevel;
                    
                    if (midDensity >= 0 == densityA >= 0) {
                        posA = midPos;
                        densityA = midDensity;
                    }
                    else {
                        posB = midPos;
                    }
                }
                
                hitPosition = (posA + posB) * 0.5F;
                hitNormal = this.CalculateNormal(hitPosition);
                return true;
            }
            
            previousPos = currentPos;
            previousDensity = currentDensity;
            wasSolid = isSolid;
        }
        
        return false;
    }
    
    /// <summary>
    /// Returns <c>true</c> when <paramref name="position"/> lies within the terrain bounds.
    /// </summary>
    /// <param name="position">The world-space position to test.</param>
    public bool Contains(Vector3 position) {
        return position.X >= 0.0F && position.X <= this.Width &&
               position.Y >= 0.0F && position.Y <= this.Height &&
               position.Z >= 0.0F && position.Z <= this.Depth;
    }
    
    /// <summary>
    /// Marks every chunk in the terrain as dirty.
    /// </summary>
    public void MarkAllChunksDirty() {
        foreach (IChunk chunk in this._chunks) {
            chunk.MarkDirty();
        }
    }
    
    /// <summary>
    /// Marks all chunks whose bounds overlap the sphere defined by <paramref name="center"/> and <paramref name="radius"/> as dirty.
    /// </summary>
    /// <param name="center">The world-space center of the modified region.</param>
    /// <param name="radius">The radius of the modified region in voxels.</param>
    private void MarkDirtyChunks(Vector3 center, float radius) {
        int minChunkX = Math.Max(0, (int) MathF.Floor(center.X - radius) - 1) / this.ChunkSize;
        int minChunkY = Math.Max(0, (int) MathF.Floor(center.Y - radius) - 1) / this.ChunkSize;
        int minChunkZ = Math.Max(0, (int) MathF.Floor(center.Z - radius) - 1) / this.ChunkSize;
        
        int maxChunkX = Math.Min(this.Width - 1, (int) MathF.Ceiling(center.X + radius) + 1) / this.ChunkSize;
        int maxChunkY = Math.Min(this.Height - 1, (int) MathF.Ceiling(center.Y + radius) + 1) / this.ChunkSize;
        int maxChunkZ = Math.Min(this.Depth - 1, (int) MathF.Ceiling(center.Z + radius) + 1) / this.ChunkSize;
        
        int chunkCountY = (this.Height + this.ChunkSize - 1) / this.ChunkSize;
        int chunkCountZ = (this.Depth + this.ChunkSize - 1) / this.ChunkSize;
        
        for (int chunkX = minChunkX; chunkX <= maxChunkX; chunkX++) {
            for (int chunkY = minChunkY; chunkY <= maxChunkY; chunkY++) {
                for (int chunkZ = minChunkZ; chunkZ <= maxChunkZ; chunkZ++) {
                    int chunkIndex = chunkX * chunkCountY * chunkCountZ + chunkY * chunkCountZ + chunkZ;
                    this._chunks[chunkIndex].MarkDirty();
                }
            }
        }
    }
    
    /// <summary>
    /// Creates all chunk objects and populates the density volume by running <see cref="IChunkGenerator.GenerateAsync"/> for each column in parallel.
    /// </summary>
    private async Task CreateChunks() {
        int chunkCountX = (this.Width + this.ChunkSize - 1) / this.ChunkSize;
        int chunkCountY = (this.Height + this.ChunkSize - 1) / this.ChunkSize;
        int chunkCountZ = (this.Depth + this.ChunkSize - 1) / this.ChunkSize;
        
        // Create chunk objects.
        for (int chunkX = 0; chunkX < chunkCountX; chunkX++) {
            for (int chunkY = 0; chunkY < chunkCountY; chunkY++) {
                for (int chunkZ = 0; chunkZ < chunkCountZ; chunkZ++) {
                    Vector3 position = new Vector3(chunkX * this.ChunkSize, chunkY * this.ChunkSize, chunkZ * this.ChunkSize);
                    
                    int width = Math.Min(this.ChunkSize, this.Width - chunkX * this.ChunkSize);
                    int height = Math.Min(this.ChunkSize, this.Height - chunkY * this.ChunkSize);
                    int depth = Math.Min(this.ChunkSize, this.Depth - chunkZ * this.ChunkSize);
                    
                    this._chunks.Add(new MarchingCubesChunk(this, position, width, height, depth));
                }
            }
        }
        
        List<Task> tasks = new List<Task>(chunkCountX * chunkCountZ);
        
        for (int chunkX = 0; chunkX < chunkCountX; chunkX++) {
            for (int chunkZ = 0; chunkZ < chunkCountZ; chunkZ++) {
                int cx = chunkX;
                int cz = chunkZ;
                
                bool isLastX = cx == chunkCountX - 1;
                bool isLastZ = cz == chunkCountZ - 1;
                
                tasks.Add(Task.Run(async () => {
                    float[,,] chunkData = await this.ChunkGenerator.GenerateAsync(cx, cz);
                    
                    int dataWidth  = chunkData.GetLength(0);
                    int dataHeight = chunkData.GetLength(1);
                    int dataDepth  = chunkData.GetLength(2);
                    
                    int startX = cx * this.ChunkSize;
                    int startZ = cz * this.ChunkSize;
                    
                    for (int localX = 0; localX < dataWidth; localX++) {
                        int worldX = startX + localX;
                        
                        if (worldX > this.Width) {
                            break;
                        }
                        
                        if (localX == dataWidth - 1 && !isLastX) {
                            continue;
                        }
                        
                        for (int localY = 0; localY < dataHeight; localY++) {
                            int worldY = localY;
                            
                            if (worldY > this.Height) {
                                break;
                            }
                            
                            for (int localZ = 0; localZ < dataDepth; localZ++) {
                                int worldZ = startZ + localZ;
                                
                                if (worldZ > this.Depth) {
                                    break;
                                }
                                
                                // Only the last chunk in Z writes the shared far edge.
                                if (localZ == dataDepth - 1 && !isLastZ) {
                                    continue;
                                }
                                
                                this._densities[worldX, worldY, worldZ] = chunkData[localX, localY, localZ];
                            }
                        }
                    }
                }));
            }
        }
        
        await Task.WhenAll(tasks);
        this.MarkAllChunksDirty();
    }
}