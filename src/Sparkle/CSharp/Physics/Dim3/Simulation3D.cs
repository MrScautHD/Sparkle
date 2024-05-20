using Jitter2;
using Jitter2.Collision;
using Jitter2.SoftBodies;
using Sparkle.CSharp.Physics.Dim3.Conversions;

namespace Sparkle.CSharp.Physics.Dim3;

public class Simulation3D : Simulation {

    public readonly World World;

    private PhysicsSettings3D _settings;
    
    /// <summary>
    /// Initializes a new instance of the Simulation class with the specified physics settings.
    /// </summary>
    /// <param name="settings">The physics settings to configure the simulation.</param>
    public Simulation3D(PhysicsSettings3D settings) {
        this._settings = settings;
        
        this.World = new World(settings.MaxBodies, settings.MaxContacts, settings.MaxConstraints) {
            Gravity = PhysicsConversion.ToJVector(settings.Gravity),
            UseFullEPASolver = settings.UseFullEPASolver
        };
        
        this.World.DynamicTree.Filter = DynamicTreeCollisionFilter.Filter;
        this.World.BroadPhaseFilter = new BroadPhaseCollisionFilter(this.World);
        this.World.NarrowPhaseFilter = new TriangleEdgeCollisionFilter();
    }
    
    protected internal override void Step(float timeStep) {
        this.World.Step(timeStep, this._settings.MultiThreaded);
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            this.World.Clear();
        }
    }
}