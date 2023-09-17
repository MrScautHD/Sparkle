using JoltPhysicsSharp;
using Sparkle.csharp.physics.layers;

namespace Sparkle.csharp.physics; 

public class Simulation : IDisposable {

    public PhysicsSystem PhysicsSystem { get; private set; }
    public PhysicsSettings Settings { get; private set; }
    public bool HasDisposed { get; private set; }
    
    private TempAllocator _allocator;
    private JobSystemThreadPool _jobSystem;

    private BroadPhaseLayerInterfaceImpl _broadPhaseLayer;
    private ObjectVsBroadPhaseLayerFilterImpl _broadPhaseLayerFilter;
    private ObjectLayerPairFilterImpl _objectLayerPairFilter;

    public Simulation(PhysicsSettings settings) {
        Foundation.Init();
        
        this.Settings = settings;
        
        this._allocator = new TempAllocator(10 * (int) settings.MaxContactConstraints * (int) settings.MaxContactConstraints);
        this._jobSystem = new JobSystemThreadPool(Foundation.MaxPhysicsJobs, Foundation.MaxPhysicsBarriers);
        
        this._broadPhaseLayer = new BroadPhaseLayerInterfaceImpl();
        this._broadPhaseLayerFilter = new ObjectVsBroadPhaseLayerFilterImpl();
        this._objectLayerPairFilter = new ObjectLayerPairFilterImpl();
        
        this.PhysicsSystem = new PhysicsSystem();
        this.PhysicsSystem.Init(settings.MaxBodies, settings.NumBodyMutexes, settings.MaxBodyPairs, settings.MaxContactConstraints, this._broadPhaseLayer, this._broadPhaseLayerFilter, this._objectLayerPairFilter);
        this.PhysicsSystem.Gravity = settings.Gravity;
        this.PhysicsSystem.OptimizeBroadPhase();
    }

    public void Update(float timeStep, int collisionSteps) {
        this.PhysicsSystem.Update(timeStep, collisionSteps, this._allocator, this._jobSystem);
    }
    
    public void Dispose() {
        if (this.HasDisposed) return;
        
        this.Dispose(true);
        GC.SuppressFinalize(this);
        this.HasDisposed = true;
    }
    
    protected virtual void Dispose(bool disposing) {
        if (disposing) {
            this._allocator.Dispose();
            this._jobSystem.Dispose();
            this._broadPhaseLayer.Dispose();
            this._broadPhaseLayerFilter.Dispose();
            this._objectLayerPairFilter.Dispose();
            this.PhysicsSystem.Dispose();
            Foundation.Shutdown();
        }
    }
}