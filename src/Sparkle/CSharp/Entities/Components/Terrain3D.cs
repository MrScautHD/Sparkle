using System.Collections.Concurrent;
using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Geometry;
using Bliss.CSharp.Graphics.Rendering.Renderers;
using Bliss.CSharp.Graphics.Rendering.Renderers.Forward;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.Graphics.Rendering.Gizmos;
using Sparkle.CSharp.Scenes;
using Sparkle.CSharp.Terrain;
using Veldrid;

namespace Sparkle.CSharp.Entities.Components;

/// <summary>
/// A component that renders a chunked, LOD-aware 3D terrain on the entity it is attached to.
/// </summary>
public class Terrain3D : InterpolatedComponent, IDebugDrawable {

    /// <summary>
    /// The terrain data source that provides chunks and dimension information.
    /// </summary>
    public ITerrain Terrain { get; private set; }

    /// <summary>
    /// Maximum number of chunks whose meshes are uploaded to the GPU per rendered frame.
    /// </summary>
    public int MaxChunkUploadsPerFrame;

    /// <summary>
    /// When <c>true</c>, chunks outside the active camera frustum are skipped during rendering.
    /// </summary>
    public bool FrustumCulling;

    /// <summary>
    /// The color used to draw chunk bounding boxes when <see cref="DebugDrawEnabled"/> is <c>true</c>.
    /// </summary>
    public Color BoxColor;

    /// <summary>
    /// Gets or sets whether debug visuals (bounding boxes and wireframe meshes) are drawn.
    /// </summary>
    public bool DebugDrawEnabled { get; set; }

    /// <summary>
    /// Gets the number of chunks submitted for rendering during the most recent frame.
    /// </summary>
    public int VisibleChunkCount { get; private set; }

    /// <summary>
    /// Gets the total number of vertices rendered during the most recent frame.
    /// </summary>
    public int VisibleVertexCount { get; private set; }

    /// <summary>
    /// Gets or sets the camera distances at which the terrain steps to the next LOD level.
    /// </summary>
    public float[] LodDistances {
        get => this._lodDistances;
        set {
            this._lodDistances = value;
            this.RebuildLodDistancesSq();
        }
    }

    /// <summary>
    /// Gets the total vertex count across every chunk that currently has an uploaded mesh.
    /// </summary>
    public int TotalVertexCount {
        get {
            int count = 0;

            for (int i = 0; i < this._chunkArray.Length; i++) {
                count += (int) (this._chunkArray[i].Mesh?.VertexCount ?? 0);
            }

            return count;
        }
    }

    /// <summary>
    /// Build-state value indicating the chunk is idle and ready to be scheduled.
    /// </summary>
    private const int StatIdle = 0;

    /// <summary>
    /// Build-state value indicating the chunk is currently generating geometry on a background thread.
    /// </summary>
    private const int StatGenerating = 1;

    /// <summary>
    /// Build-state value indicating geometry is ready and waiting for a GPU upload on the render thread.
    /// </summary>
    private const int StatReady = 2;

    /// <summary>
    /// The raw LOD distance thresholds in world units.
    /// </summary>
    private float[] _lodDistances;

    /// <summary>
    /// Precomputed squares of <see cref="_lodDistances"/> to avoid square-root calls during LOD evaluation.
    /// </summary>
    private float[] _lodDistancesSq;

    /// <summary>
    /// Precomputed LOD step values for each distance band, avoiding <see cref="Math.Pow"/> calls in the per-chunk LOD loop.
    /// </summary>
    private int[] _lodSteps;

    /// <summary>
    /// All chunks provided by <see cref="Terrain"/>, paired with their render data via <see cref="_renderData"/>.
    /// </summary>
    private IChunk[] _chunkArray;

    /// <summary>
    /// Per-chunk GPU renderables and local bounding boxes, indexed in parallel with <see cref="_chunkArray"/>.
    /// A null <see cref="ChunkRenderData.Renderable"/> means the chunk has no uploaded mesh.
    /// </summary>
    private ChunkRenderData[] _renderData;

    /// <summary>
    /// The world-space transform of each chunk, rebuilt only when the entity transform changes.
    /// </summary>
    private Transform[] _chunkTransforms;

