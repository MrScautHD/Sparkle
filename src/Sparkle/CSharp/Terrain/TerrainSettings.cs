namespace Sparkle.CSharp.Terrain;

public class TerrainSettings {
    
    public int MaxChunkUploadsPerFrame;
    
    public int MaxChunkBuildsPerFrame;
    
    public float ChunkBuildScheduleBudgetMilliseconds;
    
    public int MaxConcurrentChunkBuilds;
    
    public float[] LodDistances;
    
    public bool EnableLod;
    
    public float LodHysteresis;
    
    public bool CullChunksBeyondLastLod;
    
    public bool UseCameraFarPlaneForLodRange;
    
    public bool PauseTerrainWorkWhenOutOfView;
    
    public float LodUpdateIntervalSeconds;
    
    public float LodUpdateCameraMoveThreshold;
    
    public bool EnableFarChunkBatching;
    
    public int FarChunkBatchMinLod;
    
    public int FarChunkBatchRegionSizeInChunks;
    
    public int MaxRegionRebuildsPerFrame;
    
    public int MaxRegionUploadsPerFrame;
    
    public float RegionScheduleBudgetMilliseconds;
    
    public float RegionUploadBudgetMilliseconds;
    
    public int MidRingLodUpdateIntervalTicks;
    
    public int FarRingLodUpdateIntervalTicks;
    
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