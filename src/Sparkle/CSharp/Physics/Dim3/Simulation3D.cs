using Jitter2;
using Jitter2.Collision;
using Jitter2.SoftBodies;

namespace Sparkle.CSharp.Physics.Dim3;

public class Simulation3D : Simulation {

    /// <summary>
    /// The physics world that manages all physical objects and interactions.
    /// </summary>
    public readonly World World;
    
    /// <summary>
    /// The physics settings used to configure the simulation.
    /// </summary>
    private readonly PhysicsSettings3D _settings;
    
    /// <summary>
    /// Constructor for creating a Simulation3D object.
    /// </summary>
    /// <param name="settings">The physics settings for the 3D simulation.</param>
    public Simulation3D(PhysicsSettings3D settings) {
        this._settings = settings;

        this.World = new World(settings.Capacity) {
            Gravity = settings.Gravity
        };
        
        this.World.DynamicTree.Filter = DynamicTreeCollisionFilter.Filter;
        this.World.BroadPhaseFilter = new BroadPhaseCollisionFilter(this.World);
        this.World.NarrowPhaseFilter = new TriangleEdgeCollisionFilter();
    }
    
    /// <summary>
    /// Executes a simulation step for the 3D physics world.
    /// </summary>
    /// <param name="fixedStep">The fixed time interval for the simulation step.</param>
    protected internal override void Step(double fixedStep) {
        this.World.Step((float) fixedStep, this._settings.MultiThreaded);
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            this.World.Clear();
        }
    }
}