    /// <summary>
    /// The center of each chunk in local space, precomputed once during <see cref="Init"/>.
    /// </summary>
    private Vector3[] _chunkLocalCenters;

    /// <summary>
    /// The center of each chunk in world space, rebuilt only when the entity transform changes.
    /// </summary>
    private Vector3[] _chunkWorldCenters;

    /// <summary>
    /// Per-chunk build state accessed atomically via <see cref="Interlocked"/>.
    /// </summary>
    private int[] _chunkBuildState;

    /// <summary>
    /// Chunk indices whose geometry is ready on the CPU and are awaiting a GPU upload on the render thread.
    /// </summary>
    private readonly ConcurrentQueue<int> _uploadQueue;

    /// <summary>
    /// Chunk indices whose LOD level changed this frame and need prioritised background scheduling.
    /// </summary>
    private readonly HashSet<int> _lodPrioritySet;

    /// <summary>
    /// Reusable sort buffer for LOD-priority scheduling, kept as a field to avoid per-frame allocations.
    /// </summary>
    private readonly List<(int Index, float DistSq, int Category, int Lod)> _sortBuffer;

    /// <summary>
    /// Cached entity position used to detect transform changes and skip redundant chunk transform rebuilds.
    /// </summary>
    private Vector3 _cachedPos;

    /// <summary>
    /// Cached entity rotation used to detect transform changes and skip redundant chunk transform rebuilds.
    /// </summary>
    private Quaternion _cachedRot;

    /// <summary>
    /// Cached entity scale used to detect transform changes and skip redundant chunk transform rebuilds.
    /// </summary>
    private Vector3 _cachedScale;

    /// <summary>
    /// Camera position from the last <see cref="UpdateLodLevels"/> pass, used to skip the O(n) LOD evaluation when the camera has not moved.
    /// </summary>
    private Vector3 _lastCamPos;

    /// <summary>
    /// The async factory used to construct the <see cref="ITerrain"/> instance during <see cref="Init"/>.
    /// </summary>
    private readonly Func<Task<ITerrain>> _terrainFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="Terrain3D"/> class.
    /// </summary>
    /// <param name="terrainFactory">An async factory that constructs the <see cref="ITerrain"/> instance, invoked once during <see cref="Init"/>.</param>
    /// <param name="offsetPosition">The positional offset applied to the component relative to its entity.</param>
    /// <param name="maxChunkUploadsPerFrame">Maximum chunks uploaded to the GPU per frame. Defaults to 4.</param>
    /// <param name="frustumCulling">Whether to skip chunks outside the camera frustum. Defaults to <c>true</c>.</param>
    /// <param name="boxColor">Color used for debug bounding boxes. Defaults to <see cref="Color.White"/> when <c>null</c>.</param>
    public Terrain3D(Func<Task<ITerrain>> terrainFactory, Vector3 offsetPosition, int maxChunkUploadsPerFrame = 4, bool frustumCulling = true, Color? boxColor = null) : base(offsetPosition) {
        this._terrainFactory = terrainFactory;
        this.MaxChunkUploadsPerFrame = Math.Max(1, maxChunkUploadsPerFrame);
        this.FrustumCulling = frustumCulling;
        this.BoxColor = boxColor ?? Color.White;
        this._lodDistances = [200.0F, 400.0F, 800.0F];
        this._lodDistancesSq = [40000.0F, 160000.0F, 640000.0F];
        this._lodSteps = [2, 4, 8];
        this._chunkArray = [];
        this._renderData = [];
        this._chunkTransforms = [];
        this._chunkLocalCenters = [];
        this._chunkWorldCenters = [];
        this._chunkBuildState = [];
        this._uploadQueue = new ConcurrentQueue<int>();
        this._lodPrioritySet = new HashSet<int>();
        this._sortBuffer = new List<(int, float, int, int)>();
        this._cachedRot = Quaternion.Identity;
        this._cachedScale = Vector3.One;
    }

