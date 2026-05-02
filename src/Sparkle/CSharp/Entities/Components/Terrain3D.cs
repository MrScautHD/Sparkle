using System.Collections.Concurrent;
using System.Diagnostics;
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
using Sparkle.CSharp.Terrain.Heightmap;
using Veldrid;

namespace Sparkle.CSharp.Entities.Components;

public class Terrain3D : InterpolatedComponent, IDebugDrawable {
    
    public ITerrain Terrain { get; private set; }
    
    public bool DebugDrawEnabled { get; set; }
    
    public bool FrustumCulling;
    
    public int MaxChunkUploadsPerFrame { get; set; }
    
    public int MaxChunkBuildsPerFrame { get; set; }
    
    public float ChunkBuildScheduleBudgetMilliseconds { get; set; }
    
    public int MaxConcurrentChunkBuilds { get; set; }
    
    public float[] LodDistances { get; set; }
    
    public bool EnableLod { get; set; }
    
    public float LodHysteresis { get; set; }
    
    public bool CullChunksBeyondLastLod { get; set; }
    
    public bool UseCameraFarPlaneForLodRange { get; set; }
    
    public bool PreloadChunksOnInit { get; set; }
    
    public bool PauseTerrainWorkWhenOutOfView { get; set; }
    
    public float LodUpdateIntervalSeconds { get; set; }
    
    public float LodUpdateCameraMoveThreshold { get; set; }
    
    public bool EnableFarChunkBatching { get; set; }
    
    public int FarChunkBatchMinLod { get; set; }
    
    public int FarChunkBatchRegionSizeInChunks { get; set; }
    
    public int MaxRegionRebuildsPerFrame { get; set; }
    
    public int MaxRegionUploadsPerFrame { get; set; }
    
    public float RegionScheduleBudgetMilliseconds { get; set; }
    
    public float RegionUploadBudgetMilliseconds { get; set; }
    
    public int MidRingLodUpdateIntervalTicks { get; set; }
    
    public int FarRingLodUpdateIntervalTicks { get; set; }
    
    public int TotalVertexCount { get; private set; }
    
    public int TotalIndexCount { get; private set; }
    
    private readonly Func<Task<ITerrain>> _terrainFactory;
    private readonly List<IChunk> _meshChunkList;
    private readonly ConcurrentQueue<IChunk> _pendingUpload;
    private readonly ConcurrentDictionary<IChunk, byte> _queuedSet;
    private readonly ConcurrentDictionary<IChunk, byte> _buildingSet;
    private readonly HashSet<HeightmapChunk> _chunksPendingVertexUpload;
    private readonly ConcurrentQueue<RegionBuildResult> _pendingRegionUploads;
    private readonly ConcurrentDictionary<RegionKey, byte> _regionBuildingSet;
    private readonly Dictionary<IChunk, Renderable> _renderables;
    private readonly Dictionary<IChunk, BoundingBox> _chunkLocalBounds;
    private readonly Dictionary<IChunk, Vector3> _chunkLocalCenters;
    private readonly Dictionary<IChunk, CachedWorldBounds> _chunkWorldBounds;
    private readonly Dictionary<IChunk, int> _chunkRenderableTransformVersions;
    private readonly List<IChunk> _dirtyScheduleBuffer;
    private readonly Dictionary<IChunk, RegionKey> _chunkRegions;
    private readonly Dictionary<RegionKey, HashSet<IChunk>> _regionMembers;
    private readonly Dictionary<RegionKey, RegionBatch> _regionBatches;
    private readonly Dictionary<RegionKey, CachedWorldBounds> _regionWorldBounds;
    private readonly Dictionary<RegionKey, int> _regionRenderableTransformVersions;
    private readonly HashSet<RegionKey> _dirtyRegions;
    private readonly List<RegionKey> _regionDirtyBuffer;
    
    private Task<ITerrain>? _terrainTask;
    private bool _initializedTerrain;
    private BoundingBox _terrainBaseBox;
    private BoundingBox _terrainWorldBounds;
    private int _terrainWorldBoundsVersion;
    private int _boundsTransformVersion;
    private bool _hasBoundsTransformCache;
    private Vector3 _boundsTransformPosition;
    private Quaternion _boundsTransformRotation;
    private Vector3 _boundsTransformScale;
    private int _renderTransformVersion;
    private bool _hasRenderTransformCache;
    private Vector3 _renderTransformPosition;
    private Quaternion _renderTransformRotation;
    private Vector3 _renderTransformScale;
    private Transform _sharedRenderTransform;
    private float _lodUpdateAccumulator;
    private int _lodRingTick;
    private bool _hasLastLodCameraPosition;
    private Vector3 _lastLodCameraPosition;
    
