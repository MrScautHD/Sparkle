using System.Numerics;

namespace Sparkle.CSharp.Terrain.Marching;

public class MarchingCubesTerrain : ITerrain {

    /// <summary>The generator used to produce density data for each terrain column.</summary>
    public IChunkGenerator ChunkGenerator { get; private set; }

    /// <summary>The total width of the terrain in voxels.</summary>
    public int Width { get; private set; }

    /// <summary>The total height of the terrain in voxels.</summary>
    public int Height { get; private set; }

    /// <summary>The total depth of the terrain in voxels.</summary>
    public int Depth { get; private set; }

    /// <summary>The size of each chunk in voxels along every axis.</summary>
    public int ChunkSize { get; private set; }

    /// <summary>The density threshold at which the marching cubes algorithm places a surface.</summary>
    public float IsoLevel { get; private set; }

    /// <summary>
    /// Fired for each chunk index whenever that chunk is marked dirty.
    /// Allows subscribers (e.g. Terrain3D) to avoid an O(n) scan every frame.
    /// May be fired from background threads — subscribers must be thread-safe.
    /// </summary>
    public Action<int>? OnChunkMarkedDirty;

    /// <summary>
    /// Fired when the recently-edited cooldown expires and static chunk meshes can be re-batched.
    /// Always fired from the thread that called <see cref="Update"/>.
    /// </summary>
    public Action? OnBatchInvalidated;

    /// <summary>
    /// Chunk indices edited within the last <see cref="EditCooldownDuration"/> seconds.
    /// Cleared automatically by <see cref="Update"/>.
    /// </summary>
    public IReadOnlySet<int> RecentlyEditedChunkIndices => this._recentlyEditedIndices;

    // ── Density storage ────────────────────────────────────────────────────────────────────
    //
    // Densities are stored as short (int16) rather than float (32-bit) to halve memory usage.
    //
    //   float[1025, 129, 1025] = 1025 × 129 × 1025 × 4 bytes ≈ 540 MB
    //   short[1025, 129, 1025] = 1025 × 129 × 1025 × 2 bytes ≈ 270 MB  ← 270 MB saved
    //
    // Values are scaled by DensityScale on write and divided on read, giving a precision of
    // 1/128 ≈ 0.008 — more than sufficient for smooth marching cubes terrain.
    // Densities outside ±256 (e.g. deep solid or high air) are clamped at the short limits;
    // this has no visible effect because the iso-surface only exists near density = 0.
    //
    // To save even more memory, reduce TerrainHeight in TerrainScene (e.g. 128 → 64):
    //   short[1025, 65, 1025] ≈ 135 MB

    private const float DensityScale    = 128.0f;
    private const float DensityScaleInv = 1.0f / DensityScale;

    /// <summary>
    /// The flat density volume covering the full terrain extent, stored as scaled int16 values.
    /// Use <see cref="EncodeDensity"/> / <see cref="DecodeDensity"/> for all access.
    /// </summary>
    private short[,,] _densities;

    private List<IChunk> _chunks;
    private readonly HashSet<int> _recentlyEditedIndices;
    private double _editCooldown;
    private const double EditCooldownDuration = 1.5;

    // ══════════════════════════════════════════════════════════════════════════════════════════
    // Construction
    // ══════════════════════════════════════════════════════════════════════════════════════════

    private MarchingCubesTerrain(IChunkGenerator chunkGenerator, int width, int height, int depth, int chunkSize, float isoLevel) {
        this.ChunkGenerator          = chunkGenerator;
        this.Width                   = width;
        this.Height                  = height;
        this.Depth                   = depth;
        this.ChunkSize               = chunkSize;
        this.IsoLevel                = isoLevel;
        this._densities              = new short[width + 1, height + 1, depth + 1];
        this._chunks                 = new List<IChunk>();
        this._recentlyEditedIndices  = new HashSet<int>();
    }

    public static async Task<MarchingCubesTerrain> CreateAsync(IChunkGenerator chunkGenerator, int width, int height, int depth, int chunkSize = 16, float isoLevel = 0.0F) {
        MarchingCubesTerrain terrain = new MarchingCubesTerrain(chunkGenerator, width, height, depth, chunkSize, isoLevel);
        await terrain.CreateChunks();
        return terrain;
    }

