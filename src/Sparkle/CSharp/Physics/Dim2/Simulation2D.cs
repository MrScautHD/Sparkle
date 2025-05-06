using Box2D;

namespace Sparkle.CSharp.Physics.Dim2;

public class Simulation2D : Simulation {

    /// <summary>
    /// The world in which the 2D physics simulation takes place.
    /// </summary>
    public readonly World World;
    
    /// <summary>
    /// The settings that configure the 2D physics simulation.
    /// </summary>
    private readonly PhysicsSettings2D _settings;
    
    /// <summary>
    /// Constructor for creating a Simulation2D object.
    /// </summary>
    /// <param name="settings">The physics settings for the 2D simulation.</param>
    public Simulation2D(PhysicsSettings2D settings) {
        this._settings = settings;
        this.World = new World(this._settings.WorldDef);
    }

    /// <summary>
    /// Advances the simulation by a fixed time step.
    /// </summary>
    /// <param name="fixedStep">The time step duration in seconds to advance the simulation.</param>
    protected internal override void Step(double fixedStep) {
        this.World.Step((float) fixedStep, this._settings.SubStepCount);
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            this.World.Destroy();
        }
    }
}