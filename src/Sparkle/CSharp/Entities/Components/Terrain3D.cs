using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Geometry;
using Bliss.CSharp.Geometry.Meshes;
using Bliss.CSharp.Geometry.Meshes.Data;
using Bliss.CSharp.Graphics.Rendering.Renderers;
using Bliss.CSharp.Graphics.Rendering.Renderers.Forward;
using Bliss.CSharp.Graphics.VertexTypes;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.Graphics.Rendering.Gizmos;
using Sparkle.CSharp.Scenes;
using Sparkle.CSharp.Terrain;
using Sparkle.CSharp.Terrain.Regions;
using Veldrid;

namespace Sparkle.CSharp.Entities.Components;

public class Terrain3D : InterpolatedComponent, IDebugDrawable {
    
    public ITerrain Terrain { get; private set; }
    
    public TerrainSettings TerrainSettings { get; private set; }
    
    public bool DebugDrawEnabled { get; set; }
    
    public bool FrustumCulling;
    
    public int TotalVertexCount { get; private set; }
    
    public int TotalIndexCount { get; private set; }
    
    private readonly Func<Task<ITerrain>> _terrainFactory;
    
    private readonly List<IChunk> _meshChunkList;
    
    private readonly ConcurrentQueue<IChunk> _pendingUpload;
    
    private readonly ConcurrentDictionary<IChunk, byte> _queuedSet;
    
    private readonly ConcurrentDictionary<IChunk, byte> _buildingSet;
    
    private readonly HashSet<IChunk> _chunksPendingVertexUpload;
    
    private readonly ConcurrentQueue<TerrainRegionBuildResult> _pendingRegionUploads;
    
    private readonly ConcurrentDictionary<TerrainRegionKey, byte> _regionBuildingSet;
    
    private readonly Dictionary<IChunk, Renderable> _renderables;
    
    private readonly Dictionary<IChunk, BoundingBox> _chunkLocalBounds;
    
    private readonly Dictionary<IChunk, Vector3> _chunkLocalCenters;
    
    private readonly Dictionary<IChunk, CachedWorldBounds> _chunkWorldBounds;
    
    private readonly List<IChunk> _dirtyScheduleBuffer;
    
    private readonly Dictionary<IChunk, TerrainRegionKey> _chunkRegions;
    
    private readonly Dictionary<TerrainRegionKey, HashSet<IChunk>> _regionMembers;
    
    private readonly Dictionary<TerrainRegionKey, TerrainRegionBatch> _regionBatches;
    
    private readonly Dictionary<TerrainRegionKey, CachedWorldBounds> _regionWorldBounds;
    
    private readonly HashSet<TerrainRegionKey> _dirtyRegions;
    
    private readonly List<TerrainRegionKey> _regionDirtyBuffer;
    
    private BoundingBox _terrainLocalBounds;
    
    private BoundingBox _cachedTerrainWorldBounds;
    
    private int _cachedTerrainWorldBoundsVersion;
    
    private int _boundsCacheTransformVersion;
    
    private bool _hasCachedBoundsTransform;
    
    private Vector3 _cachedBoundsPosition;
    
    private Quaternion _cachedBoundsRotation;
    
    private Vector3 _cachedBoundsScale;
    
    private bool _hasCachedRenderTransform;
    
    private Vector3 _cachedRenderPosition;
    
    private Quaternion _cachedRenderRotation;
    
    private Vector3 _cachedRenderScale;
    
    private Transform _terrainRenderTransform;
    
    private float _lodUpdateAccumulator;
    
    private int _lodRingTick;
    
    private bool _hasLastLodCameraPosition;
    
    private Vector3 _lastLodCameraPosition;
    
    public Terrain3D(Func<Task<ITerrain>> terrainFactory, TerrainSettings terrainTerrainSettings, Vector3 offsetPosition, bool frustumCulling = true) : base(offsetPosition) {
        this._terrainFactory = terrainFactory;
        this.TerrainSettings = terrainTerrainSettings;
        this.FrustumCulling = frustumCulling;
        this._meshChunkList = new List<IChunk>();
        this._pendingUpload = new ConcurrentQueue<IChunk>();
        this._queuedSet = new ConcurrentDictionary<IChunk, byte>();
        this._buildingSet = new ConcurrentDictionary<IChunk, byte>();
        this._chunksPendingVertexUpload = new HashSet<IChunk>();
        this._pendingRegionUploads = new ConcurrentQueue<TerrainRegionBuildResult>();
        this._regionBuildingSet = new ConcurrentDictionary<TerrainRegionKey, byte>();
        this._renderables = new Dictionary<IChunk, Renderable>();
        this._chunkLocalBounds = new Dictionary<IChunk, BoundingBox>();
        this._chunkLocalCenters = new Dictionary<IChunk, Vector3>();
        this._chunkWorldBounds = new Dictionary<IChunk, CachedWorldBounds>();
        this._dirtyScheduleBuffer = new List<IChunk>();
        this._chunkRegions = new Dictionary<IChunk, TerrainRegionKey>();
        this._regionMembers = new Dictionary<TerrainRegionKey, HashSet<IChunk>>();
        this._regionBatches = new Dictionary<TerrainRegionKey, TerrainRegionBatch>();
        this._regionWorldBounds = new Dictionary<TerrainRegionKey, CachedWorldBounds>();
        this._dirtyRegions = new HashSet<TerrainRegionKey>();
        this._regionDirtyBuffer = new List<TerrainRegionKey>();
        this._terrainLocalBounds = new BoundingBox();
        this._cachedTerrainWorldBoundsVersion = -1;
        this._boundsCacheTransformVersion = 0;
        this._hasCachedBoundsTransform = false;
        this._hasCachedRenderTransform = false;
        this._terrainRenderTransform = new Transform();
        this._lodUpdateAccumulator = float.PositiveInfinity;
        this._lodRingTick = 0;
    }
    
