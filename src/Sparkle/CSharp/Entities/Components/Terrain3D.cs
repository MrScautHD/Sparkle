using System.Collections.Concurrent;
using System.Numerics;
using Bliss.CSharp;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Geometry;
using Bliss.CSharp.Geometry.Meshes;
using Bliss.CSharp.Geometry.Meshes.Data;
using Bliss.CSharp.Graphics.Rendering.Renderers;
using Bliss.CSharp.Graphics.Rendering.Renderers.Forward;
using Bliss.CSharp.Graphics.VertexTypes;
using Bliss.CSharp.Materials;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.Graphics.Rendering.Gizmos;
using Sparkle.CSharp.Scenes;
using Sparkle.CSharp.Terrain;
using Sparkle.CSharp.Terrain.Marching;
using Veldrid;
using Color = Bliss.CSharp.Colors.Color;
using Material = Bliss.CSharp.Materials.Material;

namespace Sparkle.CSharp.Entities.Components;

/// <summary>
/// A component that renders a chunked, LOD-aware 3D terrain on the entity it is attached to.
///
/// Rendering: chunks are grouped into <see cref="RegionChunkCount"/>×<see cref="RegionChunkCount"/>
/// regions. Each region's geometry is merged into one <see cref="Mesh{T}"/> — one draw call per
/// region instead of one per chunk.
///
/// Editing performance: when a region needs rebuilding, the CPU vertex merge runs on a background
/// thread. The main thread only takes a lightweight cache snapshot and later performs the GPU buffer
/// upload, keeping frame time smooth during active brush strokes.
/// </summary>
public class Terrain3D : InterpolatedComponent, IDebugDrawable {

    /// <summary>The terrain data source that provides chunks and dimension information.</summary>
    public ITerrain Terrain { get; private set; }

    /// <summary>
    /// Maximum number of chunks processed from the upload queue per frame.
    /// Each processed chunk may schedule an async region rebuild.
    /// Raise while editing to surface rebuilt meshes faster.
    /// </summary>
    public int MaxChunkUploadsPerFrame;

    /// <summary>When <c>true</c>, regions outside the active camera frustum are skipped.</summary>
    public bool FrustumCulling;

    /// <summary>Color used for region bounding boxes when <see cref="DebugDrawEnabled"/> is <c>true</c>.</summary>
    public Color RegionBoxColor;

    /// <summary>Color used for individual chunk bounding boxes when <see cref="DebugDrawEnabled"/> is <c>true</c>.</summary>
    public Color ChunkBoxColor;

    /// <inheritdoc/>
    public bool DebugDrawEnabled { get; set; }

    /// <summary>Number of regions submitted for rendering during the most recent frame.</summary>
    public int VisibleChunkCount { get; private set; }

    /// <summary>Total vertices rendered during the most recent frame.</summary>
    public int VisibleVertexCount { get; private set; }

    /// <summary>Camera distances at which the terrain steps to the next LOD level.</summary>
    public float[] LodDistances {
        get => this._lodDistances;
        set {
            this._lodDistances = value;
            this.RebuildLodDistancesSq();
        }
    }

    /// <summary>Total vertex count across all region batches that have an uploaded mesh.</summary>
    public int TotalVertexCount {
        get {
            int count = 0;
            foreach (RegionBatch batch in this._regionBatches) {
                count += (int) (batch.Mesh?.VertexCount ?? 0);
            }
            return count;
        }
    }

    // ══════════════════════════════════════════════════════════════════════════════════════════
    // Region batching
    // ══════════════════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Chunks per region along X and Z.
    /// Smaller → more draw calls but faster region rebuilds when editing.
    /// Larger  → fewer draw calls but slower rebuilds.
    /// 4 gives 16 chunks per region and ~64 total regions for a 1024×1024 terrain at ChunkSize=32.
    /// </summary>
    private const int RegionChunkCount = 4;

    private RegionBatch[] _regionBatches;
    private int[]         _chunkToRegion;
    private readonly HashSet<int> _dirtyRegions;

    // Per-chunk CPU vertex/index cache — populated when a chunk finishes generating geometry.
    // Null entries = chunk is empty (uniform or LOD-culled).
    private Vertex3D[]?[] _chunkVertexCache;
    private uint[]?[]     _chunkIndexCache;

    // ── Region build state ─────────────────────────────────────────────────────────────────

    /// <summary>Region build-state: idle and ready to be scheduled.</summary>
    private const int RegStatIdle     = 0;