    // ══════════════════════════════════════════════════════════════════════════════════════════
    // Density encoding helpers
    // ══════════════════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Encodes a float density into a short. Values outside ±256 are clamped.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static short EncodeDensity(float value) {
        return (short) Math.Clamp((int) (value * DensityScale), short.MinValue, short.MaxValue);
    }

    /// <summary>
    /// Decodes a stored short back into a float density.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static float DecodeDensity(short value) {
        return value * DensityScaleInv;
    }

    /// <summary>
    /// Adds <paramref name="delta"/> to the density at <c>[x, y, z]</c>, clamping at short limits.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private void AddDensity(int x, int y, int z, float delta) {
        int current = this._densities[x, y, z];
        int updated = current + (int) (delta * DensityScale);
        this._densities[x, y, z] = (short) Math.Clamp(updated, short.MinValue, short.MaxValue);
    }

    // ══════════════════════════════════════════════════════════════════════════════════════════
    // ITerrain implementation
    // ══════════════════════════════════════════════════════════════════════════════════════════

    public IReadOnlyList<IChunk> GetChunks() => this._chunks;

    public IEnumerable<IChunk> GetDirtyChunks() => this._chunks.Where(c => c.IsDirty);

    /// <summary>
    /// Advances the recently-edited cooldown. Call once per game update tick.
    /// When the cooldown expires the recently-edited set is cleared and <see cref="OnBatchInvalidated"/> is fired.
    /// </summary>
    public void Update(double delta) {
        if (this._recentlyEditedIndices.Count == 0) {
            return;
        }

        this._editCooldown -= delta;

        if (this._editCooldown <= 0.0) {
            this._recentlyEditedIndices.Clear();
            this.OnBatchInvalidated?.Invoke();
        }
    }

    /// <summary>
    /// Returns the trilinearly interpolated density at <paramref name="position"/>, or -1 when out of bounds.
    /// </summary>
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

        float wx = position.X - xMin;
        float wy = position.Y - yMin;
        float wz = position.Z - zMin;

        // Decode the 8 corners to float, then interpolate.
        float d000 = DecodeDensity(this._densities[xMin, yMin, zMin]);
        float d100 = DecodeDensity(this._densities[xMax, yMin, zMin]);
        float d010 = DecodeDensity(this._densities[xMin, yMax, zMin]);
        float d110 = DecodeDensity(this._densities[xMax, yMax, zMin]);
        float d001 = DecodeDensity(this._densities[xMin, yMin, zMax]);
        float d101 = DecodeDensity(this._densities[xMax, yMin, zMax]);
        float d011 = DecodeDensity(this._densities[xMin, yMax, zMax]);
        float d111 = DecodeDensity(this._densities[xMax, yMax, zMax]);

        float xBack  = float.Lerp(d000, d100, wx);
        float xTop   = float.Lerp(d010, d110, wx);
        float xFront = float.Lerp(d001, d101, wx);
        float xTF    = float.Lerp(d011, d111, wx);

        float yBack  = float.Lerp(xBack,  xTop, wy);
        float yFront = float.Lerp(xFront, xTF,  wy);

        return float.Lerp(yBack, yFront, wz);
    }

    /// <summary>
    /// Returns the decoded density at the integer voxel coordinate, or -1 when out of bounds.
    /// </summary>
    public float GetRawDensityAt(int x, int y, int z) {
        if (x < 0 || x > this.Width || y < 0 || y > this.Height || z < 0 || z > this.Depth) {
            return -1.0F;
        }

        return DecodeDensity(this._densities[x, y, z]);
    }

    /// <summary>
    /// Sets every voxel to <paramref name="density"/> and marks all chunks dirty.
    /// </summary>
    public void Fill(float density) {
        short encoded = EncodeDensity(density);

        for (int x = 0; x <= this.Width; x++) {
            for (int y = 0; y <= this.Height; y++) {
                for (int z = 0; z <= this.Depth; z++) {
                    this._densities[x, y, z] = encoded;
                }
            }
        }

        this.MarkAllChunksDirty();
    }

    /// <summary>
    /// Fills the terrain with a flat horizontal surface at <paramref name="surfaceHeight"/> and marks all chunks dirty.
    /// </summary>
    public void ApplyFlatSurface(float surfaceHeight) {
        for (int y = 0; y <= this.Height; y++) {
            // Encode once per Y layer — same value for all X/Z.
            short encoded = EncodeDensity(surfaceHeight - y);

            for (int x = 0; x <= this.Width; x++) {
                for (int z = 0; z <= this.Depth; z++) {
                    this._densities[x, y, z] = encoded;
                }
            }
        }

        this.MarkAllChunksDirty();
    }

    /// <summary>
    /// Adds a spherical density falloff centered at <paramref name="center"/> to all voxels within
    /// <paramref name="radius"/>, marks affected chunks dirty, and records the region as recently edited.
    /// Returns <c>true</c> if any voxel was modified.
    /// </summary>
    public bool ApplyBrush(Vector3 center, float radius, float strength) {
        if (radius <= 0.0F || strength == 0.0F) {
            return false;
        }

        int minX = Math.Max(0, (int) MathF.Floor(center.X - radius));
        int minY = Math.Max(0, (int) MathF.Floor(center.Y - radius));
        int minZ = Math.Max(0, (int) MathF.Floor(center.Z - radius));
        int maxX = Math.Min(this.Width,  (int) MathF.Ceiling(center.X + radius));
        int maxY = Math.Min(this.Height, (int) MathF.Ceiling(center.Y + radius));
        int maxZ = Math.Min(this.Depth,  (int) MathF.Ceiling(center.Z + radius));

        float radiusSquared = radius * radius;
        float invRadius     = 1.0F / radius;
        bool  changed       = false;

        // Serial path only — density writes on short[,,] are not atomic and brush radii are
        // small enough (typically < 4000 voxels) that serial is faster than thread overhead.
        for (int x = minX; x <= maxX; x++) {
            float dx   = x - center.X;
            float dxSq = dx * dx;

            for (int y = minY; y <= maxY; y++) {
                float dy   = y - center.Y;
                float dySq = dy * dy;

                if (dxSq + dySq > radiusSquared) {
                    continue;
                }

                for (int z = minZ; z <= maxZ; z++) {
                    float dz              = z - center.Z;
                    float distanceSquared = dxSq + dySq + dz * dz;

                    if (distanceSquared > radiusSquared) {
                        continue;
                    }

                    float falloff = 1.0F - MathF.Sqrt(distanceSquared) * invRadius;
                    this.AddDensity(x, y, z, falloff * strength);
                    changed = true;
                }
            }
        }

        if (changed) {
            this.MarkDirtyChunks(center, radius);
            this.TrackEditedChunks(center, radius);
        }

        return changed;
    }

    /// <summary>Resets the entire terrain to <paramref name="density"/> and marks all chunks dirty.</summary>
    public void Clear(float density = -1.0F) {
        this.Fill(density);
    }

    /// <summary>Computes the surface normal at <paramref name="position"/> by sampling the density gradient.</summary>
    public Vector3 CalculateNormal(Vector3 position) {
        const float threshold = 0.25F;

        float dx = this.GetDensityAt(position + Vector3.UnitX * threshold) - this.GetDensityAt(position - Vector3.UnitX * threshold);
        float dy = this.GetDensityAt(position + Vector3.UnitY * threshold) - this.GetDensityAt(position - Vector3.UnitY * threshold);
        float dz = this.GetDensityAt(position + Vector3.UnitZ * threshold) - this.GetDensityAt(position - Vector3.UnitZ * threshold);

        Vector3 gradient = new Vector3(dx, dy, dz);

        return gradient.LengthSquared() <= float.Epsilon ? Vector3.UnitY : -Vector3.Normalize(gradient);
    }

    /// <summary>Casts a ray and returns the first point where it crosses the iso-surface.</summary>
    public bool RaycastSurface(Vector3 origin, Vector3 direction, float maxDistance, float stepSize, out Vector3 hitPosition, out Vector3 hitNormal) {
        hitPosition = default;
        hitNormal   = Vector3.UnitY;

        if (direction.LengthSquared() < float.Epsilon || maxDistance <= 0 || stepSize <= 0) {
            return false;
        }

        Vector3 rayDir          = Vector3.Normalize(direction);
        Vector3 previousPos     = origin;
        float   previousDensity = this.GetDensityAt(previousPos) - this.IsoLevel;
        bool    wasSolid        = previousDensity >= 0;

        for (float distance = stepSize; distance <= maxDistance; distance += stepSize) {
            Vector3 currentPos     = origin + rayDir * distance;
            float   currentDensity = this.GetDensityAt(currentPos) - this.IsoLevel;
            bool    isSolid        = currentDensity >= 0;

            if (wasSolid != isSolid) {
                Vector3 posA     = previousPos;
                Vector3 posB     = currentPos;
                float   densityA = previousDensity;

                for (int i = 0; i < 8; i++) {
                    Vector3 midPos     = (posA + posB) * 0.5F;
                    float   midDensity = this.GetDensityAt(midPos) - this.IsoLevel;

                    if (midDensity >= 0 == densityA >= 0) {
                        posA     = midPos;
                        densityA = midDensity;
                    }
                    else {
                        posB = midPos;
                    }
                }

                hitPosition = (posA + posB) * 0.5F;
                hitNormal   = this.CalculateNormal(hitPosition);
                return true;
            }

            previousPos     = currentPos;
            previousDensity = currentDensity;
            wasSolid        = isSolid;
        }

        return false;
    }

    /// <summary>Returns <c>true</c> when <paramref name="position"/> lies within the terrain bounds.</summary>
    public bool Contains(Vector3 position) {
        return position.X >= 0 && position.X <= this.Width  &&
               position.Y >= 0 && position.Y <= this.Height &&
               position.Z >= 0 && position.Z <= this.Depth;
    }

    /// <summary>Marks every chunk dirty and fires <see cref="OnChunkMarkedDirty"/> for each.</summary>
    public void MarkAllChunksDirty() {
        for (int i = 0; i < this._chunks.Count; i++) {
            this._chunks[i].MarkDirty();
            this.OnChunkMarkedDirty?.Invoke(i);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════════════════════
    // Private helpers
    // ══════════════════════════════════════════════════════════════════════════════════════════

    private void MarkDirtyChunks(Vector3 center, float radius) {
        int chunkCountY = (this.Height + this.ChunkSize - 1) / this.ChunkSize;
        int chunkCountZ = (this.Depth  + this.ChunkSize - 1) / this.ChunkSize;

        int minChunkX = Math.Max(0, (int) MathF.Floor(center.X - radius) - 1) / this.ChunkSize;
        int minChunkY = Math.Max(0, (int) MathF.Floor(center.Y - radius) - 1) / this.ChunkSize;
        int minChunkZ = Math.Max(0, (int) MathF.Floor(center.Z - radius) - 1) / this.ChunkSize;

        int maxChunkX = Math.Min(this.Width  - 1, (int) MathF.Ceiling(center.X + radius) + 1) / this.ChunkSize;
        int maxChunkY = Math.Min(this.Height - 1, (int) MathF.Ceiling(center.Y + radius) + 1) / this.ChunkSize;
        int maxChunkZ = Math.Min(this.Depth  - 1, (int) MathF.Ceiling(center.Z + radius) + 1) / this.ChunkSize;

        for (int cx = minChunkX; cx <= maxChunkX; cx++) {
            for (int cy = minChunkY; cy <= maxChunkY; cy++) {
                for (int cz = minChunkZ; cz <= maxChunkZ; cz++) {
                    int chunkIndex = cx * chunkCountY * chunkCountZ + cy * chunkCountZ + cz;
                    this._chunks[chunkIndex].MarkDirty();
                    this.OnChunkMarkedDirty?.Invoke(chunkIndex);
                }
            }
        }
    }

    private void TrackEditedChunks(Vector3 center, float radius) {
        int chunkCountY = (this.Height + this.ChunkSize - 1) / this.ChunkSize;
        int chunkCountZ = (this.Depth  + this.ChunkSize - 1) / this.ChunkSize;

        int minChunkX = Math.Max(0, (int) MathF.Floor(center.X - radius) - 1) / this.ChunkSize;
        int minChunkY = Math.Max(0, (int) MathF.Floor(center.Y - radius) - 1) / this.ChunkSize;
        int minChunkZ = Math.Max(0, (int) MathF.Floor(center.Z - radius) - 1) / this.ChunkSize;

        int maxChunkX = Math.Min(this.Width  - 1, (int) MathF.Ceiling(center.X + radius) + 1) / this.ChunkSize;
        int maxChunkY = Math.Min(this.Height - 1, (int) MathF.Ceiling(center.Y + radius) + 1) / this.ChunkSize;
        int maxChunkZ = Math.Min(this.Depth  - 1, (int) MathF.Ceiling(center.Z + radius) + 1) / this.ChunkSize;

        for (int cx = minChunkX; cx <= maxChunkX; cx++) {
            for (int cy = minChunkY; cy <= maxChunkY; cy++) {
                for (int cz = minChunkZ; cz <= maxChunkZ; cz++) {
                    int idx = cx * chunkCountY * chunkCountZ + cy * chunkCountZ + cz;
                    this._recentlyEditedIndices.Add(idx);
                }
            }
        }

        this._editCooldown = EditCooldownDuration;
    }

    private async Task CreateChunks() {
        int chunkCountX = (this.Width  + this.ChunkSize - 1) / this.ChunkSize;
        int chunkCountY = (this.Height + this.ChunkSize - 1) / this.ChunkSize;
        int chunkCountZ = (this.Depth  + this.ChunkSize - 1) / this.ChunkSize;

        for (int cx = 0; cx < chunkCountX; cx++) {
            for (int cy = 0; cy < chunkCountY; cy++) {
                for (int cz = 0; cz < chunkCountZ; cz++) {
                    Vector3 position = new Vector3(cx * this.ChunkSize, cy * this.ChunkSize, cz * this.ChunkSize);

                    int w = Math.Min(this.ChunkSize, this.Width  - cx * this.ChunkSize);
                    int h = Math.Min(this.ChunkSize, this.Height - cy * this.ChunkSize);
                    int d = Math.Min(this.ChunkSize, this.Depth  - cz * this.ChunkSize);

                    this._chunks.Add(new MarchingCubesChunk(this, position, w, h, d));
                }
            }
        }

        List<Task> tasks = new List<Task>(chunkCountX * chunkCountZ);

        for (int cx = 0; cx < chunkCountX; cx++) {
            for (int cz = 0; cz < chunkCountZ; cz++) {
                int  capturedX = cx;
                int  capturedZ = cz;
                bool isLastX   = capturedX == chunkCountX - 1;
                bool isLastZ   = capturedZ == chunkCountZ - 1;

                tasks.Add(Task.Run(async () => {
                    float[,,] chunkData = await this.ChunkGenerator.GenerateAsync(capturedX, capturedZ);

                    int dataWidth  = chunkData.GetLength(0);
                    int dataHeight = chunkData.GetLength(1);
                    int dataDepth  = chunkData.GetLength(2);

                    int startX = capturedX * this.ChunkSize;
                    int startZ = capturedZ * this.ChunkSize;

                    for (int localX = 0; localX < dataWidth; localX++) {
                        int worldX = startX + localX;

                        if (worldX > this.Width) break;
                        if (localX == dataWidth - 1 && !isLastX) continue;

                        for (int localY = 0; localY < dataHeight; localY++) {
                            int worldY = localY;

                            if (worldY > this.Height) break;

                            for (int localZ = 0; localZ < dataDepth; localZ++) {
                                int worldZ = startZ + localZ;

                                if (worldZ > this.Depth) break;
                                if (localZ == dataDepth - 1 && !isLastZ) continue;

                                // Encode float → short on write.
                                this._densities[worldX, worldY, worldZ] = EncodeDensity(chunkData[localX, localY, localZ]);
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