    protected internal override void Init() {
        base.Init();
        
        // Generate terrain.
        this.Terrain = this._terrainFactory().GetAwaiter().GetResult();
        
        // Create terrain base box.
        this._terrainLocalBounds = new BoundingBox {
            Min = new Vector3(0.0F, -this.Terrain.Height, 0.0F),
            Max = new Vector3(this.Terrain.Width, this.Terrain.Height, this.Terrain.Depth)
        };
        
        // Create chunk local boxes.
        foreach (IChunk chunk in this.Terrain.GetChunks()) {
            this._chunkLocalBounds[chunk] = new BoundingBox {
                Min = chunk.Position + new Vector3(0.0F, -chunk.Height, 0.0F),
                Max = chunk.Position + new Vector3(chunk.Width, chunk.Height, chunk.Depth)
            };
            
            this._chunkLocalCenters[chunk] = chunk.Position + new Vector3(chunk.Width * 0.5F, chunk.Height * 0.5F, chunk.Depth * 0.5F);
        }
        
        // Preload all chunk meshes.
        this.PreloadAllChunkMeshes();
    }
    
    protected internal override void Update(double delta) {
        base.Update(delta);
        
        Camera3D? cam3D = SceneManager.ActiveCam3D;
        
        if (cam3D == null) {
            return;
        }
        
        // Refresh bounds transform version.
        this.RefreshBoundsTransformVersion();
        
        // Pause terrain work when the terrain is outside the camera view.
        if (this.TerrainSettings.PauseTerrainWorkWhenOutOfView && this.FrustumCulling && !cam3D.GetFrustum().ContainsBox(this.GetTerrainWorldBounds())) {
            return;
        }
        
        // Update terrain LOD levels.
        this.UpdateLodLevels(delta);
        
        // Process dirty far terrain regions.
        this.ProcessDirtyRegions(cam3D, false);
        
        // Calculate how many chunk builds can be scheduled this frame.
        int buildBudget = Math.Min(this.TerrainSettings.MaxChunkBuildsPerFrame, Math.Max(0, this.TerrainSettings.MaxConcurrentChunkBuilds - this._buildingSet.Count));
        
        if (buildBudget <= 0) {
            return;
        }
        
        // Fill the schedule buffer with dirty chunks.
        this._dirtyScheduleBuffer.Clear();
        this._dirtyScheduleBuffer.AddRange(this.Terrain.GetDirtyChunks());
        
        // Remove chunks that are already queued, building, or should not be scheduled.
        for (int i = this._dirtyScheduleBuffer.Count - 1; i >= 0; i--) {
            if (!this.ShouldSchedule(this._dirtyScheduleBuffer[i])) {
                this._dirtyScheduleBuffer.RemoveAt(i);
            }
        }
        
        // Sort chunks by LOD first, then by camera distance.
        this._dirtyScheduleBuffer.Sort((chunkA, chunkB) => {
            int lodCompare = Math.Max(0, chunkA.Lod).CompareTo(Math.Max(0, chunkB.Lod));
            
            if (lodCompare != 0) {
                return lodCompare;
            }
            
            return this.GetChunkDistanceSquared(cam3D.Position, chunkA).CompareTo(this.GetChunkDistanceSquared(cam3D.Position, chunkB));
        });
        
        // Start measuring how long scheduling takes this frame.
        Stopwatch scheduleStopwatch = Stopwatch.StartNew();
        double scheduleBudgetMs = Math.Max(0.0F, this.TerrainSettings.ChunkBuildScheduleBudgetMilliseconds);
        int scheduled = 0;
        
        // Schedule chunk builds until the count or time budget is reached.
        foreach (IChunk chunk in this._dirtyScheduleBuffer) {
            
            // Stop if the scheduling time budget has been used.
            if (scheduleBudgetMs > 0.0 && scheduled > 0 && scheduleStopwatch.Elapsed.TotalMilliseconds >= scheduleBudgetMs) {
                break;
            }
            
            // Schedule the chunk for background rebuilding.
            this.ScheduleBackground(chunk);
            scheduled++;
            
            // Stop if the per-frame build count budget has been reached.
            if (scheduled >= buildBudget) {
                break;
            }
        }
    }
    
    protected internal override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        base.Draw(context, framebuffer);
        
        Camera3D? cam3D = SceneManager.ActiveCam3D;
        
        if (cam3D == null) {
            return;
        }
        
        // Refresh transform versions.
        this.RefreshBoundsTransformVersion();
        this.RefreshRenderTransformVersion();
        
        // Process pending chunk/region uploads.
        this.ProcessPendingChunkUploads();
        this.ProcessPendingRegionUploads();
        
        // Check if the whole terrain is visible, if not, the draw method gets canceled.
        if (this.FrustumCulling) {
            bool terrainVisible = cam3D.GetFrustum().ContainsBox(this.GetTerrainWorldBounds());
            
            if (!terrainVisible) {
                return;
            }
        }
        
        // Reset indexer.
        this.TotalVertexCount = 0;
        this.TotalIndexCount = 0;
        
        // Draw far region batches.
        this.DrawFarRegionBatches(cam3D);
        
