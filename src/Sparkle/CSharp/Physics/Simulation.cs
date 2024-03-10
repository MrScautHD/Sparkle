using Jitter2;
using Jitter2.Collision;
using Jitter2.SoftBodies;
using Sparkle.CSharp.Physics.Conversions;

namespace Sparkle.CSharp.Physics;

public class Simulation : Disposable {

    public readonly World World;

    private PhysicsSettings _settings;
    
    /// <summary>
    /// Initializes a new instance of the Simulation class with the specified physics settings.
    /// </summary>
    /// <param name="settings">The physics settings to configure the simulation.</param>
    public Simulation(PhysicsSettings settings) {
        this._settings = settings;
        
        this.World = new World(settings.MaxBodies, settings.MaxContacts, settings.MaxConstraints) {
            Gravity = PhysicsConversion.ToJVector(settings.Gravity),
            UseFullEPASolver = settings.UseFullEPASolver
        };
        
        this.World.DynamicTree.Filter = DynamicTreeCollisionFilter.Filter;
        this.World.BroadPhaseFilter = new BroadPhaseCollisionFilter(this.World);
        this.World.NarrowPhaseFilter = new TriangleEdgeCollisionFilter();
    }

    /// <summary>
    /// Performs a single step in the physics simulation based on the given time step and multi-threading flag.
    /// </summary>
    /// <param name="timeStep">The duration of the step in seconds.</param>
    protected internal void Step(float timeStep) {
        this.World.Step(timeStep, this._settings.MultiThreaded);
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            this.World.Clear();
        }
    }
}