    public Terrain3D(Func<Task<ITerrain>> terrainFactory, Vector3 offsetPosition, bool frustumCulling = true) : base(offsetPosition) {
        this._terrainFactory = terrainFactory;
        this.FrustumCulling = frustumCulling;
        this.MaxChunkUploadsPerFrame = 6;
        this.MaxChunkBuildsPerFrame = 8;
        this.ChunkBuildScheduleBudgetMilliseconds = 1.5F;
        this.MaxConcurrentChunkBuilds = Math.Max(1, Environment.ProcessorCount - 1);
        this.LodDistances = [200.0F, 400.0F, 800.0F];
        this.EnableLod = true;
        this.LodHysteresis = 0.15F;
        this.CullChunksBeyondLastLod = false;
        this.UseCameraFarPlaneForLodRange = true;
        this.PreloadChunksOnInit = true;
        this.PauseTerrainWorkWhenOutOfView = true;
        this.LodUpdateIntervalSeconds = 0.08F;
        this.LodUpdateCameraMoveThreshold = 16.0F;
        this.EnableFarChunkBatching = true;
        this.FarChunkBatchMinLod = 2;
        this.FarChunkBatchRegionSizeInChunks = 4;
        this.MaxRegionRebuildsPerFrame = 1;
        this.MaxRegionUploadsPerFrame = 1;
        this.RegionScheduleBudgetMilliseconds = 1.0F;
        this.RegionUploadBudgetMilliseconds = 1.0F;
        this.MidRingLodUpdateIntervalTicks = 3;
        this.FarRingLodUpdateIntervalTicks = 10;
        this._meshChunkList = new List<IChunk>();
        this._pendingUpload = new ConcurrentQueue<IChunk>();
        this._queuedSet = new ConcurrentDictionary<IChunk, byte>();
        this._buildingSet = new ConcurrentDictionary<IChunk, byte>();
        this._chunksPendingVertexUpload = new HashSet<HeightmapChunk>();
        this._pendingRegionUploads = new ConcurrentQueue<RegionBuildResult>();
        this._regionBuildingSet = new ConcurrentDictionary<RegionKey, byte>();
        this._renderables = new Dictionary<IChunk, Renderable>();
        this._chunkLocalBounds = new Dictionary<IChunk, BoundingBox>();
        this._chunkLocalCenters = new Dictionary<IChunk, Vector3>();
        this._chunkWorldBounds = new Dictionary<IChunk, CachedWorldBounds>();
        this._chunkRenderableTransformVersions = new Dictionary<IChunk, int>();
        this._dirtyScheduleBuffer = new List<IChunk>();
        this._chunkRegions = new Dictionary<IChunk, RegionKey>();
        this._regionMembers = new Dictionary<RegionKey, HashSet<IChunk>>();
        this._regionBatches = new Dictionary<RegionKey, RegionBatch>();
        this._regionWorldBounds = new Dictionary<RegionKey, CachedWorldBounds>();
        this._regionRenderableTransformVersions = new Dictionary<RegionKey, int>();
        this._dirtyRegions = new HashSet<RegionKey>();
        this._regionDirtyBuffer = new List<RegionKey>();
        this._terrainBaseBox = new BoundingBox();
        this._terrainWorldBoundsVersion = -1;
        this._boundsTransformVersion = 0;
        this._hasBoundsTransformCache = false;
        this._renderTransformVersion = 0;
        this._hasRenderTransformCache = false;
        this._sharedRenderTransform = new Transform();
        this._lodUpdateAccumulator = float.PositiveInfinity;
        this._lodRingTick = 0;
    }
    
    protected internal override void Init() {
        base.Init();
        
        if (this.PreloadChunksOnInit) {
            ITerrain terrain = this._terrainFactory().GetAwaiter().GetResult();
            this.InitializeTerrain(terrain);
            this.PreloadAllChunkMeshes();
            return;
        }
        
        this._terrainTask = this._terrainFactory();
    }
    
    protected internal override void Update(double delta) {
        base.Update(delta);
        
        if (!this.EnsureTerrainReady()) {
            return;
        }

        this.RefreshBoundsTransformVersion();

        Camera3D? cam3D = SceneManager.ActiveCam3D;

        if (this.PauseTerrainWorkWhenOutOfView &&
            this.FrustumCulling &&
            cam3D != null &&
            !cam3D.GetFrustum().ContainsBox(this.GetTerrainWorldBounds())) {
            return;
        }
        
        this._lodUpdateAccumulator += (float) delta;
        this.UpdateLodLevels();
        this.ProcessDirtyRegions(false);
        
        int buildBudget = Math.Min(this.MaxChunkBuildsPerFrame, Math.Max(0, this.MaxConcurrentChunkBuilds - this._buildingSet.Count));

        if (buildBudget <= 0) {
            return;
        }
        
        int scheduled = 0;
        Vector3 camPos = cam3D?.Position ?? Vector3.Zero;

        this._dirtyScheduleBuffer.Clear();
        this._dirtyScheduleBuffer.AddRange(this.Terrain.GetDirtyChunks());

        for (int i = this._dirtyScheduleBuffer.Count - 1; i >= 0; i--) {
            if (!this.ShouldSchedule(this._dirtyScheduleBuffer[i])) {
                this._dirtyScheduleBuffer.RemoveAt(i);
            }
        }

        if (cam3D != null) {
            this._dirtyScheduleBuffer.Sort((a, b) => {
                int lodCompare = Math.Max(0, a.Lod).CompareTo(Math.Max(0, b.Lod));
                
                if (lodCompare != 0) {
                    return lodCompare;
                }
                
                return this.GetChunkDistanceSquared(camPos, a).CompareTo(this.GetChunkDistanceSquared(camPos, b));
            });
        }

        Stopwatch scheduleStopwatch = Stopwatch.StartNew();
        double scheduleBudgetMs = Math.Max(0.0F, this.ChunkBuildScheduleBudgetMilliseconds);

        foreach (IChunk chunk in this._dirtyScheduleBuffer) {
            if (scheduleBudgetMs > 0.0 && scheduled > 0 && scheduleStopwatch.Elapsed.TotalMilliseconds >= scheduleBudgetMs) {
                break;
            }

            this.ScheduleBackground(chunk);
            scheduled++;
            
            if (scheduled >= buildBudget) {
                break;
            }
        }
    }
    
    protected internal override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        base.Draw(context, framebuffer);
        
        if (!this.EnsureTerrainReady()) {
            return;
        }

        this.RefreshBoundsTransformVersion();
        this.RefreshRenderTransformVersion();
        
        this.ProcessPendingUploads(this.GraphicsDevice);
        this.ProcessPendingRegionUploads();
        
        Camera3D? cam3D = SceneManager.ActiveCam3D;
        
        if (cam3D != null && this.FrustumCulling) {
            bool visible = cam3D.GetFrustum().ContainsBox(this.GetTerrainWorldBounds());
            
            if (!visible) {
                return;
            }
        }
        
        this.TotalVertexCount = 0;
        this.TotalIndexCount = 0;