        // Draw normal/non-batched terrain chunks.
        foreach (IChunk chunk in this._meshChunkList) {
            if (chunk.Mesh == null) {
                continue;
            }
            
            // Apply pending in-place mesh updates before visibility/Lod checks.
            if (this._chunksPendingVertexUpload.Contains(chunk)) {
                chunk.UpdateGeometry(context.CommandList);
                this._chunksPendingVertexUpload.Remove(chunk);
            }
            
            // Skip chunks that LOD currently culls.
            if (chunk.Lod < 0) {
                continue;
            }
            
            // Skip this chunk if it is already included inside a far region batch.
            if (this._chunkRegions.TryGetValue(chunk, out TerrainRegionKey regionKey) && this._regionBatches.TryGetValue(regionKey, out TerrainRegionBatch? regionBatch) && regionBatch.Mesh != null) {
                continue;
            }
            
            // If frustum culling is enabled, skip chunks outside the camera view.
            if (this.FrustumCulling && this._chunkLocalBounds.TryGetValue(chunk, out BoundingBox chunkBounds)) {
                bool chunkVisible = cam3D.GetFrustum().ContainsBox(this.GetChunkWorldBounds(chunk, chunkBounds));
                
                if (!chunkVisible) {
                    continue;
                }
            }
            
            // Get or create the renderable for this chunk.
            Renderable renderable = this.GetOrCreateChunkRenderable(chunk);
            renderable.SetTransform(0, this._terrainRenderTransform);
            
            // Draw renderable.
            this.Entity.Scene.Renderer.DrawRenderable(renderable);
            
            // Add vertex/index count.
            this.TotalVertexCount += (int) chunk.Mesh.VertexCount;
            this.TotalIndexCount += (int) chunk.Mesh.IndexCount;
        }
    }
    
    public void DrawDebug(ImmediateRenderer immediateRenderer) {
        Camera3D? cam3D = SceneManager.ActiveCam3D;
        
        if (cam3D == null) {
            return;
        }
        
        // Refresh bounds transform version.
        this.RefreshBoundsTransformVersion();
        
        Transform transform = new Transform {
            Translation = this.LerpedGlobalPosition,
            Rotation = this.LerpedRotation,
            Scale = this.LerpedScale
        };
        
        // Draw terrain base box.
        immediateRenderer.DrawBoundingBox(transform, this._terrainLocalBounds, Color.Green);
        
        // Draw chunk boxes.
        foreach (IChunk chunk in this.Terrain.GetChunks()) {
            if (!this._chunkLocalBounds.TryGetValue(chunk, out BoundingBox chunkBox)) {
                continue;
            }
            
            bool isPending = this._buildingSet.ContainsKey(chunk) || this._queuedSet.ContainsKey(chunk) || chunk.IsDirty;
            bool hasMesh = chunk.Mesh != null;
            
            Color chunkColor = isPending ? Color.White : hasMesh ? Color.LightGray : Color.DarkGray;
            
            if (this.FrustumCulling && hasMesh && !cam3D.GetFrustum().ContainsBox(this.GetChunkWorldBounds(chunk, chunkBox))) {
                chunkColor = Color.Gray;
            }
            
            // Draw chunk box.
            immediateRenderer.DrawBoundingBox(transform, chunkBox, chunkColor);
        }
    }
    
    private void PreloadAllChunkMeshes() {
        Camera3D? cam3D = SceneManager.ActiveCam3D;
        
        if (cam3D == null) {
            return;
        }
        
        // Calculate the maximum LOD distance.
        float maxLodDistance = this.TerrainSettings.UseCameraFarPlaneForLodRange ? cam3D.FarPlane : float.PositiveInfinity;
        float maxLodDistanceSquared = maxLodDistance * maxLodDistance;
        
        // Set the initial LOD for every terrain chunk.
        foreach (IChunk chunk in this.Terrain.GetChunks()) {
            float distanceSquared = this.GetChunkDistanceSquared(cam3D.Position, chunk);
            int targetLod = this.TerrainSettings.EnableLod ? this.DetermineLod(distanceSquared, this.TerrainSettings.LodDistances, this.TerrainSettings.CullChunksBeyondLastLod, maxLodDistanceSquared) : 0;
            
            if (chunk.Lod != targetLod) {
                chunk.Lod = targetLod;
            }
        }
        
        // Get all chunks that should be visible.
        IChunk[] chunks = this.Terrain.GetChunks().Where(chunk => chunk.Lod >= 0).ToArray();
        
        // Generate all visible chunk meshes in parallel.
        Parallel.ForEach(chunks, new ParallelOptions() {
            MaxDegreeOfParallelism = Math.Max(1, this.TerrainSettings.MaxConcurrentChunkBuilds)
        }, chunk => chunk.GenerateGeometry());
        
        // Upload generated chunk meshes and refresh their render state.
        foreach (IChunk chunk in chunks) {
            this.UploadChunkGeometry(chunk);
            this.RefreshMeshChunkState(chunk);
        }
        
        // Update far chunk batching membership.
        this.UpdateFarChunkMembership();
        
        // Force process all dirty far terrain regions.
        this.ProcessDirtyRegions(cam3D, true);
    }
    
    private bool ShouldSchedule(IChunk chunk) {
        
        // Skip chunks that are already queued or currently building.
        if (this._queuedSet.ContainsKey(chunk) || this._buildingSet.ContainsKey(chunk)) {
            return false;
        }
        
        // Skip culled chunks that do not have an existing mesh.
        if (chunk.Lod < 0 && chunk.Mesh == null) {
            return false;
        }
        
        return true;
    }
    
    private void ScheduleBackground(IChunk chunk) {
        
        // Mark the chunk as building so it cannot be scheduled twice.
        if (!this._buildingSet.TryAdd(chunk, 0)) {
            return;
        }
        
        // Generate the chunk geometry in the background.
        _ = Task.Run(() => {
            try {
                chunk.GenerateGeometry();
                
                // Queue the chunk for GPU upload after the geometry was generated.
                if (this._queuedSet.TryAdd(chunk, 0)) {
                    this._pendingUpload.Enqueue(chunk);
                }
            }
            finally {
                
                // Remove the chunk from the building set once the background work is done.
                this._buildingSet.TryRemove(chunk, out _);
            }
        });
    }
    
    private void UpdateLodLevels(double delta) {
        Camera3D? cam3D = SceneManager.ActiveCam3D;
        
        if (cam3D == null) {
            return;
        }
        
        TerrainSettings settings = this.TerrainSettings;
        
        // Accumulate time since the last LOD update.
        this._lodUpdateAccumulator += (float) delta;
        
        // Calculate LOD update timing and movement thresholds.
        float updateInterval = Math.Max(0.0F, settings.LodUpdateIntervalSeconds);
        float moveThreshold = Math.Max(0.0F, settings.LodUpdateCameraMoveThreshold);
        float moveThresholdSquared = moveThreshold * moveThreshold;
        
        // Check if the camera moved far enough to force a LOD update.
        bool movedEnough = !this._hasLastLodCameraPosition || Vector3.DistanceSquared(this._lastLodCameraPosition, cam3D.Position) >= moveThresholdSquared;
        
        // Check if enough time has passed since the last LOD update.
        bool waitedEnough = this._lodUpdateAccumulator >= updateInterval;
        
        if (!movedEnough && !waitedEnough) {
            return;
        }
        
        // Reset LOD update state.
        this._lodUpdateAccumulator = 0.0F;
        this._hasLastLodCameraPosition = true;
        this._lastLodCameraPosition = cam3D.Position;
        this._lodRingTick++;
        
        // Calculate the maximum allowed LOD distance.
        float maxLodDistance = settings.UseCameraFarPlaneForLodRange ? cam3D.FarPlane : float.PositiveInfinity;
        float maxLodDistanceSquared = maxLodDistance * maxLodDistance;
        
        // Calculate near and mid-ring distances.
        float nearRingDistance = settings.LodDistances.Length == 0 ? float.PositiveInfinity : settings.LodDistances[Math.Min(1, settings.LodDistances.Length - 1)];
        float midRingDistance = settings.LodDistances.Length == 0 ? float.PositiveInfinity : settings.LodDistances[Math.Min(2, settings.LodDistances.Length - 1)];
        
        float nearRingDistanceSquared = nearRingDistance * nearRingDistance;
        float midRingDistanceSquared = midRingDistance * midRingDistance;
        
        // Calculate how often mid and far ring chunks are updated.
        int midRingInterval = Math.Max(1, settings.MidRingLodUpdateIntervalTicks);
        int farRingInterval = Math.Max(1, settings.FarRingLodUpdateIntervalTicks);
        
        bool updateMidRing = this._lodRingTick % midRingInterval == 0;
        bool updateFarRing = this._lodRingTick % farRingInterval == 0;
        
        foreach (IChunk chunk in this.Terrain.GetChunks()) {
            float distanceSquared = this.GetChunkDistanceSquared(cam3D.Position, chunk);
            
            // Check which LOD update ring this chunk belongs to.
            bool inNearRing = distanceSquared <= nearRingDistanceSquared;
            bool inMidRing = distanceSquared <= midRingDistanceSquared;
            
            // Only update far chunks on slower intervals.
            bool shouldUpdateChunk = inNearRing || (inMidRing && updateMidRing) || (!inMidRing && updateFarRing);
            
            if (!shouldUpdateChunk) {
                continue;
            }
            
            // Determine the target LOD for this chunk.
            int targetLod = settings.EnableLod ? this.DetermineLod(distanceSquared, settings.LodDistances, settings.CullChunksBeyondLastLod, maxLodDistanceSquared) : 0;
            
            if (this.ShouldHoldCurrentLod(chunk.Lod, targetLod, distanceSquared)) {
                continue;
            }
            
            if (chunk.Lod == targetLod) {
                continue;
            }
            
            // Apply the new LOD and update far batching membership.
            chunk.Lod = targetLod;
            this.UpdateChunkRegionMembership(chunk);
        }
    }
    
    private int DetermineLod(float distanceSquared, float[] lodDistances, bool cullBeyondLastLod, float maxDistanceSquared) {
        
        // Cull the chunk if it is beyond the maximum allowed LOD distance.
        if (distanceSquared > maxDistanceSquared) {
            return -1;
        }
        
        // Use the highest detail LOD when no LOD distances are defined.
        if (lodDistances.Length == 0) {
            return 0;
        }
        
        // Find the first LOD distance that contains this chunk.
        for (int i = 0; i < lodDistances.Length; i++) {
            float lodDistance = lodDistances[i];
            
            if (distanceSquared <= lodDistance * lodDistance) {
                return i;
            }
        }
        
        // Cull chunks beyond the last LOD or keep them at the lowest detail LOD.
        return cullBeyondLastLod ? -1 : lodDistances.Length - 1;
    }
    
    private bool ShouldHoldCurrentLod(int currentLod, int targetLod, float distanceSquared) {
        TerrainSettings settings = this.TerrainSettings;
        float[] lodDistances = settings.LodDistances;
        float hysteresis = settings.LodHysteresis;
        
        // Do not apply hysteresis when LOD is invalid, unchanged, or disabled.
        if (currentLod < 0 || targetLod == currentLod || lodDistances.Length == 0 || hysteresis <= 0.0F) {
            return false;
        }
        
        // Hold the current higher-detail LOD a bit longer before switching to lower detail.
        if (targetLod > currentLod) {
            if (currentLod >= lodDistances.Length) {
                return false;
            }
            
            float upperThreshold = lodDistances[currentLod] * (1.0F + hysteresis);
            float upperThresholdSquared = upperThreshold * upperThreshold;
            
            return distanceSquared <= upperThresholdSquared;
        }
        
        // Do not hold when the target LOD is culled or outside the distance table.
        if (targetLod < 0 || targetLod >= lodDistances.Length) {
            return false;
        }
        
        // Hold the current lower-detail LOD a bit longer before switching to higher detail.
        float lowerThreshold = lodDistances[targetLod] * (1.0F - hysteresis);
        float lowerThresholdSquared = lowerThreshold * lowerThreshold;
        
        return distanceSquared >= lowerThresholdSquared;
    }
    
    private float GetChunkDistanceSquared(Vector3 cameraPosition, IChunk chunk) {
        
        // Get or calculate the chunk center in terrain local space.
        if (!this._chunkLocalCenters.TryGetValue(chunk, out Vector3 chunkLocalCenter)) {
            chunkLocalCenter = chunk.Position + new Vector3(chunk.Width * 0.5F, chunk.Height * 0.5F, chunk.Depth * 0.5F);
            this._chunkLocalCenters[chunk] = chunkLocalCenter;
        }
        
        // Convert the local chunk center to world space.
        Vector3 chunkWorldCenter = this.LerpedGlobalPosition + chunkLocalCenter;
        
        // Return squared distance to avoid a square root.
        return Vector3.DistanceSquared(cameraPosition, chunkWorldCenter);
    }
    
    private void ProcessPendingChunkUploads() {
        int uploadBudget = Math.Max(0, this.TerrainSettings.MaxChunkUploadsPerFrame);
        
        if (uploadBudget == 0) {
            return;
        }
        
        int uploads = 0;
        
        // Upload pending chunk meshes until the per-frame budget is reached.
        while (uploads < uploadBudget && this._pendingUpload.TryDequeue(out IChunk? chunk)) {
            this._queuedSet.TryRemove(chunk, out _);
            this.UploadChunkGeometry(chunk);
            this.RefreshMeshChunkState(chunk);
            uploads++;
        }
    }
    
    private void ProcessPendingRegionUploads() {
        TerrainSettings settings = this.TerrainSettings;
        
        int uploadBudget = Math.Max(0, settings.MaxRegionUploadsPerFrame);
        
        if (uploadBudget == 0) {
            return;
        }
        
        double uploadBudgetMs = Math.Max(0.0F, settings.RegionUploadBudgetMilliseconds);
        Stopwatch uploadStopwatch = Stopwatch.StartNew();
        int uploads = 0;
        
        // Upload pending region meshes until the count or time budget is reached.
        while (uploads < uploadBudget && this._pendingRegionUploads.TryDequeue(out TerrainRegionBuildResult buildResult)) {
            if (uploadBudgetMs > 0.0 && uploads > 0 && uploadStopwatch.Elapsed.TotalMilliseconds >= uploadBudgetMs) {
                this._pendingRegionUploads.Enqueue(buildResult);
                break;
            }
            
            // Replace the old region batch with the newly built result.
            this.DisposeRegionBatch(buildResult.Key);
            
            if (buildResult.HasGeometry) {
                BasicMeshData meshData = new BasicMeshData(buildResult.Vertices!, buildResult.Indices!);
                Mesh<Vertex3D> regionMesh = new Mesh<Vertex3D>(this.GraphicsDevice, this.Terrain.Material, meshData);
                Renderable renderable = new Renderable(regionMesh, new Transform(), this.Terrain.Material);
                
                this._regionBatches[buildResult.Key] = new TerrainRegionBatch(regionMesh, renderable, buildResult.LocalBounds, buildResult.VertexCount, buildResult.IndexCount);
            }
            
            // Clear cached render/bounds data for the replaced region.
            this._regionWorldBounds.Remove(buildResult.Key);
            
            uploads++;
        }
    }
    
    private void RefreshMeshChunkState(IChunk chunk) {
        bool hasMesh = chunk.Mesh != null;
        
        // Clear pending vertex upload state when the chunk no longer has a mesh.
        if (!hasMesh) {
            this._chunksPendingVertexUpload.Remove(chunk);
        }
        
        if (hasMesh) {
            
            // Add the chunk to the render list if it is not already tracked.
            if (!this._meshChunkList.Contains(chunk)) {
                this._meshChunkList.Add(chunk);
            }
            
            // Update far batching membership for the refreshed chunk.
            this.UpdateChunkRegionMembership(chunk);
            return;
        }
        
        // Remove all render state for chunks without geometry.
        this._meshChunkList.Remove(chunk);
        this.RemoveChunkFromRegion(chunk);
        this._chunkWorldBounds.Remove(chunk);
        
        // Dispose the old renderable if the chunk had one.
        if (this._renderables.Remove(chunk, out Renderable? renderable)) {
            renderable.Dispose();
        }
    }
    
    private void UploadChunkGeometry(IChunk chunk) {
        
        // Upload immediately when there is no pending geometry or in-place update is not possible.
        if (!chunk.HasPendingGeometry || !chunk.CanUpdateGeometryInPlace) {
            chunk.UploadGeometry(this.GraphicsDevice);
            this._chunksPendingVertexUpload.Remove(chunk);
            return;
        }
        
        // Defer the vertex update so it can be applied during rendering.
        this._chunksPendingVertexUpload.Add(chunk);
    }
    
    private void DrawFarRegionBatches(Camera3D cam3D) {
        foreach ((TerrainRegionKey regionKey, TerrainRegionBatch batch) in this._regionBatches) {
            Renderable? renderable = batch.Renderable;
            
            // Skip empty or disposed region batches.
            if (batch.Mesh == null || renderable == null) {
                continue;
            }
            
            // Skip region batches outside the camera view.
            if (this.FrustumCulling) {
                bool visible = cam3D.GetFrustum().ContainsBox(this.GetRegionWorldBounds(regionKey, batch.LocalBounds));
                
                if (!visible) {
                    continue;
                }
            }
            
            // Apply the current terrain transform to the region renderable.
            renderable.SetTransform(0, this._terrainRenderTransform);
            
            // Draw the region batch.
            this.Entity.Scene.Renderer.DrawRenderable(renderable);
            
            // Add region batch geometry to the debug counters.
            this.TotalVertexCount += batch.VertexCount;
            this.TotalIndexCount += batch.IndexCount;
        }
    }
    
    private Renderable GetOrCreateChunkRenderable(IChunk chunk) {
        if (chunk.Mesh == null) {
            throw new InvalidOperationException("Cannot create a renderable for a chunk without a mesh.");
        }
        
        // Reuse the existing renderable when it already uses the current chunk mesh.
        if (this._renderables.TryGetValue(chunk, out Renderable? renderable) && ReferenceEquals(renderable.Mesh, chunk.Mesh)) {
            return renderable;
        }
        
        renderable?.Dispose();
        Renderable newRenderable = new Renderable(chunk.Mesh, new Transform(), this.Terrain.Material);
        this._renderables[chunk] = newRenderable;
        return newRenderable;
    }
    
    private void RefreshBoundsTransformVersion() {
        if (!this._hasCachedBoundsTransform ||
            this._cachedBoundsPosition != this.LerpedGlobalPosition ||
            this._cachedBoundsRotation != this.LerpedRotation ||
            this._cachedBoundsScale != this.LerpedScale) {
            
            this._hasCachedBoundsTransform = true;
            this._cachedBoundsPosition = this.LerpedGlobalPosition;
            this._cachedBoundsRotation = this.LerpedRotation;
            this._cachedBoundsScale = this.LerpedScale;
            this._boundsCacheTransformVersion++;
        }
    }
    
    private void RefreshRenderTransformVersion() {
        if (!this._hasCachedRenderTransform ||
            this._cachedRenderPosition != this.LerpedGlobalPosition ||
            this._cachedRenderRotation != this.LerpedRotation ||
            this._cachedRenderScale != this.LerpedScale) {
            
            this._hasCachedRenderTransform = true;
            this._cachedRenderPosition = this.LerpedGlobalPosition;
            this._cachedRenderRotation = this.LerpedRotation;
            this._cachedRenderScale = this.LerpedScale;
            
            this._terrainRenderTransform.Translation = this.LerpedGlobalPosition;
            this._terrainRenderTransform.Rotation = this.LerpedRotation;
            this._terrainRenderTransform.Scale = this.LerpedScale;
        }
    }
    
    private BoundingBox GetTerrainWorldBounds() {
        int currentVersion = this._boundsCacheTransformVersion;
        
        // Return the cached terrain bounds if the transform has not changed.
        if (this._cachedTerrainWorldBoundsVersion == currentVersion) {
            return this._cachedTerrainWorldBounds;
        }
        
        // Recalculate and cache the terrain world bounds.
        this._cachedTerrainWorldBounds = this.ComputeWorldAabb(this._terrainLocalBounds);
        this._cachedTerrainWorldBoundsVersion = currentVersion;
        
        return this._cachedTerrainWorldBounds;
    }
    
    private BoundingBox GetChunkWorldBounds(IChunk chunk, BoundingBox localBounds) {
        int currentVersion = this._boundsCacheTransformVersion;
        
        // Return the cached chunk bounds if the transform has not changed.
        if (this._chunkWorldBounds.TryGetValue(chunk, out CachedWorldBounds cached) && cached.Version == currentVersion) {
            return cached.Bounds;
        }
        
        // Recalculate and cache the chunk world bounds.
        BoundingBox worldBounds = this.ComputeWorldAabb(localBounds);
        this._chunkWorldBounds[chunk] = new CachedWorldBounds(worldBounds, currentVersion);
        
        return worldBounds;
    }
    
    private BoundingBox GetRegionWorldBounds(TerrainRegionKey key, BoundingBox localBounds) {
        int currentVersion = this._boundsCacheTransformVersion;
        
        // Return the cached region bounds if the transform has not changed.
        if (this._regionWorldBounds.TryGetValue(key, out CachedWorldBounds cached) && cached.Version == currentVersion) {
            return cached.Bounds;
        }
        
        // Recalculate and cache the region world bounds.
        BoundingBox worldBounds = this.ComputeWorldAabb(localBounds);
        this._regionWorldBounds[key] = new CachedWorldBounds(worldBounds, currentVersion);
        
        return worldBounds;
    }
    
    private BoundingBox ComputeWorldAabb(BoundingBox localBox) {
        Vector3 localMin = localBox.Min;
        Vector3 localMax = localBox.Max;
        
        Vector3 worldMin = new Vector3(float.MaxValue);
        Vector3 worldMax = new Vector3(float.MinValue);
        
        // Transform all local bounds corners and expand the world AABB around them.
        this.ExpandWorldAabb(localMin.X, localMin.Y, localMin.Z, ref worldMin, ref worldMax);
        this.ExpandWorldAabb(localMax.X, localMin.Y, localMin.Z, ref worldMin, ref worldMax);
        this.ExpandWorldAabb(localMin.X, localMax.Y, localMin.Z, ref worldMin, ref worldMax);
        this.ExpandWorldAabb(localMax.X, localMax.Y, localMin.Z, ref worldMin, ref worldMax);
        this.ExpandWorldAabb(localMin.X, localMin.Y, localMax.Z, ref worldMin, ref worldMax);
        this.ExpandWorldAabb(localMax.X, localMin.Y, localMax.Z, ref worldMin, ref worldMax);
        this.ExpandWorldAabb(localMin.X, localMax.Y, localMax.Z, ref worldMin, ref worldMax);
        this.ExpandWorldAabb(localMax.X, localMax.Y, localMax.Z, ref worldMin, ref worldMax);
        
        return new BoundingBox {
            Min = worldMin,
            Max = worldMax
        };
    }
    
    private void ExpandWorldAabb(float x, float y, float z, ref Vector3 worldMin, ref Vector3 worldMax) {
        Vector3 localPoint = new Vector3(x, y, z);
        Vector3 scaledPoint = localPoint * this.LerpedScale;
        Vector3 rotatedPoint = Vector3.Transform(scaledPoint, this.LerpedRotation);
        Vector3 worldPoint = rotatedPoint + this.LerpedGlobalPosition;
        
        worldMin = Vector3.Min(worldMin, worldPoint);
        worldMax = Vector3.Max(worldMax, worldPoint);
    }
    
    private void UpdateFarChunkMembership() {
        TerrainSettings settings = this.TerrainSettings;
        bool batchingEnabled = settings.EnableFarChunkBatching && settings.FarChunkBatchRegionSizeInChunks > 0;
        
        // Clear all region batching state when far chunk batching is disabled.
        if (!batchingEnabled) {
            this.ClearAllChunkRegions();
            return;
        }
        
        // Refresh region membership for every terrain chunk.
        foreach (IChunk chunk in this.Terrain.GetChunks()) {
            this.UpdateChunkRegionMembership(chunk);
        }
    }
    
    private void UpdateChunkRegionMembership(IChunk chunk) {
        
        // Remove chunks that should not be included in far region batching.
        if (!this.ShouldBatchChunk(chunk)) {
            this.RemoveChunkFromRegion(chunk);
            return;
        }
        
        TerrainRegionKey targetKey = this.GetRegionKey(chunk);
        
        // Keep the current membership if the chunk is already in the correct region.
        if (this._chunkRegions.TryGetValue(chunk, out TerrainRegionKey currentKey) && currentKey == targetKey) {
            return;
        }
        
        // Move the chunk from its old region into the target region.
        this.RemoveChunkFromRegion(chunk);
        this._chunkRegions[chunk] = targetKey;
        
        if (!this._regionMembers.TryGetValue(targetKey, out HashSet<IChunk>? members)) {
            members = new HashSet<IChunk>();
            this._regionMembers[targetKey] = members;
        }
        
        members.Add(chunk);
        
        // Mark the target region dirty so its batch can be rebuilt.
        this._dirtyRegions.Add(targetKey);
    }
    
    private void RemoveChunkFromRegion(IChunk chunk) {
        
        // Stop if the chunk is not assigned to any region.
        if (!this._chunkRegions.Remove(chunk, out TerrainRegionKey regionKey)) {
            return;
        }
        
        // Remove the chunk from its region member list.
        if (this._regionMembers.TryGetValue(regionKey, out HashSet<IChunk>? members)) {
            members.Remove(chunk);
            
            if (members.Count == 0) {
                this._regionMembers.Remove(regionKey);
            }
        }
        
        // Mark the old region dirty so its batch can be rebuilt.
        this._dirtyRegions.Add(regionKey);
    }
    
    private void ClearAllChunkRegions() {
        
        // Dispose all existing region batches.
        foreach (TerrainRegionBatch batch in this._regionBatches.Values) {
            batch.Dispose();
        }
        
        // Clear all region batching state.
        this._regionBatches.Clear();
        this._chunkRegions.Clear();
        this._regionMembers.Clear();
        this._regionWorldBounds.Clear();
        this._dirtyRegions.Clear();
    }
    
    private bool ShouldBatchChunk(IChunk chunk) {
        int minBatchLod = this.TerrainSettings.FarChunkBatchMinLod;
        
        return chunk.Mesh != null &&
               chunk.Lod >= minBatchLod &&
               chunk.CurrentLod >= minBatchLod;
    }
    
    private TerrainRegionKey GetRegionKey(IChunk chunk) {
        int chunkSize = Math.Max(1, this.Terrain.ChunkSize);
        int regionSizeInChunks = Math.Max(1, this.TerrainSettings.FarChunkBatchRegionSizeInChunks);
        
        // Convert world chunk position to chunk grid coordinates.
        int chunkX = Math.Max(0, (int) chunk.Position.X / chunkSize);
        int chunkZ = Math.Max(0, (int) chunk.Position.Z / chunkSize);
        
        // Convert chunk grid coordinates to region grid coordinates.
        int regionX = chunkX / regionSizeInChunks;
        int regionZ = chunkZ / regionSizeInChunks;
        
        return new TerrainRegionKey(regionX, regionZ, chunk.CurrentLod);
    }
    
    private void ProcessDirtyRegions(Camera3D camera, bool forceAll) {
        if (this._dirtyRegions.Count == 0) {
            return;
        }
        
        TerrainSettings settings = this.TerrainSettings;
        Vector3 cameraPosition = camera.Position;
        
        // Copy dirty regions into a buffer so the set can be modified while scheduling.
        this._regionDirtyBuffer.Clear();
        this._regionDirtyBuffer.AddRange(this._dirtyRegions);
        
        // Prioritize regions closest to the camera.
        this._regionDirtyBuffer.Sort((a, b) => {
            float distanceA = this.GetRegionDistanceSquared(cameraPosition, a);
            float distanceB = this.GetRegionDistanceSquared(cameraPosition, b);
            return distanceA.CompareTo(distanceB);
        });
        
        int rebuildBudget = forceAll ? int.MaxValue : Math.Max(0, settings.MaxRegionRebuildsPerFrame);
        double scheduleBudgetMs = forceAll ? double.PositiveInfinity : Math.Max(0.0F, settings.RegionScheduleBudgetMilliseconds);
        
        Stopwatch scheduleStopwatch = Stopwatch.StartNew();
        int scheduled = 0;
        
        // Schedule region rebuilds until the count or time budget is reached.
        foreach (TerrainRegionKey regionKey in this._regionDirtyBuffer) {
            if (scheduled >= rebuildBudget) {
                break;
            }
            
            if (scheduleBudgetMs > 0.0 && scheduled > 0 && scheduleStopwatch.Elapsed.TotalMilliseconds >= scheduleBudgetMs) {
                break;
            }
            
            if (!this.ScheduleRegionBuild(regionKey)) {
                continue;
            }
            
            this._dirtyRegions.Remove(regionKey);
            scheduled++;
        }
    }
    
    private float GetRegionDistanceSquared(Vector3 cameraPosition, TerrainRegionKey key) {
        int chunkSize = Math.Max(1, this.Terrain.ChunkSize);
        int regionSizeInChunks = Math.Max(1, this.TerrainSettings.FarChunkBatchRegionSizeInChunks);
        
        float regionSize = chunkSize * regionSizeInChunks;
        
        // Calculate the region center in terrain local space.
        Vector3 regionLocalCenter = new Vector3(
            (key.X + 0.5F) * regionSize,
            this.Terrain.Height * 0.5F,
            (key.Z + 0.5F) * regionSize
        );
        
        // Convert the region center to world space.
        Vector3 regionWorldCenter = this.LerpedGlobalPosition + regionLocalCenter;
        
        // Return squared distance to avoid a square root.
        return Vector3.DistanceSquared(cameraPosition, regionWorldCenter);
    }
    
    private bool ScheduleRegionBuild(TerrainRegionKey regionKey) {
        
        // Mark the region as building so it cannot be scheduled twice.
        if (!this._regionBuildingSet.TryAdd(regionKey, 0)) {
            return false;
        }
        
        // Copy the current region members so the background build has a stable snapshot.
        IChunk[] memberSnapshot = this._regionMembers.TryGetValue(regionKey, out HashSet<IChunk>? members) ? members.ToArray() : [];
        
        // Build the region geometry in the background.
        _ = Task.Run(() => {
            try {
                TerrainRegionBuildResult buildResult = BuildRegionGeometry(regionKey, memberSnapshot);
                this._pendingRegionUploads.Enqueue(buildResult);
            }
            finally {
                
                // Remove the region from the building set once the background work is done.
                this._regionBuildingSet.TryRemove(regionKey, out _);
            }
        });
        
        return true;
    }
    
    private TerrainRegionBuildResult BuildRegionGeometry(TerrainRegionKey regionKey, IChunk[] members) {
        int totalVertexCount = 0;
        int totalIndexCount = 0;
        
        bool hasBounds = false;
        BoundingBox regionBounds = default;
        
        List<BasicMeshData> meshDataList = new List<BasicMeshData>(members.Length);
        
        // Collect valid chunk mesh data and calculate the final buffer sizes.
        foreach (IChunk chunk in members) {
            if (chunk.Mesh is not Mesh<Vertex3D> mesh ||
                mesh.MeshData is not BasicMeshData meshData ||
                meshData.Vertices.Length == 0 ||
                meshData.Indices.Length == 0) {
                continue;
            }
            
            meshDataList.Add(meshData);
            totalVertexCount += meshData.Vertices.Length;
            totalIndexCount += meshData.Indices.Length;
            
            // Expand the region bounds to include this chunk.
            if (this._chunkLocalBounds.TryGetValue(chunk, out BoundingBox chunkBounds)) {
                if (!hasBounds) {
                    regionBounds = chunkBounds;
                    hasBounds = true;
                }
                else {
                    regionBounds = new BoundingBox {
                        Min = Vector3.Min(regionBounds.Min, chunkBounds.Min),
                        Max = Vector3.Max(regionBounds.Max, chunkBounds.Max)
                    };
                }
            }
        }
        
        // Return an empty result when no valid geometry was found.
        if (totalVertexCount == 0 || totalIndexCount == 0 || !hasBounds) {
            return TerrainRegionBuildResult.Empty(regionKey);
        }
        
        Vertex3D[] mergedVertices = new Vertex3D[totalVertexCount];
        uint[] mergedIndices = new uint[totalIndexCount];
        
        int vertexOffset = 0;
        int indexOffset = 0;
        
        // Merge all chunk meshes into one region mesh.
        foreach (BasicMeshData meshData in meshDataList) {
            Vertex3D[] vertices = meshData.Vertices;
            uint[] indices = meshData.Indices;
            
            Array.Copy(vertices, 0, mergedVertices, vertexOffset, vertices.Length);
            
            for (int i = 0; i < indices.Length; i++) {
                mergedIndices[indexOffset + i] = indices[i] + (uint) vertexOffset;
            }
            
            vertexOffset += vertices.Length;
            indexOffset += indices.Length;
        }
        
        return TerrainRegionBuildResult.Create(
            regionKey,
            mergedVertices,
            mergedIndices,
            regionBounds,
            totalVertexCount,
            totalIndexCount
        );
    }
    
    private void DisposeRegionBatch(TerrainRegionKey regionKey) {
        if (!this._regionBatches.Remove(regionKey, out TerrainRegionBatch? batch)) {
            return;
        }
        
        // Clear cached bounds for the disposed region.
        this._regionWorldBounds.Remove(regionKey);
        batch.Dispose();
    }
    
    private readonly struct CachedWorldBounds {
        
        public readonly BoundingBox Bounds;
        
        public readonly int Version;
        
        public CachedWorldBounds(BoundingBox bounds, int version) {
            this.Bounds = bounds;
            this.Version = version;
        }
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            // Dispose chunk renderables.
            foreach (Renderable renderable in this._renderables.Values) {
                renderable.Dispose();
            }
            
            this._renderables.Clear();
            
            // Dispose region batches.
            foreach (TerrainRegionBatch batch in this._regionBatches.Values) {
                batch.Dispose();
            }
            
            this._regionBatches.Clear();
            
            // Dispose terrain chunks.
            foreach (IChunk chunk in this.Terrain.GetChunks()) {
                chunk.Dispose();
            }
            
            // Clear chunk state.
            this._meshChunkList.Clear();
            this._pendingUpload.Clear();
            this._queuedSet.Clear();
            this._buildingSet.Clear();
            this._chunksPendingVertexUpload.Clear();
            
            // Clear chunk caches.
            this._chunkLocalBounds.Clear();
            this._chunkLocalCenters.Clear();
            this._chunkWorldBounds.Clear();
            
            // Clear region state.
            this._pendingRegionUploads.Clear();
            this._regionBuildingSet.Clear();
            this._chunkRegions.Clear();
            this._regionMembers.Clear();
            this._regionWorldBounds.Clear();
            this._dirtyRegions.Clear();
        }
    }
}
