using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.World;

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
        this.World = new World(this._settings.Gravity);
    }

    /// <summary>
    /// Performs a single step in the 2D physics simulation based on the given time step.
    /// </summary>
    /// <param name="timeStep">The duration of the step in seconds.</param>
    protected internal override void Step(double timeStep) {
        this.World.Step((float) timeStep, this._settings.VelocityIterations, this._settings.PositionIterations);
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            for (Body body = this.World.GetBodyList(); body != null; body = body.GetNext()) {
                this.World.DestroyBody(body);
            }
        }
    }
}