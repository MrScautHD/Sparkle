namespace Sparkle.CSharp.Terrain;

public class TerrainSettings {
    
    /// <summary>
    /// Maximum number of chunk meshes that can be uploaded to the GPU per frame.
    /// </summary>
    public int MaxChunkUploadsPerFrame;
    
    /// <summary>
    /// Maximum number of chunk mesh rebuilds that can be scheduled per frame.
    /// </summary>
    public int MaxChunkBuildsPerFrame;
    
    /// <summary>
    /// Maximum amount of time, in milliseconds, that can be spent scheduling chunk builds per frame.
    /// A value of 0 disables the time budget.
    /// </summary>
    public float ChunkBuildScheduleBudgetMilliseconds;
    
    /// <summary>
    /// Maximum number of chunk mesh builds that may run in the background at the same time.
    /// </summary>
    public int MaxConcurrentChunkBuilds;
    
    /// <summary>
    /// Distance thresholds used to determine terrain LOD levels.
    /// Each entry represents the maximum distance for that LOD index.
    /// </summary>
    public float[] LodDistances;
    
    /// <summary>
    /// Whether terrain LOD selection is enabled.
    /// When disabled, all visible chunks use LOD 0.
    /// </summary>
    public bool EnableLod;
    
    /// <summary>
    /// Extra distance padding used to prevent chunks from rapidly switching LOD near threshold boundaries.
    /// </summary>
    public float LodHysteresis;
    
    /// <summary>
    /// Whether chunks beyond the last LOD distance should be culled instead of using the last available LOD.
    /// </summary>
    public bool CullChunksBeyondLastLod;
    
    /// <summary>
    /// Whether the active camera far plane should limit the maximum terrain LOD range.
    /// </summary>
    public bool UseCameraFarPlaneForLodRange;
    
    /// <summary>
    /// Whether terrain background work should pause when the whole terrain is outside the camera view.
    /// </summary>
    public bool PauseTerrainWorkWhenOutOfView;
    
    /// <summary>
    /// Minimum time, in seconds, between LOD update passes.
    /// </summary>
    public float LodUpdateIntervalSeconds;
    
    /// <summary>
    /// Minimum camera movement distance required to force a LOD update.
    /// </summary>
    public float LodUpdateCameraMoveThreshold;
    
    /// <summary>
    /// Whether far terrain chunks should be merged into larger region batches.
    /// </summary>
    public bool EnableFarChunkBatching;
    
    /// <summary>
    /// Minimum LOD level required for a chunk to be included in a far region batch.
    /// </summary>
    public int FarChunkBatchMinLod;
    
    /// <summary>
    /// Size of one far region batch in chunks along the X and Z axes.
    /// </summary>
    public int FarChunkBatchRegionSizeInChunks;
    
    /// <summary>
    /// Maximum number of dirty far region batches that can be scheduled for rebuilding per frame.
    /// </summary>
    public int MaxRegionRebuildsPerFrame;
    
    /// <summary>
    /// Maximum number of rebuilt far region batches that can be uploaded to the GPU per frame.
    /// </summary>
    public int MaxRegionUploadsPerFrame;
    
    /// <summary>
    /// Maximum amount of time, in milliseconds, that can be spent scheduling region rebuilds per frame.
    /// A value of 0 disables the time budget.
    /// </summary>
    public float RegionScheduleBudgetMilliseconds;
    
    /// <summary>
    /// Maximum amount of time, in milliseconds, that can be spent uploading region batches per frame.
    /// A value of 0 disables the time budget.
    /// </summary>
    public float RegionUploadBudgetMilliseconds;
    
    /// <summary>
    /// Number of LOD update ticks between updates for mid-distance chunks.
    /// Higher values update mid-distance chunks less often.
    /// </summary>
    public int MidRingLodUpdateIntervalTicks;
    
    /// <summary>
    /// Number of LOD update ticks between updates for far-distance chunks.
    /// Higher values update far chunks less often.
    /// </summary>
    public int FarRingLodUpdateIntervalTicks;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TerrainSettings"/> class with default terrain performance settings.
    /// </summary>
    public TerrainSettings() {
        this.MaxChunkUploadsPerFrame = 6;
        this.MaxChunkBuildsPerFrame = 8;
        this.ChunkBuildScheduleBudgetMilliseconds = 1.5F;
        this.MaxConcurrentChunkBuilds = Math.Max(1, Environment.ProcessorCount - 1);
        this.LodDistances = [200.0F, 400.0F, 800.0F];
        this.EnableLod = true;
        this.LodHysteresis = 0.15F;
        this.CullChunksBeyondLastLod = false;
        this.UseCameraFarPlaneForLodRange = true;
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
    }
}