    /// <summary>Region build-state: CPU merge running on a background thread.</summary>
    private const int RegStatBuilding = 1;

    /// <summary>Per-region atomic build state (parallel to <see cref="_regionBatches"/>).</summary>
    private int[] _regionBuildState;

    /// <summary>
    /// Per-region flag set on the main thread when a rebuild is requested while the region is
    /// already building. Checked on upload to immediately schedule another rebuild.
    /// Only accessed from the main thread — no atomics needed.
    /// </summary>
    private bool[] _regionNeedsRebuildAfterCurrent;

    /// <summary>
    /// Completed CPU merges waiting for a main-thread GPU upload.
    /// Written by background tasks, drained by the main thread each frame.
    /// </summary>
    private readonly ConcurrentQueue<(int RegionIdx, Vertex3D[] Vertices, uint[] Indices)> _regionUploadQueue;

    // ══════════════════════════════════════════════════════════════════════════════════════════
    // Chunk build state
    // ══════════════════════════════════════════════════════════════════════════════════════════

    private const int StatIdle       = 0;
    private const int StatGenerating = 1;
    private const int StatReady      = 2;

    // ══════════════════════════════════════════════════════════════════════════════════════════
    // Per-chunk arrays
    // ══════════════════════════════════════════════════════════════════════════════════════════

    private float[] _lodDistances;
    private float[] _lodDistancesSq;
    private int[]   _lodSteps;

    private IChunk[]  _chunkArray;
    private Vector3[] _chunkLocalCenters;
    private Vector3[] _chunkWorldCenters;
    private int[]     _chunkBuildState;

    // ── Queues / priority sets ─────────────────────────────────────────────────────────────

    private readonly ConcurrentQueue<int>                            _uploadQueue;
    private readonly ConcurrentQueue<int>                            _pendingRebuildQueue;
    private readonly HashSet<int>                                    _lodPrioritySet;
    private readonly List<(int Index, float DistSq, int Category, int Lod)> _sortBuffer;

    // ── Transform cache ────────────────────────────────────────────────────────────────────

    private Vector3    _cachedPos;
    private Quaternion _cachedRot;
    private Vector3    _cachedScale;
    private Vector3    _lastCamPos;

    private readonly Func<Task<ITerrain>> _terrainFactory;

    // ══════════════════════════════════════════════════════════════════════════════════════════
    // Constructor
    // ══════════════════════════════════════════════════════════════════════════════════════════

    public Terrain3D(Func<Task<ITerrain>> terrainFactory, Vector3 offsetPosition, int maxChunkUploadsPerFrame = 4, bool frustumCulling = true) : base(offsetPosition) {
        this._terrainFactory          = terrainFactory;
        this.MaxChunkUploadsPerFrame  = Math.Max(1, maxChunkUploadsPerFrame);
        this.FrustumCulling           = frustumCulling;
        this.RegionBoxColor           = new Color(0, 100, 255, 255);   // Blue
        this.ChunkBoxColor            = new Color(160, 160, 160, 255); // Gray
        this._lodDistances            = [200.0F, 400.0F, 800.0F];
        this._lodDistancesSq          = [40000.0F, 160000.0F, 640000.0F];
        this._lodSteps                = [2, 4, 8];
        this._chunkArray              = [];
        this._chunkLocalCenters       = [];
        this._chunkWorldCenters       = [];
        this._chunkBuildState         = [];
        this._chunkVertexCache        = [];
        this._chunkIndexCache         = [];
        this._chunkToRegion           = [];
        this._regionBatches           = [];
        this._regionBuildState        = [];
        this._regionNeedsRebuildAfterCurrent = [];
        this._uploadQueue             = new ConcurrentQueue<int>();
        this._pendingRebuildQueue     = new ConcurrentQueue<int>();
        this._regionUploadQueue       = new ConcurrentQueue<(int, Vertex3D[], uint[])>();
        this._dirtyRegions            = new HashSet<int>();
        this._lodPrioritySet          = new HashSet<int>();
        this._sortBuffer              = new List<(int, float, int, int)>();
        this._cachedRot               = Quaternion.Identity;
        this._cachedScale             = Vector3.One;
    }

    // ══════════════════════════════════════════════════════════════════════════════════════════
    // Init
    // ══════════════════════════════════════════════════════════════════════════════════════════