    /// <summary>
    /// Invokes the terrain factory, allocates per-chunk arrays, and generates all chunk meshes before the first frame.
    /// </summary>
    protected internal override void Init() {
        base.Init();
        this.Terrain = this._terrainFactory().GetAwaiter().GetResult();
        this._chunkArray = this.Terrain.GetChunks().ToArray();
        this._renderData = new ChunkRenderData[this._chunkArray.Length];
        this._chunkTransforms = new Transform[this._chunkArray.Length];
        this._chunkLocalCenters = new Vector3[this._chunkArray.Length];
        this._chunkWorldCenters = new Vector3[this._chunkArray.Length];
        this._chunkBuildState = new int[this._chunkArray.Length];

        for (int i = 0; i < this._chunkArray.Length; i++) {
            IChunk c = this._chunkArray[i];
            this._chunkLocalCenters[i] = c.Position + new Vector3(c.Width * 0.5F, c.Height * 0.5F, c.Depth * 0.5F);
        }

        this.RebuildLodDistancesSq();
        this.RefreshChunkTransforms(force: true);
        this.UpdateLodLevels();

        Parallel.For(0, this._chunkArray.Length, i => this._chunkArray[i].GenerateGeometry());

        for (int i = 0; i < this._chunkArray.Length; i++) {
            this.UploadChunk(this.GraphicsDevice, i);
        }

        this._lodPrioritySet.Clear();
    }

    /// <summary>
    /// Each frame: refreshes world-space transforms, recalculates LOD levels, drains the
    /// GPU upload queue, and submits visible chunks to the scene renderer.
    /// Chunks outside the camera frustum are skipped when <see cref="FrustumCulling"/> is enabled.
    /// </summary>
    /// <param name="context">The current rendering context.</param>
    /// <param name="framebuffer">The framebuffer to render into.</param>
    protected internal override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        base.Draw(context, framebuffer);

        this.VisibleChunkCount = 0;
        this.VisibleVertexCount = 0;

        this.RefreshChunkTransforms(force: false);
        this.UpdateLodLevels();
        this.ProcessDirtyChunks(context.GraphicsDevice);

        Camera3D? cam3D = SceneManager.ActiveCam3D;