        this.DrawFarBatches(cam3D);
        
        foreach (IChunk chunk in this._meshChunkList) {
            if (chunk.Mesh == null) {
                continue;
            }
            
            if (chunk is HeightmapChunk heightmapChunk && this._chunksPendingVertexUpload.Contains(heightmapChunk)) {
                heightmapChunk.UpdateGeometry(context.CommandList);
                this._chunksPendingVertexUpload.Remove(heightmapChunk);
            }
            
            if (chunk.Lod < 0) {
                continue;
            }

            if (this._chunkRegions.TryGetValue(chunk, out RegionKey regionKey) &&
                this._regionBatches.TryGetValue(regionKey, out RegionBatch? regionBatch) &&
                regionBatch.Mesh != null) {
                continue;
            }
            
            if (cam3D != null && this.FrustumCulling && this._chunkLocalBounds.TryGetValue(chunk, out BoundingBox chunkBounds)) {
                bool chunkVisible = cam3D.GetFrustum().ContainsBox(this.GetChunkWorldBounds(chunk, chunkBounds));
                
                if (!chunkVisible) {
                    continue;
                }
            }
            
            if (!this._renderables.TryGetValue(chunk, out Renderable? renderable) || !ReferenceEquals(renderable.Mesh, chunk.Mesh)) {
                renderable?.Dispose();
                renderable = new Renderable(chunk.Mesh, new Transform(), this.Terrain.Material);
                this._renderables[chunk] = renderable;
                this._chunkRenderableTransformVersions.Remove(chunk);
            }
            
            this.ApplyChunkRenderableTransform(chunk, renderable);
            this.Entity.Scene.Renderer.DrawRenderable(renderable);
            this.TotalVertexCount += (int) chunk.Mesh.VertexCount;

            if (chunk.Mesh is Mesh<Vertex3D> mesh && mesh.MeshData is BasicMeshData meshData) {
                this.TotalIndexCount += meshData.Indices.Length;
            }
        }
    }
    
    public void DrawDebug(ImmediateRenderer immediateRenderer) {
        if (!this._initializedTerrain) {
            return;
        }

        this.RefreshBoundsTransformVersion();
        
        Transform boxTransform = new Transform {
            Translation = this.LerpedGlobalPosition,
            Rotation = this.LerpedRotation,
            Scale = this.LerpedScale
        };
        
        immediateRenderer.DrawBoundingBox(boxTransform, this._terrainBaseBox, Color.Green);
        
        Camera3D? cam3D = SceneManager.ActiveCam3D;
        
        foreach (IChunk chunk in this.Terrain.GetChunks()) {
            Color chunkColor;
            bool hasMeshColor;
            
            if (this._buildingSet.ContainsKey(chunk) || this._queuedSet.ContainsKey(chunk) || chunk.IsDirty) {
                chunkColor = Color.White;
                hasMeshColor = false;
            }
            else if (chunk.Mesh != null) {
                chunkColor = Color.LightGray;
                hasMeshColor = true;
            }
            else {
                chunkColor = Color.DarkGray;
                hasMeshColor = false;
            }
            
            if (cam3D != null && this.FrustumCulling && this._chunkLocalBounds.TryGetValue(chunk, out BoundingBox frustumBounds)) {
                bool chunkVisible = cam3D.GetFrustum().ContainsBox(this.GetChunkWorldBounds(chunk, frustumBounds));
                
                if (!chunkVisible && hasMeshColor) {
                    chunkColor = Color.Gray;
                }
            }
            
            BoundingBox chunkBox = new BoundingBox {
                Min = chunk.Position + new Vector3(0.0F, -chunk.Height, 0.0F),
                Max = chunk.Position + new Vector3(chunk.Width, chunk.Height, chunk.Depth)
            };
            
            immediateRenderer.DrawBoundingBox(boxTransform, chunkBox, chunkColor);
        }
    }
    
    private bool EnsureTerrainReady() {
        if (this._initializedTerrain) {
            return true;
        }
        
        if (this._terrainTask == null || !this._terrainTask.IsCompletedSuccessfully) {
            return false;
        }
        
        this.InitializeTerrain(this._terrainTask.Result);
        return true;
    }
    
    private void InitializeTerrain(ITerrain terrain) {
        this.Terrain = terrain;
        this._initializedTerrain = true;
        
        this._terrainBaseBox = new BoundingBox {
            Min = new Vector3(0.0F, -this.Terrain.Height, 0.0F),
            Max = new Vector3(this.Terrain.Width, this.Terrain.Height, this.Terrain.Depth)
        };
        
        this._chunkLocalBounds.Clear();
        this._chunkLocalCenters.Clear();
        this._chunkWorldBounds.Clear();
        this._terrainWorldBoundsVersion = -1;
        
        foreach (IChunk chunk in this.Terrain.GetChunks()) {
            this._chunkLocalBounds[chunk] = new BoundingBox {
                Min = chunk.Position + new Vector3(0.0F, -chunk.Height, 0.0F),
                Max = chunk.Position + new Vector3(chunk.Width, chunk.Height, chunk.Depth)
            };
            this._chunkLocalCenters[chunk] = chunk.Position + new Vector3(chunk.Width * 0.5F, chunk.Height * 0.5F, chunk.Depth * 0.5F);
        }

        this._lodUpdateAccumulator = float.PositiveInfinity;
        this._hasLastLodCameraPosition = false;
    }
    
    private void PreloadAllChunkMeshes() {
        Camera3D? camera = (Camera3D?) this.Entity.Scene.GetEntitiesWithTag("camera3D").FirstOrDefault();
        Vector3 cameraPosition = camera?.Position ?? Vector3.Zero;
        float maxLodDistance = this.UseCameraFarPlaneForLodRange && camera != null ? camera.FarPlane : float.PositiveInfinity;
        float maxLodDistanceSquared = maxLodDistance * maxLodDistance;
        
        foreach (IChunk chunk in this.Terrain.GetChunks()) {
            float distanceSquared = this.GetChunkDistanceSquared(cameraPosition, chunk);
            int targetLod = this.EnableLod ? DetermineLod(distanceSquared, this.LodDistances, this.CullChunksBeyondLastLod, maxLodDistanceSquared) : 0;
            
            if (chunk.Lod != targetLod) {
                chunk.Lod = targetLod;
            }
        }
        
        IChunk[] chunks = this.Terrain.GetChunks().Where(chunk => chunk.Lod >= 0).ToArray();
        
        Parallel.ForEach(chunks, new ParallelOptions {
            MaxDegreeOfParallelism = Math.Max(1, this.MaxConcurrentChunkBuilds)
        }, chunk => chunk.GenerateGeometry());
        
        foreach (IChunk chunk in chunks) {
            this.ApplyChunkGeometryUpload(chunk, this.GraphicsDevice);
            this.RefreshMeshChunkState(chunk);
        }

        this.UpdateFarChunkMembership();
        this.ProcessDirtyRegions(true);
    }
    
    private void ScheduleBackground(IChunk chunk) {
        if (!this._buildingSet.TryAdd(chunk, 0)) {
            return;
        }
        
        _ = Task.Run(() => {
            try {
                chunk.GenerateGeometry();
                if (this._queuedSet.TryAdd(chunk, 0)) {
                    this._pendingUpload.Enqueue(chunk);
                }
            }
            finally {
                this._buildingSet.TryRemove(chunk, out _);
            }
        });
    }
    
    private bool ShouldSchedule(IChunk chunk) {
        if (this._queuedSet.ContainsKey(chunk) || this._buildingSet.ContainsKey(chunk)) {
            return false;
        }
        
        if (chunk.Lod < 0 && chunk.Mesh == null) {
            return false;
        }
        
        return true;
    }
    
    private void UpdateLodLevels() {
        Camera3D? cam3D = SceneManager.ActiveCam3D;
        
        if (cam3D == null) {
            return;
        }

        float interval = Math.Max(0.0F, this.LodUpdateIntervalSeconds);
        float moveThreshold = Math.Max(0.0F, this.LodUpdateCameraMoveThreshold);
        bool movedEnough = !this._hasLastLodCameraPosition ||
                           Vector3.DistanceSquared(this._lastLodCameraPosition, cam3D.Position) >= (moveThreshold * moveThreshold);

        if (!movedEnough && this._lodUpdateAccumulator < interval) {
            return;
        }

        this._lodUpdateAccumulator = 0.0F;
        this._hasLastLodCameraPosition = true;
        this._lastLodCameraPosition = cam3D.Position;
        this._lodRingTick++;
        
        float maxLodDistance = this.UseCameraFarPlaneForLodRange ? cam3D.FarPlane : float.PositiveInfinity;
        float maxLodDistanceSquared = maxLodDistance * maxLodDistance;

        float nearRingDistance = this.LodDistances.Length == 0 ? float.PositiveInfinity : this.LodDistances[Math.Min(1, this.LodDistances.Length - 1)];
        float midRingDistance = this.LodDistances.Length == 0 ? float.PositiveInfinity : this.LodDistances[Math.Min(2, this.LodDistances.Length - 1)];
        float nearRingDistanceSquared = nearRingDistance * nearRingDistance;
        float midRingDistanceSquared = midRingDistance * midRingDistance;
        int midInterval = Math.Max(1, this.MidRingLodUpdateIntervalTicks);
        int farInterval = Math.Max(1, this.FarRingLodUpdateIntervalTicks);
        
        foreach (IChunk chunk in this.Terrain.GetChunks()) {
            float distanceSquared = this.GetChunkDistanceSquared(cam3D.Position, chunk);

            bool updateThisChunk = distanceSquared <= nearRingDistanceSquared ||
                                   (distanceSquared <= midRingDistanceSquared && (this._lodRingTick % midInterval == 0)) ||
                                   (distanceSquared > midRingDistanceSquared && (this._lodRingTick % farInterval == 0));

            if (!updateThisChunk) {
                continue;
            }

            int targetLod = this.EnableLod ? DetermineLod(distanceSquared, this.LodDistances, this.CullChunksBeyondLastLod, maxLodDistanceSquared) : 0;
            
            if (this.ShouldHoldCurrentLod(chunk.Lod, targetLod, distanceSquared)) {
                continue;
            }
            
            if (chunk.Lod != targetLod) {
                chunk.Lod = targetLod;
                this.UpdateChunkRegionMembership(chunk);
            }
        }
    }
    
    private static int DetermineLod(float distanceSquared, float[] lodDistances, bool cullBeyondLastLod, float maxDistanceSquared) {
        if (distanceSquared > maxDistanceSquared) {
            return -1;
        }
        
        if (lodDistances.Length == 0) {
            return 0;
        }
        
        for (int i = 0; i < lodDistances.Length; i++) {
            float lodDistance = lodDistances[i];

            if (distanceSquared <= lodDistance * lodDistance) {
                return i;
            }
        }
        
        return cullBeyondLastLod ? -1 : lodDistances.Length - 1;
    }
    
    private bool ShouldHoldCurrentLod(int currentLod, int targetLod, float distanceSquared) {
        if (currentLod < 0 || targetLod == currentLod || this.LodDistances.Length == 0 || this.LodHysteresis <= 0.0F) {
            return false;
        }
        
        if (targetLod > currentLod) {
            if (currentLod >= this.LodDistances.Length) {
                return false;
            }
            
            float threshold = this.LodDistances[currentLod] * (1.0F + this.LodHysteresis);
            return distanceSquared <= threshold * threshold;
        }
        
        if (targetLod < 0 || targetLod >= this.LodDistances.Length) {
            return false;
        }
        
        float lowerThreshold = this.LodDistances[targetLod] * (1.0F - this.LodHysteresis);
        return distanceSquared >= lowerThreshold * lowerThreshold;
    }
    
    private float GetChunkDistanceSquared(Vector3 cameraPosition, IChunk chunk) {
        if (!this._chunkLocalCenters.TryGetValue(chunk, out Vector3 chunkCenter)) {
            chunkCenter = chunk.Position + new Vector3(chunk.Width * 0.5F, chunk.Height * 0.5F, chunk.Depth * 0.5F);
            this._chunkLocalCenters[chunk] = chunkCenter;
        }

        Vector3 chunkWorldCenter = this.LerpedGlobalPosition + chunkCenter;
        return Vector3.DistanceSquared(cameraPosition, chunkWorldCenter);
    }
    
    private void ProcessPendingUploads(GraphicsDevice graphicsDevice) {
        int uploads = 0;
        
        while (uploads < this.MaxChunkUploadsPerFrame && this._pendingUpload.TryDequeue(out IChunk? chunk)) {
            this._queuedSet.TryRemove(chunk, out _);
            this.ApplyChunkGeometryUpload(chunk, graphicsDevice);
            this.RefreshMeshChunkState(chunk);
            uploads++;
        }
    }

    private void ProcessPendingRegionUploads() {
        int uploads = 0;
        int uploadBudget = Math.Max(0, this.MaxRegionUploadsPerFrame);

        if (uploadBudget == 0) {
            return;
        }

        Stopwatch uploadStopwatch = Stopwatch.StartNew();
        double uploadBudgetMs = Math.Max(0.0F, this.RegionUploadBudgetMilliseconds);

        while (uploads < uploadBudget && this._pendingRegionUploads.TryDequeue(out RegionBuildResult result)) {
            if (uploadBudgetMs > 0.0 && uploads > 0 && uploadStopwatch.Elapsed.TotalMilliseconds >= uploadBudgetMs) {
                this._pendingRegionUploads.Enqueue(result);
                break;
            }

            this.DisposeRegionBatch(result.Key);

            if (result.HasGeometry) {
                Mesh<Vertex3D> regionMesh = new Mesh<Vertex3D>(this.GraphicsDevice, this.Terrain.Material, new BasicMeshData(result.Vertices!, result.Indices!));
                RegionBatch batch = new RegionBatch(regionMesh, new Renderable(regionMesh, new Transform(), this.Terrain.Material), result.LocalBounds, result.VertexCount, result.IndexCount);
                this._regionBatches[result.Key] = batch;
            }

            this._regionWorldBounds.Remove(result.Key);
            this._regionRenderableTransformVersions.Remove(result.Key);
            uploads++;
        }
    }
    
    private void RefreshMeshChunkState(IChunk chunk) {
        if (chunk is HeightmapChunk heightmapChunk && chunk.Mesh == null) {
            this._chunksPendingVertexUpload.Remove(heightmapChunk);
        }

        if (chunk.Mesh != null) {
            if (!this._meshChunkList.Contains(chunk)) {
                this._meshChunkList.Add(chunk);
            }

            this.UpdateChunkRegionMembership(chunk);
            return;
        }
        
        this._meshChunkList.Remove(chunk);
        this.RemoveChunkFromRegion(chunk);
        this._chunkWorldBounds.Remove(chunk);
        this._chunkRenderableTransformVersions.Remove(chunk);
        
        if (this._renderables.Remove(chunk, out Renderable? renderable)) {
            renderable.Dispose();
        }
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            foreach (Renderable renderable in this._renderables.Values) {
                renderable.Dispose();
            }
            
            this._renderables.Clear();
            
            foreach (RegionBatch batch in this._regionBatches.Values) {
                batch.Dispose();
            }
            
            this._regionBatches.Clear();
            
            if (this._initializedTerrain) {
                foreach (IChunk chunk in this.Terrain.GetChunks()) {
                    chunk.Dispose();
                }
            }
            
            this._meshChunkList.Clear();
            this._buildingSet.Clear();
            this._queuedSet.Clear();
            this._regionBuildingSet.Clear();
            this._chunkLocalBounds.Clear();
            this._chunkLocalCenters.Clear();
            this._chunkWorldBounds.Clear();
            this._chunkRenderableTransformVersions.Clear();
            this._chunkRegions.Clear();
            this._regionMembers.Clear();
            this._regionWorldBounds.Clear();
            this._regionRenderableTransformVersions.Clear();
            this._dirtyRegions.Clear();
            this._chunksPendingVertexUpload.Clear();
            
            while (this._pendingUpload.TryDequeue(out _)) {
            }

            while (this._pendingRegionUploads.TryDequeue(out _)) {
            }
        }
    }

    private void ApplyChunkGeometryUpload(IChunk chunk, GraphicsDevice graphicsDevice) {
        if (chunk is not HeightmapChunk heightmapChunk) {
            chunk.UploadGeometry(graphicsDevice);
            return;
        }

        bool hasGeometry = heightmapChunk.PendingVertexCount > 0 && heightmapChunk.PendingIndexCount > 0;

        if (!hasGeometry) {
            chunk.UploadGeometry(graphicsDevice);
            this._chunksPendingVertexUpload.Remove(heightmapChunk);
            return;
        }

        bool canUpdateInPlace = heightmapChunk.Mesh is Mesh<Vertex3D> existingMesh &&
                                existingMesh.MeshData is BasicMeshData meshData &&
                                meshData.Vertices.Length == heightmapChunk.PendingVertexCount &&
                                meshData.Indices.Length == heightmapChunk.PendingIndexCount;

        if (canUpdateInPlace) {
            this._chunksPendingVertexUpload.Add(heightmapChunk);
        }
        else {
            chunk.UploadGeometry(graphicsDevice);
            this._chunksPendingVertexUpload.Remove(heightmapChunk);
        }
    }

    private void DrawFarBatches(Camera3D? cam3D) {
        foreach (KeyValuePair<RegionKey, RegionBatch> pair in this._regionBatches) {
            RegionKey regionKey = pair.Key;
            RegionBatch batch = pair.Value;

            if (batch.Mesh == null || batch.Renderable == null) {
                continue;
            }

            if (cam3D != null && this.FrustumCulling) {
                bool visible = cam3D.GetFrustum().ContainsBox(this.GetRegionWorldBounds(regionKey, batch.LocalBounds));

                if (!visible) {
                    continue;
                }
            }
            
            this.ApplyRegionRenderableTransform(regionKey, batch.Renderable);
            this.Entity.Scene.Renderer.DrawRenderable(batch.Renderable);
            this.TotalVertexCount += batch.VertexCount;
            this.TotalIndexCount += batch.IndexCount;
        }
    }

    private void RefreshBoundsTransformVersion() {
        if (!this._hasBoundsTransformCache ||
            this._boundsTransformPosition != this.LerpedGlobalPosition ||
            this._boundsTransformRotation != this.LerpedRotation ||
            this._boundsTransformScale != this.LerpedScale) {
            this._hasBoundsTransformCache = true;
            this._boundsTransformPosition = this.LerpedGlobalPosition;
            this._boundsTransformRotation = this.LerpedRotation;
            this._boundsTransformScale = this.LerpedScale;
            this._boundsTransformVersion++;
        }
    }

    private void RefreshRenderTransformVersion() {
        if (!this._hasRenderTransformCache ||
            this._renderTransformPosition != this.LerpedGlobalPosition ||
            this._renderTransformRotation != this.LerpedRotation ||
            this._renderTransformScale != this.LerpedScale) {
            this._hasRenderTransformCache = true;
            this._renderTransformPosition = this.LerpedGlobalPosition;
            this._renderTransformRotation = this.LerpedRotation;
            this._renderTransformScale = this.LerpedScale;
            this._renderTransformVersion++;

            this._sharedRenderTransform.Translation = this.LerpedGlobalPosition;
            this._sharedRenderTransform.Rotation = this.LerpedRotation;
            this._sharedRenderTransform.Scale = this.LerpedScale;
        }
    }

    private void ApplyChunkRenderableTransform(IChunk chunk, Renderable renderable) {
        if (this._chunkRenderableTransformVersions.TryGetValue(chunk, out int version) && version == this._renderTransformVersion) {
            return;
        }

        renderable.SetTransform(0, this._sharedRenderTransform);
        this._chunkRenderableTransformVersions[chunk] = this._renderTransformVersion;
    }

    private void ApplyRegionRenderableTransform(RegionKey key, Renderable renderable) {
        if (this._regionRenderableTransformVersions.TryGetValue(key, out int version) && version == this._renderTransformVersion) {
            return;
        }

        renderable.SetTransform(0, this._sharedRenderTransform);
        this._regionRenderableTransformVersions[key] = this._renderTransformVersion;
    }

    private BoundingBox GetTerrainWorldBounds() {
        if (this._terrainWorldBoundsVersion == this._boundsTransformVersion) {
            return this._terrainWorldBounds;
        }

        this._terrainWorldBounds = this.ComputeWorldAabb(this._terrainBaseBox);
        this._terrainWorldBoundsVersion = this._boundsTransformVersion;
        return this._terrainWorldBounds;
    }

    private BoundingBox GetChunkWorldBounds(IChunk chunk, BoundingBox localBounds) {
        if (this._chunkWorldBounds.TryGetValue(chunk, out CachedWorldBounds cached) &&
            cached.Version == this._boundsTransformVersion) {
            return cached.Bounds;
        }

        BoundingBox worldBounds = this.ComputeWorldAabb(localBounds);
        this._chunkWorldBounds[chunk] = new CachedWorldBounds(worldBounds, this._boundsTransformVersion);
        return worldBounds;
    }

    private BoundingBox GetRegionWorldBounds(RegionKey key, BoundingBox localBounds) {
        if (this._regionWorldBounds.TryGetValue(key, out CachedWorldBounds cached) &&
            cached.Version == this._boundsTransformVersion) {
            return cached.Bounds;
        }

        BoundingBox worldBounds = this.ComputeWorldAabb(localBounds);
        this._regionWorldBounds[key] = new CachedWorldBounds(worldBounds, this._boundsTransformVersion);
        return worldBounds;
    }

    private BoundingBox ComputeWorldAabb(BoundingBox localBox) {
        Vector3 worldMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 worldMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        this.ExpandWorldBounds(localBox.Min.X, localBox.Min.Y, localBox.Min.Z, ref worldMin, ref worldMax);
        this.ExpandWorldBounds(localBox.Max.X, localBox.Min.Y, localBox.Min.Z, ref worldMin, ref worldMax);
        this.ExpandWorldBounds(localBox.Min.X, localBox.Max.Y, localBox.Min.Z, ref worldMin, ref worldMax);
        this.ExpandWorldBounds(localBox.Max.X, localBox.Max.Y, localBox.Min.Z, ref worldMin, ref worldMax);
        this.ExpandWorldBounds(localBox.Min.X, localBox.Min.Y, localBox.Max.Z, ref worldMin, ref worldMax);
        this.ExpandWorldBounds(localBox.Max.X, localBox.Min.Y, localBox.Max.Z, ref worldMin, ref worldMax);
        this.ExpandWorldBounds(localBox.Min.X, localBox.Max.Y, localBox.Max.Z, ref worldMin, ref worldMax);
        this.ExpandWorldBounds(localBox.Max.X, localBox.Max.Y, localBox.Max.Z, ref worldMin, ref worldMax);

        return new BoundingBox {
            Min = worldMin,
            Max = worldMax
        };
    }

    private void ExpandWorldBounds(float x, float y, float z, ref Vector3 worldMin, ref Vector3 worldMax) {
        Vector3 local = new Vector3(x, y, z);
        Vector3 scaled = local * this.LerpedScale;
        Vector3 rotated = Vector3.Transform(scaled, this.LerpedRotation);
        Vector3 world = rotated + this.LerpedGlobalPosition;
        worldMin = Vector3.Min(worldMin, world);
        worldMax = Vector3.Max(worldMax, world);
    }

    private void UpdateFarChunkMembership() {
        if (!this.EnableFarChunkBatching || this.FarChunkBatchRegionSizeInChunks <= 0) {
            this.ClearAllChunkRegions();
            return;
        }

        foreach (IChunk chunk in this.Terrain.GetChunks()) {
            this.UpdateChunkRegionMembership(chunk);
        }
    }

    private void UpdateChunkRegionMembership(IChunk chunk) {
        bool shouldBatch = this.ShouldBatchChunk(chunk);

        if (!shouldBatch) {
            this.RemoveChunkFromRegion(chunk);
            return;
        }

        RegionKey newKey = this.GetRegionKey(chunk);

        if (this._chunkRegions.TryGetValue(chunk, out RegionKey currentKey) && currentKey.Equals(newKey)) {
            return;
        }

        this.RemoveChunkFromRegion(chunk);
        this._chunkRegions[chunk] = newKey;

        if (!this._regionMembers.TryGetValue(newKey, out HashSet<IChunk>? members)) {
            members = new HashSet<IChunk>();
            this._regionMembers[newKey] = members;
        }

        members.Add(chunk);
        this._dirtyRegions.Add(newKey);
    }

    private void RemoveChunkFromRegion(IChunk chunk) {
        if (!this._chunkRegions.Remove(chunk, out RegionKey key)) {
            return;
        }

        if (this._regionMembers.TryGetValue(key, out HashSet<IChunk>? members)) {
            members.Remove(chunk);

            if (members.Count == 0) {
                this._regionMembers.Remove(key);
            }
        }

        this._dirtyRegions.Add(key);
    }

    private void ClearAllChunkRegions() {
        foreach (RegionBatch batch in this._regionBatches.Values) {
            batch.Dispose();
        }

        this._chunkRegions.Clear();
        this._regionMembers.Clear();
        this._regionBatches.Clear();
        this._regionWorldBounds.Clear();
        this._regionRenderableTransformVersions.Clear();
        this._dirtyRegions.Clear();
    }

    private bool ShouldBatchChunk(IChunk chunk) {
        if (chunk.Mesh == null || chunk.Lod < 0 || chunk.CurrentLod < 0) {
            return false;
        }

        if (chunk.Lod < this.FarChunkBatchMinLod || chunk.CurrentLod < this.FarChunkBatchMinLod) {
            return false;
        }

        return true;
    }

    private RegionKey GetRegionKey(IChunk chunk) {
        int chunkSize = Math.Max(1, this.Terrain.ChunkSize);
        int regionChunkSize = Math.Max(1, this.FarChunkBatchRegionSizeInChunks);
        int chunkX = Math.Max(0, (int) chunk.Position.X / chunkSize);
        int chunkZ = Math.Max(0, (int) chunk.Position.Z / chunkSize);

        return new RegionKey(chunkX / regionChunkSize, chunkZ / regionChunkSize, chunk.CurrentLod);
    }

    private void ProcessDirtyRegions(bool forceAll) {
        if (this._dirtyRegions.Count == 0) {
            return;
        }

        this._regionDirtyBuffer.Clear();
        this._regionDirtyBuffer.AddRange(this._dirtyRegions);

        Camera3D? cam3D = SceneManager.ActiveCam3D;

        if (cam3D != null) {
            this._regionDirtyBuffer.Sort((a, b) => {
                float da = this.GetRegionDistanceSquared(cam3D.Position, a);
                float db = this.GetRegionDistanceSquared(cam3D.Position, b);
                return da.CompareTo(db);
            });
        }

        int budget = forceAll ? int.MaxValue : Math.Max(0, this.MaxRegionRebuildsPerFrame);
        int scheduled = 0;
        Stopwatch scheduleStopwatch = Stopwatch.StartNew();
        double scheduleBudgetMs = forceAll ? double.PositiveInfinity : Math.Max(0.0F, this.RegionScheduleBudgetMilliseconds);

        foreach (RegionKey key in this._regionDirtyBuffer) {
            if (scheduled >= budget) {
                break;
            }

            if (scheduleBudgetMs > 0.0 && scheduled > 0 && scheduleStopwatch.Elapsed.TotalMilliseconds >= scheduleBudgetMs) {
                break;
            }

            if (this.ScheduleRegionBuild(key)) {
                this._dirtyRegions.Remove(key);
                scheduled++;
            }
        }
    }

    private float GetRegionDistanceSquared(Vector3 cameraPosition, RegionKey key) {
        int chunkSize = Math.Max(1, this.Terrain.ChunkSize);
        int regionChunkSize = Math.Max(1, this.FarChunkBatchRegionSizeInChunks);
        float regionWorldSize = chunkSize * regionChunkSize;
        float centerX = ((key.X + 0.5F) * regionWorldSize);
        float centerZ = ((key.Z + 0.5F) * regionWorldSize);
        float centerY = this.Terrain.Height * 0.5F;
        Vector3 worldCenter = this.LerpedGlobalPosition + new Vector3(centerX, centerY, centerZ);
        return Vector3.DistanceSquared(cameraPosition, worldCenter);
    }

    private bool ScheduleRegionBuild(RegionKey key) {
        if (!this._regionBuildingSet.TryAdd(key, 0)) {
            return false;
        }

        IChunk[] memberSnapshot = this._regionMembers.TryGetValue(key, out HashSet<IChunk>? members) ? members.ToArray() : [];

        _ = Task.Run(() => {
            try {
                RegionBuildResult result = BuildRegionGeometry(key, memberSnapshot);
                this._pendingRegionUploads.Enqueue(result);
            }
            finally {
                this._regionBuildingSet.TryRemove(key, out _);
            }
        });

        return true;
    }

    private RegionBuildResult BuildRegionGeometry(RegionKey key, IChunk[] members) {
        int totalVertexCount = 0;
        int totalIndexCount = 0;
        bool hasBounds = false;
        BoundingBox bounds = default;
        List<BasicMeshData> sourceData = new List<BasicMeshData>(members.Length);

        foreach (IChunk chunk in members) {
            if (chunk.Mesh is not Mesh<Vertex3D> mesh || mesh.MeshData is not BasicMeshData meshData || meshData.Vertices.Length == 0 || meshData.Indices.Length == 0) {
                continue;
            }

            sourceData.Add(meshData);
            totalVertexCount += meshData.Vertices.Length;
            totalIndexCount += meshData.Indices.Length;

            if (this._chunkLocalBounds.TryGetValue(chunk, out BoundingBox chunkBounds)) {
                if (!hasBounds) {
                    bounds = chunkBounds;
                    hasBounds = true;
                }
                else {
                    bounds = new BoundingBox {
                        Min = Vector3.Min(bounds.Min, chunkBounds.Min),
                        Max = Vector3.Max(bounds.Max, chunkBounds.Max)
                    };
                }
            }
        }

        if (totalVertexCount == 0 || totalIndexCount == 0 || !hasBounds) {
            return RegionBuildResult.Empty(key);
        }

        Vertex3D[] mergedVertices = new Vertex3D[totalVertexCount];
        uint[] mergedIndices = new uint[totalIndexCount];

        int vertexOffset = 0;
        int indexOffset = 0;

        foreach (BasicMeshData meshData in sourceData) {
            Array.Copy(meshData.Vertices, 0, mergedVertices, vertexOffset, meshData.Vertices.Length);

            for (int i = 0; i < meshData.Indices.Length; i++) {
                mergedIndices[indexOffset + i] = meshData.Indices[i] + (uint) vertexOffset;
            }

            vertexOffset += meshData.Vertices.Length;
            indexOffset += meshData.Indices.Length;
        }

        return RegionBuildResult.Create(key, mergedVertices, mergedIndices, bounds, totalVertexCount, totalIndexCount);
    }

    private void DisposeRegionBatch(RegionKey key) {
        if (!this._regionBatches.Remove(key, out RegionBatch? batch)) {
            return;
        }
        
        this._regionWorldBounds.Remove(key);
        this._regionRenderableTransformVersions.Remove(key);
        batch.Dispose();
    }

    private readonly struct RegionBuildResult {
        
        public readonly RegionKey Key;
        
        public readonly Vertex3D[]? Vertices;
        
        public readonly uint[]? Indices;
        
        public readonly BoundingBox LocalBounds;
        
        public readonly int VertexCount;
        
        public readonly int IndexCount;
        
        public readonly bool HasGeometry;
        
        private RegionBuildResult(RegionKey key, Vertex3D[]? vertices, uint[]? indices, BoundingBox localBounds, int vertexCount, int indexCount, bool hasGeometry) {
            this.Key = key;
            this.Vertices = vertices;
            this.Indices = indices;
            this.LocalBounds = localBounds;
            this.VertexCount = vertexCount;
            this.IndexCount = indexCount;
            this.HasGeometry = hasGeometry;
        }
        
        public static RegionBuildResult Create(RegionKey key, Vertex3D[] vertices, uint[] indices, BoundingBox localBounds, int vertexCount, int indexCount) {
            return new RegionBuildResult(key, vertices, indices, localBounds, vertexCount, indexCount, true);
        }
        
        public static RegionBuildResult Empty(RegionKey key) {
            return new RegionBuildResult(key, null, null, default, 0, 0, false);
        }
    }

    private readonly struct RegionKey : IEquatable<RegionKey> {
        
        public readonly int X;
        
        public readonly int Z;
        
        public readonly int Lod;
        
        public RegionKey(int x, int z, int lod) {
            this.X = x;
            this.Z = z;
            this.Lod = lod;
        }
        
        public static bool operator ==(RegionKey left, RegionKey right) => left.Equals(right);
        
        public static bool operator !=(RegionKey left, RegionKey right) => !left.Equals(right);
        
        public bool Equals(RegionKey other) {
            return this.X.Equals(other.X) && this.Z.Equals(other.Z) && this.Lod.Equals(other.Lod);
        }
        
        public override bool Equals(object? obj) {
            return obj is RegionKey other && this.Equals(other);
        }
        
        public override int GetHashCode() {
            return HashCode.Combine(this.X.GetHashCode(), this.Z.GetHashCode(), this.Lod.GetHashCode());
        }
    }
    
    private readonly struct CachedWorldBounds {
        
        public readonly BoundingBox Bounds;
        
        public readonly int Version;
        
        public CachedWorldBounds(BoundingBox bounds, int version) {
            this.Bounds = bounds;
            this.Version = version;
        }
    }
    
    private class RegionBatch : Disposable {
        
        public IMesh? Mesh { get; private set; }
        
        public Renderable? Renderable { get; private set; }
        
        public BoundingBox LocalBounds { get; private set; }
        
        public int VertexCount { get; private set; }
        
        public int IndexCount { get; private set; }
        
        public RegionBatch(IMesh mesh, Renderable renderable, BoundingBox localBounds, int vertexCount, int indexCount) {
            this.Mesh = mesh;
            this.Renderable = renderable;
            this.LocalBounds = localBounds;
            this.VertexCount = vertexCount;
            this.IndexCount = indexCount;
        }
        
        protected override void Dispose(bool disposing) {
            if (disposing) {
                this.Renderable?.Dispose();
                this.Renderable = null;
                this.Mesh?.Dispose();
                this.Mesh = null;
            }
        }
    }
}