    protected internal override void Init() {
        base.Init();
        this.Terrain     = this._terrainFactory().GetAwaiter().GetResult();
        this._chunkArray = this.Terrain.GetChunks().ToArray();

        int n = this._chunkArray.Length;
        this._chunkLocalCenters = new Vector3[n];
        this._chunkWorldCenters = new Vector3[n];
        this._chunkBuildState   = new int[n];
        this._chunkVertexCache  = new Vertex3D[]?[n];
        this._chunkIndexCache   = new uint[]?[n];
        this._chunkToRegion     = new int[n];

        for (int i = 0; i < n; i++) {
            IChunk c = this._chunkArray[i];
            this._chunkLocalCenters[i] = c.Position + new Vector3(c.Width * 0.5F, c.Height * 0.5F, c.Depth * 0.5F);
        }

        if (this.Terrain is MarchingCubesTerrain mct) {
            mct.OnChunkMarkedDirty += this.EnqueuePendingRebuild;
        }

        this.RebuildLodDistancesSq();
        this.RefreshChunkWorldCenters(force: true);
        this.UpdateLodLevels();
        this.BuildRegionBatches();

        // Generate all chunk geometries in parallel.
        Parallel.For(0, n, i => this._chunkArray[i].GenerateGeometry());

        // Cache all chunk CPU data.
        for (int i = 0; i < n; i++) {
            this.CacheChunkData(i);
        }

        // Merge all regions in parallel (CPU only), then upload sequentially on the main thread.
        var mergedRegions = new (Vertex3D[] Verts, uint[] Inds)[this._regionBatches.Length];
        Parallel.For(0, this._regionBatches.Length, r => mergedRegions[r] = this.MergeRegionCpu(r));

        for (int r = 0; r < this._regionBatches.Length; r++) {
            this.UploadRegionBatch(this.GraphicsDevice, r, mergedRegions[r].Verts, mergedRegions[r].Inds);
        }

        this._lodPrioritySet.Clear();
        this._dirtyRegions.Clear();

        // Drain any notifications fired during init — handled by the loops above.
        while (this._pendingRebuildQueue.TryDequeue(out _)) { }
    }

    // ══════════════════════════════════════════════════════════════════════════════════════════
    // Draw
    // ══════════════════════════════════════════════════════════════════════════════════════════

    protected internal override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        base.Draw(context, framebuffer);

        this.VisibleChunkCount  = 0;
        this.VisibleVertexCount = 0;

        this.RefreshChunkWorldCenters(force: false);
        this.UpdateLodLevels();
        this.ProcessDirtyChunks(context.GraphicsDevice);

        Camera3D? cam3D      = SceneManager.ActiveCam3D;
        bool      doFrustum  = cam3D != null && this.FrustumCulling;
        float     maxDistSq  = this._lodDistancesSq.Length > 0 ? this._lodDistancesSq[^1] : 0f;
        float     cullDistSq = maxDistSq * 1.1f;

