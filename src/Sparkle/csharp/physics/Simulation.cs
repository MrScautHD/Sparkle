using JoltPhysicsSharp;
using Sparkle.csharp.physics.layers;

namespace Sparkle.csharp.physics; 

public class Simulation {

    public PhysicsSystem PhysicsSystem { get; private set; }
    public PhysicsSettings Settings { get; private set; }
    
    private TempAllocator _allocator;
    private JobSystemThreadPool _jobSystem;

    private BroadPhaseLayerInterfaceImpl _broadPhaseLayer;
    private ObjectVsBroadPhaseLayerFilterImpl _broadPhaseLayerFilter;
    private ObjectLayerPairFilterImpl _objectLayerPairFilter;

    public Simulation(PhysicsSettings settings) {
        this.Settings = settings;
        
        this._allocator = new TempAllocator(10 * 1024 * 1024);
        this._jobSystem = new JobSystemThreadPool(Foundation.MaxPhysicsJobs, Foundation.MaxPhysicsBarriers);
        
        this._broadPhaseLayer = new BroadPhaseLayerInterfaceImpl();
        this._broadPhaseLayerFilter = new ObjectVsBroadPhaseLayerFilterImpl();
        this._objectLayerPairFilter = new ObjectLayerPairFilterImpl();
        
        this.PhysicsSystem = new PhysicsSystem();
        this.PhysicsSystem.Init(settings.MaxBodies, settings.NumBodyMutexes, settings.MaxBodyPairs, settings.MaxContactConstraints, this._broadPhaseLayer, this._broadPhaseLayerFilter, this._objectLayerPairFilter);
        this.PhysicsSystem.Gravity = settings.Gravity;
    }

    public void Update(float timeStep, int collisionSteps) {
        this.PhysicsSystem.Update(timeStep, collisionSteps, this._allocator, this._jobSystem);
    }
}