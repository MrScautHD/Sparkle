using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.World;

namespace Sparkle.CSharp.Physics.Dim2;

public class Simulation2D : Simulation {

    public readonly World World;
    private readonly PhysicsSettings2D _settings;
    
    /// <summary>
    /// Constructor for creating a Simulation2D object.
    /// </summary>
    /// <param name="settings">The physics settings for the 2D simulation.</param>
    public Simulation2D(PhysicsSettings2D settings) {
        this._settings = settings;
        this.World = new World(this._settings.Gravity);
    }

    protected internal override void Step(float timeStep) {
        this.World.Step(timeStep, this._settings.VelocityIterations, this._settings.PositionIterations);
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            for (Body body = this.World.GetBodyList(); body != null; body = body.GetNext()) {
                this.World.DestroyBody(body);
            }
        }
    }
}