        foreach (RegionBatch batch in this._regionBatches) {
            if (batch.Renderable == null) {
                continue;
            }

            Vector3 regionWorldOrigin = this._cachedPos + Vector3.Transform(batch.LocalOrigin * this._cachedScale, this._cachedRot);

            Transform regionTransform = new Transform {
                Translation = regionWorldOrigin,
                Rotation    = this._cachedRot,
                Scale       = this._cachedScale
            };

            // Distance cull.
            Vector3 regionWorldCenter = regionWorldOrigin + Vector3.Transform((batch.LocalBounds.Max * 0.5f) * this._cachedScale, this._cachedRot);

            if (cam3D != null && Vector3.DistanceSquared(cam3D.Position, regionWorldCenter) > cullDistSq) {
                continue;
            }

            // Frustum cull.
            if (doFrustum) {
                BoundingBox frustumBox = this.ComputeFrustumBox(batch.LocalBounds, in regionTransform);

                if (!cam3D!.GetFrustum().ContainsOrientedBox(frustumBox, regionTransform.Translation, regionTransform.Rotation)) {
                    continue;
                }
            }

            this.VisibleChunkCount++;
            this.VisibleVertexCount += (int) batch.Mesh!.VertexCount;

            batch.Renderable.SetTransform(0, regionTransform);
            this.Entity.Scene.Renderer.DrawRenderable(batch.Renderable);
        }
    }

    /// <inheritdoc/>
    public void DrawDebug(ImmediateRenderer immediateRenderer) {
        // Draw region bounding boxes in blue.
        foreach (RegionBatch batch in this._regionBatches) {
            Vector3 regionWorldOrigin = this._cachedPos + Vector3.Transform(batch.LocalOrigin * this._cachedScale, this._cachedRot);

            Transform regionTransform = new Transform {
                Translation = regionWorldOrigin,
                Rotation    = this._cachedRot,
                Scale       = this._cachedScale
            };

            immediateRenderer.DrawBoundingBox(regionTransform, batch.LocalBounds, this.RegionBoxColor);
        }

        // Draw individual chunk bounding boxes in gray.
        for (int i = 0; i < this._chunkArray.Length; i++) {
            IChunk chunk = this._chunkArray[i];

            // Skip empty/culled chunks so the view isn't cluttered with invisible volumes.
            if (this._chunkVertexCache[i] == null) {
                continue;
            }

            Vector3 chunkWorldOrigin = this._cachedPos + Vector3.Transform(chunk.Position * this._cachedScale, this._cachedRot);

            Transform chunkTransform = new Transform {
                Translation = chunkWorldOrigin,
                Rotation    = this._cachedRot,
                Scale       = this._cachedScale
            };

            BoundingBox chunkBounds = new BoundingBox(Vector3.Zero, new Vector3(chunk.Width, chunk.Height, chunk.Depth));
            immediateRenderer.DrawBoundingBox(chunkTransform, chunkBounds, this.ChunkBoxColor);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════════════════════
    // Dirty chunk pipeline
    // ══════════════════════════════════════════════════════════════════════════════════════════

    private void ProcessDirtyChunks(GraphicsDevice graphicsDevice) {
        // ── LOD priority scheduling ────────────────────────────────────────────────────────────
        if (this._lodPrioritySet.Count > 0) {
            Camera3D? cam3D = SceneManager.ActiveCam3D;

            if (cam3D != null) {
                Vector3 cameraPos = cam3D.Position;
                this._sortBuffer.Clear();

                foreach (int idx in this._lodPrioritySet) {
                    IChunk chunk    = this._chunkArray[idx];
                    int    category = chunk.Lod == -1
                        ? 2
                        : chunk.CurrentLod is -2 or -1 || chunk.Lod < chunk.CurrentLod
                            ? 0
                            : chunk.Lod > chunk.CurrentLod
                                ? 1
                                : 3;

                    this._sortBuffer.Add((idx, Vector3.DistanceSquared(cameraPos, this._chunkWorldCenters[idx]), category, chunk.Lod));
                }

                this._sortBuffer.Sort(static (a, b) => {
                    if (a.Category != b.Category) return a.Category.CompareTo(b.Category);

                    return a.Category switch {
                        0 => a.Lod != b.Lod ? a.Lod.CompareTo(b.Lod) : a.DistSq.CompareTo(b.DistSq),
                        1 => a.Lod != b.Lod ? b.Lod.CompareTo(a.Lod) : b.DistSq.CompareTo(a.DistSq),
                        _ => b.DistSq.CompareTo(a.DistSq)
                    };
                });

                foreach ((int idx, _, _, _) in this._sortBuffer) {
                    this.ScheduleChunkBackground(idx);
                }
            }
            else {
                foreach (int idx in this._lodPrioritySet) {
                    this.ScheduleChunkBackground(idx);
                }
            }

            this._lodPrioritySet.Clear();
        }

        // ── Dirty chunk scheduling ─────────────────────────────────────────────────────────────
        if (this.Terrain is MarchingCubesTerrain) {
            while (this._pendingRebuildQueue.TryDequeue(out int pendingIdx)) {
                if (this._chunkBuildState[pendingIdx] == StatIdle && this._chunkArray[pendingIdx].IsDirty) {
                    this.ScheduleChunkBackground(pendingIdx);
                }
            }
        }
        else {
            for (int i = 0; i < this._chunkArray.Length; i++) {
                if (this._chunkBuildState[i] == StatIdle && this._chunkArray[i].IsDirty) {
                    this.ScheduleChunkBackground(i);
                }
            }
        }

        // ── Cache finished chunks and mark their regions dirty ─────────────────────────────────
        int processed = 0;

        while (processed < this.MaxChunkUploadsPerFrame && this._uploadQueue.TryDequeue(out int idx)) {
            this.CacheChunkData(idx);
            processed++;
        }

        // ── Schedule async CPU merges for dirty regions ────────────────────────────────────────
        // The heavy vertex merge work is done on a background thread (see ScheduleRegionRebuild).
        // Only scheduling and the later GPU upload happen on the main thread.
        foreach (int regionIdx in this._dirtyRegions) {
            this.ScheduleRegionRebuild(regionIdx);
        }

        this._dirtyRegions.Clear();

        // ── GPU upload for completed region merges ─────────────────────────────────────────────
        // Background tasks enqueue here when their CPU merge is done.
        // Creating the Mesh<Vertex3D> (buffer allocation + DMA upload) must be on the main thread
        // but is fast compared to the vertex merge, so it does not cause visible frame drops.
        while (this._regionUploadQueue.TryDequeue(out var item)) {
            this.UploadRegionBatch(graphicsDevice, item.RegionIdx, item.Vertices, item.Indices);
        }
    }

    private void EnqueuePendingRebuild(int index) {
        this._pendingRebuildQueue.Enqueue(index);
    }

    /// <summary>
    /// Atomically transitions a chunk from idle to generating and kicks off a background Task.
    /// No-op when the chunk is already in flight.
    /// </summary>
    private void ScheduleChunkBackground(int index) {
        if (Interlocked.CompareExchange(ref this._chunkBuildState[index], StatGenerating, StatIdle) != StatIdle) {
            return;
        }

        IChunk chunk = this._chunkArray[index];

        Task.Run(() => {
            chunk.GenerateGeometry();
            Interlocked.Exchange(ref this._chunkBuildState[index], StatReady);
            this._uploadQueue.Enqueue(index);
        });
    }

    /// <summary>
    /// Reads the chunk's pending CPU geometry into the per-chunk cache, discards the individual
    /// GPU mesh, resets the build state, and marks the owning region dirty for an async rebuild.
    /// Called on the main thread after a chunk's background <see cref="IChunk.GenerateGeometry"/> finishes.
    /// </summary>
    private void CacheChunkData(int chunkIdx) {
        if (this._chunkArray[chunkIdx] is MarchingCubesChunk mcc) {
            Vertex3D[] verts = mcc.GetPendingVertices();
            uint[]     inds  = mcc.GetPendingIndices();

            this._chunkVertexCache[chunkIdx] = verts.Length > 0 ? verts : null;
            this._chunkIndexCache[chunkIdx]  = verts.Length > 0 ? inds  : null;

            mcc.DiscardGeometry();
        }

        Interlocked.Exchange(ref this._chunkBuildState[chunkIdx], StatIdle);
        this._dirtyRegions.Add(this._chunkToRegion[chunkIdx]);

        if (this._chunkArray[chunkIdx].IsDirty) {
            this.ScheduleChunkBackground(chunkIdx);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════════════════════
    // Async region rebuild — the fix for editing performance
    // ══════════════════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Schedules an async CPU merge for the region at <paramref name="regionIdx"/>.
    ///
    /// Main-thread work (this method):
    ///   • Atomic CAS to claim the region (skips if already building).
    ///   • Takes a lightweight snapshot of cached vertex/index array references — O(chunks_per_region) pointer copies.
    ///
    /// Background-thread work (Task.Run):
    ///   • Iterates the snapshot arrays and merges vertices with chunk-to-region offsets applied.
    ///   • Enqueues the finished arrays to <see cref="_regionUploadQueue"/>.
    ///
    /// Main-thread work (next drain in <see cref="ProcessDirtyChunks"/>):
    ///   • Creates <see cref="Mesh{T}"/> from merged arrays (GPU buffer alloc + DMA upload).
    ///   • Replaces the old batch mesh.
    /// </summary>
    private void ScheduleRegionRebuild(int regionIdx) {
        // Only one rebuild in flight per region at a time.
        if (Interlocked.CompareExchange(ref this._regionBuildState[regionIdx], RegStatBuilding, RegStatIdle) != RegStatIdle) {
            // Already rebuilding — flag so we kick off another one when the current finishes.
            this._regionNeedsRebuildAfterCurrent[regionIdx] = true;
            return;
        }

        RegionBatch batch = this._regionBatches[regionIdx];
        int         n     = batch.ChunkIndices.Length;

        // Snapshot the cache array references on the main thread.
        // The background thread reads these arrays but never writes to them.
        // The main thread may later replace the references in the cache arrays (a different slot),
        // but since each slot is a separate array object this does not affect the snapshot.
        var vertSnap = new Vertex3D[]?[n];
        var idxSnap  = new uint[]?[n];

        for (int i = 0; i < n; i++) {
            vertSnap[i] = this._chunkVertexCache[batch.ChunkIndices[i]];
            idxSnap[i]  = this._chunkIndexCache[batch.ChunkIndices[i]];
        }

        // Capture everything the background lambda needs (avoid capturing 'this' fields that change).
        int     capturedRegionIdx = regionIdx;
        IChunk[] chunkArray       = this._chunkArray;
        Vector3 localOrigin       = batch.LocalOrigin;
        int[]   chunkIndices      = batch.ChunkIndices;

        Task.Run(() => {
            // ── CPU merge (background thread) ──────────────────────────────────────────────────
            // This is the expensive part — iterating and copying all vertex data.
            // Running it here keeps the main thread free to render at full speed.
            List<Vertex3D> mergedVerts = new List<Vertex3D>();
            List<uint>     mergedInds  = new List<uint>();

            for (int i = 0; i < n; i++) {
                Vertex3D[]? verts = vertSnap[i];
                uint[]?     inds  = idxSnap[i];

                if (verts == null || verts.Length == 0) {
                    continue;
                }

                uint    indexOffset = (uint) mergedVerts.Count;
                // Offset from chunk-local space into region-local space.
                Vector3 chunkOffset = chunkArray[chunkIndices[i]].Position - localOrigin;

                foreach (Vertex3D v in verts) {
                    Vertex3D sv = v;
                    sv.Position += chunkOffset;
                    mergedVerts.Add(sv);
                }

                foreach (uint idx in inds!) {
                    mergedInds.Add(idx + indexOffset);
                }
            }

            // Deliver the merged arrays back to the main thread for GPU upload.
            this._regionUploadQueue.Enqueue((capturedRegionIdx, mergedVerts.ToArray(), mergedInds.ToArray()));
        });
    }

    /// <summary>
    /// Performs the GPU-side part of a region rebuild: creates a new <see cref="Mesh{T}"/> from
    /// the pre-merged vertex/index arrays and replaces the region's batch mesh.
    /// Must be called on the main (render) thread.
    /// If the region was marked dirty again while its merge was running, a new async rebuild is
    /// scheduled immediately so the next edit appears without an extra frame of delay.
    /// </summary>
    private void UploadRegionBatch(GraphicsDevice graphicsDevice, int regionIdx, Vertex3D[] vertices, uint[] indices) {
        RegionBatch batch = this._regionBatches[regionIdx];

        // Release previous GPU resources.
        batch.Mesh?.Dispose();
        batch.Mesh       = null;
        batch.Renderable = null;

        if (vertices.Length > 0 && indices.Length > 0) {
            Material material = new Material(GlobalResource.DefaultModelEffect);

            material.AddMaterialMap(MaterialMapType.Albedo, 0, new MaterialMap {
                Texture = GlobalResource.DefaultModelTexture,
                Color   = Color.White
            });

            if (this.DebugDrawEnabled) {
                material.RasterizerState = new RasterizerStateDescription {
                    FillMode = PolygonFillMode.Wireframe
                };
            }

            batch.Mesh       = new Mesh<Vertex3D>(graphicsDevice, material, new BasicMeshData(vertices, indices));
            batch.Renderable = new Renderable(batch.Mesh, new Transform(), batch.Mesh.Material);
        }

        // Mark the region idle.
        Interlocked.Exchange(ref this._regionBuildState[regionIdx], RegStatIdle);

        // If the region was dirtied while we were merging, kick off another rebuild now
        // so the player sees the most up-to-date result as soon as the new merge completes.
        if (this._regionNeedsRebuildAfterCurrent[regionIdx]) {
            this._regionNeedsRebuildAfterCurrent[regionIdx] = false;
            this.ScheduleRegionRebuild(regionIdx);
        }
    }

    /// <summary>
    /// Pure CPU merge of all cached chunk geometries for a region into a single vertex/index pair.
    /// Thread-safe to run in parallel across different regions (each region reads different chunk indices).
    /// Used during <see cref="Init"/> where the cache is fully populated and not being written.
    /// </summary>
    private (Vertex3D[] Vertices, uint[] Indices) MergeRegionCpu(int regionIdx) {
        RegionBatch    batch       = this._regionBatches[regionIdx];
        List<Vertex3D> mergedVerts = new List<Vertex3D>();
        List<uint>     mergedInds  = new List<uint>();

        foreach (int chunkIdx in batch.ChunkIndices) {
            Vertex3D[]? verts = this._chunkVertexCache[chunkIdx];
            uint[]?     inds  = this._chunkIndexCache[chunkIdx];

            if (verts == null || verts.Length == 0) {
                continue;
            }

            uint    indexOffset = (uint) mergedVerts.Count;
            Vector3 chunkOffset = this._chunkArray[chunkIdx].Position - batch.LocalOrigin;

            foreach (Vertex3D v in verts) {
                Vertex3D sv = v;
                sv.Position += chunkOffset;
                mergedVerts.Add(sv);
            }

            foreach (uint idx in inds!) {
                mergedInds.Add(idx + indexOffset);
            }
        }

        return (mergedVerts.ToArray(), mergedInds.ToArray());
    }

    // ══════════════════════════════════════════════════════════════════════════════════════════
    // Region structure
    // ══════════════════════════════════════════════════════════════════════════════════════════

    private void BuildRegionBatches() {
        if (this.Terrain is not MarchingCubesTerrain mct) {
            this._regionBatches                  = Array.Empty<RegionBatch>();
            this._regionBuildState               = Array.Empty<int>();
            this._regionNeedsRebuildAfterCurrent = Array.Empty<bool>();
            return;
        }

        int chunkSize   = mct.ChunkSize;
        int chunkCountX = (mct.Width  + chunkSize - 1) / chunkSize;
        int chunkCountY = (mct.Height + chunkSize - 1) / chunkSize;
        int chunkCountZ = (mct.Depth  + chunkSize - 1) / chunkSize;

        int regionCountX = (chunkCountX + RegionChunkCount - 1) / RegionChunkCount;
        int regionCountZ = (chunkCountZ + RegionChunkCount - 1) / RegionChunkCount;

        int totalRegions = regionCountX * regionCountZ;
        this._regionBatches                  = new RegionBatch[totalRegions];
        this._regionBuildState               = new int[totalRegions];
        this._regionNeedsRebuildAfterCurrent = new bool[totalRegions];

        for (int rx = 0; rx < regionCountX; rx++) {
            for (int rz = 0; rz < regionCountZ; rz++) {
                int cxStart = rx * RegionChunkCount;
                int czStart = rz * RegionChunkCount;
                int cxEnd   = Math.Min(cxStart + RegionChunkCount, chunkCountX);
                int czEnd   = Math.Min(czStart + RegionChunkCount, chunkCountZ);

                List<int> chunkIndices = new List<int>();

                for (int cx = cxStart; cx < cxEnd; cx++) {
                    for (int cy = 0; cy < chunkCountY; cy++) {
                        for (int cz = czStart; cz < czEnd; cz++) {
                            int chunkIdx = cx * chunkCountY * chunkCountZ + cy * chunkCountZ + cz;
                            chunkIndices.Add(chunkIdx);
                            this._chunkToRegion[chunkIdx] = rx * regionCountZ + rz;
                        }
                    }
                }

                Vector3 localOrigin = new Vector3(cxStart * chunkSize, 0, czStart * chunkSize);
                Vector3 extent = new Vector3(
                    Math.Min((cxEnd - cxStart) * chunkSize, mct.Width  - (int) localOrigin.X),
                    mct.Height,
                    Math.Min((czEnd - czStart) * chunkSize, mct.Depth  - (int) localOrigin.Z));

                int regionIdx = rx * regionCountZ + rz;
                this._regionBatches[regionIdx] = new RegionBatch(chunkIndices.ToArray(), localOrigin, extent);
            }
        }
    }

    // ══════════════════════════════════════════════════════════════════════════════════════════
    // Transform and LOD helpers
    // ══════════════════════════════════════════════════════════════════════════════════════════

    private void RefreshChunkWorldCenters(bool force) {
        Vector3    pos   = this.LerpedGlobalPosition;
        Quaternion rot   = this.LerpedRotation;
        Vector3    scale = this.LerpedScale;

        if (!force
            && Vector3.DistanceSquared(pos, this._cachedPos) < 1e-6f
            && Quaternion.Dot(rot, this._cachedRot) > 0.9999f
            && Vector3.DistanceSquared(scale, this._cachedScale) < 1e-6f) {
            return;
        }

        this._cachedPos   = pos;
        this._cachedRot   = rot;
        this._cachedScale = scale;

        for (int i = 0; i < this._chunkArray.Length; i++) {
            this._chunkWorldCenters[i] = pos + Vector3.Transform(this._chunkLocalCenters[i] * scale, rot);
        }
    }

    private void RebuildLodDistancesSq() {
        this._lodDistancesSq = new float[this._lodDistances.Length];
        this._lodSteps       = new int[this._lodDistances.Length];

        for (int i = 0; i < this._lodDistances.Length; i++) {
            this._lodDistancesSq[i] = this._lodDistances[i] * this._lodDistances[i];
            this._lodSteps[i]       = 1 << (i + 1);
        }
    }

    private void UpdateLodLevels() {
        Camera3D? cam3D = SceneManager.ActiveCam3D;

        if (cam3D == null) {
            return;
        }

        Vector3 cameraPos = cam3D.Position;

        if (Vector3.DistanceSquared(cameraPos, this._lastCamPos) < 4.0f) {
            return;
        }

        this._lastCamPos = cameraPos;

        float maxDistSq       = this._lodDistancesSq.Length > 0 ? this._lodDistancesSq[^1] : 0f;
        float cullThresholdSq = maxDistSq * 1.1f;

        for (int i = 0; i < this._chunkArray.Length; i++) {
            IChunk chunk  = this._chunkArray[i];
            float  distSq = Vector3.DistanceSquared(cameraPos, this._chunkWorldCenters[i]);

            if (distSq > cullThresholdSq) {
                if (chunk.Lod != -1) {
                    chunk.Lod = -1;
                    chunk.MarkDirty();
                    this._lodPrioritySet.Add(i);
                }

                continue;
            }

            int targetLod = 1;

            for (int li = 0; li < this._lodDistancesSq.Length; li++) {
                if (distSq > this._lodDistancesSq[li]) {
                    targetLod = this._lodSteps[li];
                }
                else {
                    break;
                }
            }

            if (chunk.Lod != targetLod) {
                chunk.Lod = targetLod;
                chunk.MarkDirty();
                this._lodPrioritySet.Add(i);
            }
        }
    }

    private BoundingBox ComputeFrustumBox(BoundingBox localBox, in Transform ct) {
        Vector3 center    = (localBox.Min + localBox.Max) * 0.5F;
        Vector3 dimension = (localBox.Max - localBox.Min) * ct.Scale;
        Vector3 offset    = center * ct.Scale;

        float maxSide = Math.Max(dimension.X, dimension.Z);
        dimension.X = maxSide;
        dimension.Z = maxSide;

        Vector3 fc = ct.Translation + offset;

        return new BoundingBox {
            Min = new Vector3(fc.X - dimension.X * 0.5F, ct.Translation.Y + localBox.Min.Y * ct.Scale.Y, fc.Z - dimension.Z * 0.5F),
            Max = new Vector3(fc.X + dimension.X * 0.5F, ct.Translation.Y + localBox.Max.Y * ct.Scale.Y, fc.Z + dimension.Z * 0.5F)
        };
    }

    // ══════════════════════════════════════════════════════════════════════════════════════════
    // Inner types
    // ══════════════════════════════════════════════════════════════════════════════════════════

    private sealed class RegionBatch {
        public readonly int[]       ChunkIndices;
        public readonly Vector3     LocalOrigin;
        public readonly BoundingBox LocalBounds;
        public IMesh?      Mesh;
        public Renderable? Renderable;

        public RegionBatch(int[] chunkIndices, Vector3 localOrigin, Vector3 extent) {
            this.ChunkIndices = chunkIndices;
            this.LocalOrigin  = localOrigin;
            this.LocalBounds  = new BoundingBox(Vector3.Zero, extent);
        }

        public void Dispose() {
            this.Mesh?.Dispose();
            this.Mesh       = null;
            this.Renderable = null;
        }
    }

    // ══════════════════════════════════════════════════════════════════════════════════════════
    // Dispose
    // ══════════════════════════════════════════════════════════════════════════════════════════

    protected override void Dispose(bool disposing) {
        if (disposing) {
            if (this.Terrain is MarchingCubesTerrain mct) {
                mct.OnChunkMarkedDirty -= this.EnqueuePendingRebuild;
            }

            foreach (RegionBatch batch in this._regionBatches) {
                batch.Dispose();
            }

            for (int i = 0; i < this._chunkArray.Length; i++) {
                this._chunkArray[i].Dispose();
            }
        }
    }
}