        for (int i = 0; i < this._chunkArray.Length; i++) {
            ref ChunkRenderData data = ref this._renderData[i];

            if (data.Renderable == null) {
                continue;
            }

            bool shouldRender = true;

            if (cam3D != null && this.FrustumCulling) {
                ref Transform ct = ref this._chunkTransforms[i];
                BoundingBox frustum = this.ComputeChunkFrustumBox(data.BaseBounds, in ct);
                shouldRender = cam3D.GetFrustum().ContainsOrientedBox(frustum, ct.Translation, ct.Rotation);
            }

            if (!shouldRender) {
                continue;
            }

            this.VisibleChunkCount++;
            this.VisibleVertexCount += (int) this._chunkArray[i].Mesh!.VertexCount;

            data.Renderable.SetTransform(0, this._chunkTransforms[i]);
            this.Entity.Scene.Renderer.DrawRenderable(data.Renderable);
        }
    }

    /// <summary>
    /// Draws a bounding box for every chunk that currently has an uploaded mesh.
    /// </summary>
    /// <param name="immediateRenderer">The immediate-mode renderer used to issue debug draw calls.</param>
    public void DrawDebug(ImmediateRenderer immediateRenderer) {
        for (int i = 0; i < this._chunkArray.Length; i++) {
            if (this._renderData[i].Renderable == null) {
                continue;
            }

            immediateRenderer.DrawBoundingBox(this._chunkTransforms[i], this._renderData[i].BaseBounds, this.BoxColor);
        }
    }

    /// <summary>
    /// Schedules dirty chunks for background geometry generation, then uploads any ready chunks to the GPU.
    /// LOD-priority chunks are sorted by camera distance and scheduled first to minimise visible popping.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used to create GPU buffers.</param>
    private void ProcessDirtyChunks(GraphicsDevice graphicsDevice) {
        if (this._lodPrioritySet.Count > 0) {
            Camera3D? cam3D = SceneManager.ActiveCam3D;

            if (cam3D != null) {
                Vector3 cameraPos = cam3D.Position;
                this._sortBuffer.Clear();

                foreach (int idx in this._lodPrioritySet) {
                    IChunk chunk = this._chunkArray[idx];
                    int category;

                    if (chunk.Lod == -1) {
                        category = 2;
                    }
                    else if (chunk.CurrentLod is -2 or -1 || chunk.Lod < chunk.CurrentLod) {
                        category = 0;
                    }
                    else if (chunk.Lod > chunk.CurrentLod) {
                        category = 1;
                    }
                    else {
                        category = 3;
                    }

                    this._sortBuffer.Add((idx, Vector3.DistanceSquared(cameraPos, this._chunkWorldCenters[idx]), category, chunk.Lod));
                }

                this._sortBuffer.Sort(static (a, b) => {
                    if (a.Category != b.Category) {
                        return a.Category.CompareTo(b.Category);
                    }

                    return a.Category switch {
                        0 => a.Lod != b.Lod ? a.Lod.CompareTo(b.Lod) : a.DistSq.CompareTo(b.DistSq),
                        1 => a.Lod != b.Lod ? b.Lod.CompareTo(a.Lod) : b.DistSq.CompareTo(a.DistSq),
                        _ => b.DistSq.CompareTo(a.DistSq)
                    };
                });

                foreach (var entry in this._sortBuffer) {
                    this.ScheduleBackground(entry.Index);
                }
            }
            else {
                foreach (int idx in this._lodPrioritySet) {
                    this.ScheduleBackground(idx);
                }
            }

            this._lodPrioritySet.Clear();
        }

        for (int i = 0; i < this._chunkArray.Length; i++) {
            if (this._chunkBuildState[i] == StatIdle && this._chunkArray[i].IsDirty) {
                this.ScheduleBackground(i);
            }
        }

        int uploaded = 0;

        while (uploaded < this.MaxChunkUploadsPerFrame && this._uploadQueue.TryDequeue(out int idx)) {
            this.UploadChunk(graphicsDevice, idx);
            uploaded++;
        }
    }

    /// <summary>
    /// Atomically transitions the chunk at <paramref name="index"/> from idle to generating and kicks off a background task.
    /// No-op if the chunk is already in flight.
    /// </summary>
    /// <param name="index">The index of the chunk within <see cref="_chunkArray"/>.</param>
    private void ScheduleBackground(int index) {
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
    /// Uploads the geometry of the chunk at <paramref name="index"/> to the GPU and refreshes its render data.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used to create GPU buffers.</param>
    /// <param name="index">The index of the chunk within <see cref="_chunkArray"/>.</param>
    private void UploadChunk(GraphicsDevice graphicsDevice, int index) {
        IChunk chunk = this._chunkArray[index];
        chunk.UploadGeometry(graphicsDevice);
        Interlocked.Exchange(ref this._chunkBuildState[index], StatIdle);

        ref ChunkRenderData data = ref this._renderData[index];

        if (chunk.Mesh == null) {
            data = default;
        }
        else {
            if (this.DebugDrawEnabled) {
                chunk.Mesh.Material.RasterizerState = new RasterizerStateDescription {
                    FillMode = PolygonFillMode.Wireframe
                };
            }

            if (data.Renderable == null || !ReferenceEquals(data.Renderable.Mesh, chunk.Mesh)) {
                data.Renderable = new Renderable(chunk.Mesh, new Transform(), chunk.Mesh.Material);
            }

            data.BaseBounds = new BoundingBox(Vector3.Zero, new Vector3(chunk.Width, chunk.Height, chunk.Depth));
        }

        if (chunk.IsDirty) {
            this.ScheduleBackground(index);
        }
    }

    /// <summary>
    /// Rebuilds chunk world-space transforms and world centers from the current entity transform.
    /// Skips the rebuild entirely when the transform has not changed, unless <paramref name="force"/> is <c>true</c>.
    /// </summary>
    /// <param name="force">When <c>true</c>, forces a full rebuild regardless of whether the transform has changed.</param>
    private void RefreshChunkTransforms(bool force) {
        Vector3 pos = this.LerpedGlobalPosition;
        Quaternion rot = this.LerpedRotation;
        Vector3 scale = this.LerpedScale;

        if (!force
            && Vector3.DistanceSquared(pos, this._cachedPos) < 1e-6f
            && Quaternion.Dot(rot, this._cachedRot) > 0.9999f
            && Vector3.DistanceSquared(scale, this._cachedScale) < 1e-6f) {
            return;
        }

        this._cachedPos = pos;
        this._cachedRot = rot;
        this._cachedScale = scale;

        for (int i = 0; i < this._chunkArray.Length; i++) {
            this._chunkTransforms[i] = new Transform {
                Translation = pos + Vector3.Transform(this._chunkArray[i].Position * scale, rot),
                Rotation = rot,
                Scale = scale
            };

            this._chunkWorldCenters[i] = pos + Vector3.Transform(this._chunkLocalCenters[i] * scale, rot);
        }
    }

    /// <summary>
    /// Rebuilds <see cref="_lodDistancesSq"/> and <see cref="_lodSteps"/> from <see cref="_lodDistances"/>.
    /// </summary>
    private void RebuildLodDistancesSq() {
        this._lodDistancesSq = new float[this._lodDistances.Length];
        this._lodSteps = new int[this._lodDistances.Length];

        for (int i = 0; i < this._lodDistances.Length; i++) {
            this._lodDistancesSq[i] = this._lodDistances[i] * this._lodDistances[i];
            this._lodSteps[i] = 1 << (i + 1);
        }
    }

    /// <summary>
    /// Evaluates the target LOD level for every chunk and marks changed chunks dirty for rebuild.
    /// Skips the pass entirely when the camera has not moved meaningfully since the last evaluation.
    /// </summary>
    private void UpdateLodLevels() {
        Camera3D? cam3D = SceneManager.ActiveCam3D;

        if (cam3D == null) {
            return;
        }

        Vector3 cameraPos = cam3D.Position;

        if (Vector3.DistanceSquared(cameraPos, this._lastCamPos) < 0.25f) {
            return;
        }

        this._lastCamPos = cameraPos;

        float maxDistSq = this._lodDistancesSq.Length > 0 ? this._lodDistancesSq[^1] : 0f;
        float cullThresholdSq = maxDistSq * 2.25f;

        for (int i = 0; i < this._chunkArray.Length; i++) {
            IChunk chunk = this._chunkArray[i];
            float distSq = Vector3.DistanceSquared(cameraPos, this._chunkWorldCenters[i]);

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

    /// <summary>
    /// Computes a conservative axis-aligned bounding box from <paramref name="baseBox"/> and the chunk transform for frustum culling.
    /// </summary>
    /// <param name="baseBox">The chunk's local-space bounding box.</param>
    /// <param name="ct">The chunk's world-space transform.</param>
    /// <returns>An axis-aligned bounding box suitable for frustum intersection tests.</returns>
    private BoundingBox ComputeChunkFrustumBox(BoundingBox baseBox, in Transform ct) {
        Vector3 center = (baseBox.Min + baseBox.Max) * 0.5F;
        Vector3 dimension = (baseBox.Max - baseBox.Min) * ct.Scale;
        Vector3 offset = center * ct.Scale;

        float maxSide = Math.Max(dimension.X, dimension.Z);
        dimension.X = maxSide;
        dimension.Z = maxSide;

        Vector3 fc = ct.Translation + offset;

        return new BoundingBox {
            Min = new Vector3(fc.X - dimension.X * 0.5F, ct.Translation.Y + baseBox.Min.Y * ct.Scale.Y, fc.Z - dimension.Z * 0.5F),
            Max = new Vector3(fc.X + dimension.X * 0.5F, ct.Translation.Y + baseBox.Max.Y * ct.Scale.Y, fc.Z + dimension.Z * 0.5F)
        };
    }

    /// <summary>
    /// Holds the GPU-side renderable and local-space bounding box for a single terrain chunk.
    /// </summary>
    private struct ChunkRenderData {

        /// <summary>The renderable used to submit the chunk mesh to the scene renderer, or <c>null</c> when the chunk has no uploaded mesh.</summary>
        public Renderable? Renderable;

        /// <summary>The axis-aligned bounding box of the chunk in local (untransformed) space.</summary>
        public BoundingBox BaseBounds;
    }

    /// <summary>
    /// Disposes all chunks and releases their associated GPU resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged.</param>
    protected override void Dispose(bool disposing) {
        if (disposing) {
            for (int i = 0; i < this._chunkArray.Length; i++) {
                this._chunkArray[i].Dispose();
            }
        }
    }
}