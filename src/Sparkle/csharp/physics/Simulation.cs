using System.Numerics;
using JoltPhysicsSharp;
using Sparkle.csharp.physics.layers;

namespace Sparkle.csharp.physics; 

public class Simulation : Disposable {

    public PhysicsSystem PhysicsSystem { get; private set; }
    public PhysicsSettings Settings { get; private set; }
    
    private TempAllocator _allocator;
    private JobSystemThreadPool _jobSystem;

    private BroadPhaseLayerInterfaceImpl _broadPhaseLayer;
    private ObjectVsBroadPhaseLayerFilterImpl _broadPhaseLayerFilter;
    private ObjectLayerPairFilterImpl _objectLayerPairFilter;

    private BroadPhaseLayerFilterImpl _broadPhaseLayerFilterImpl;
    private ObjectLayerFilterImpl _objectLayerFilterImpl;
    private BodyFilterImpl _bodyFilterImpl;

    public Simulation(PhysicsSettings settings) {
        Foundation.Init();
        
        this.Settings = settings;
        
        this._allocator = new TempAllocator(10 * (int) settings.MaxContactConstraints * (int) settings.MaxContactConstraints);
        this._jobSystem = new JobSystemThreadPool(Foundation.MaxPhysicsJobs, Foundation.MaxPhysicsBarriers);
        
        this._broadPhaseLayer = new BroadPhaseLayerInterfaceImpl();
        this._broadPhaseLayerFilter = new ObjectVsBroadPhaseLayerFilterImpl();
        this._objectLayerPairFilter = new ObjectLayerPairFilterImpl();
        
        this._broadPhaseLayerFilterImpl = new BroadPhaseLayerFilterImpl();
        this._objectLayerFilterImpl = new ObjectLayerFilterImpl();
        this._bodyFilterImpl = new BodyFilterImpl();
        
        this.PhysicsSystem = new PhysicsSystem();
        this.PhysicsSystem.Init(settings.MaxBodies, settings.NumBodyMutexes, settings.MaxBodyPairs, settings.MaxContactConstraints, this._broadPhaseLayer, this._broadPhaseLayerFilter, this._objectLayerPairFilter);
        this.PhysicsSystem.Gravity = settings.Gravity;
        this.PhysicsSystem.OptimizeBroadPhase();
    }

    public void Update(float timeStep, int collisionSteps) {
        this.PhysicsSystem.Update(timeStep, collisionSteps, this._allocator, this._jobSystem);
    }

    public bool RayCast(Vector3 origin, out RayCastResult result, Vector3 direction, float distance) {
        result = RayCastResult.Default;
        
        return this.PhysicsSystem.NarrowPhaseQuery.CastRay((Double3) origin, direction * distance, ref result, this._broadPhaseLayerFilterImpl, this._objectLayerFilterImpl, this._bodyFilterImpl);
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            Foundation.Shutdown();
            this._allocator.Dispose();
            this._jobSystem.Dispose();
            this._broadPhaseLayer.Dispose();
            this._broadPhaseLayerFilter.Dispose();
            this._broadPhaseLayerFilterImpl.Dispose();
            this._objectLayerFilterImpl.Dispose();
            this._objectLayerPairFilter.Dispose();
            this._bodyFilterImpl.Dispose();
            this.PhysicsSystem.Dispose();
        }
    }
}