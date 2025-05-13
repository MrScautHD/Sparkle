using Box2D;

namespace Sparkle.CSharp.Physics.Dim2;

public class Simulation2D : Simulation {
    
    /// <summary>
    /// The world in which the 2D physics simulation takes place.
    /// </summary>
    public readonly World World;
    
    /// <summary>
    /// Triggered when a body has moved during the physics simulation step.
    /// </summary>
    public event Action<BodyMoveEvent>? BodyMoved;
    
    /// <summary>
    /// Triggered when a sensor begins touching another object.
    /// </summary>
    public event Action<SensorBeginTouchEvent>? SensorBeginTouch;
    
    /// <summary>
    /// Triggered when a sensor stops touching another object.
    /// </summary>
    public event Action<SensorEndTouchEvent>? SensorEndTouch;
    
    /// <summary>
    /// Triggered when contact between two objects begins.
    /// </summary>
    public event Action<ContactBeginTouchEvent>? ContactBeginTouch;
    
    /// <summary>
    /// Triggered when contact between two objects ends.
    /// </summary>
    public event Action<ContactEndTouchEvent>? ContactEndTouch;
    
    /// <summary>
    /// Triggered when a contact hit occurs between two colliding bodies.
    /// </summary>
    public event Action<ContactHitEvent>? ContactHit;
    
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
        this.World.Step((float) fixedStep, this._settings.SubStepCount, this._settings.ParallelEvents);
        
        foreach (BodyMoveEvent moveEvent in this.World.BodyEvents.MoveEvents) {
            this.BodyMoved?.Invoke(moveEvent);
        }
        
        foreach (SensorBeginTouchEvent sensorBeginTouchEvent in this.World.SensorEvents.BeginEvents) {
            this.SensorBeginTouch?.Invoke(sensorBeginTouchEvent);
        }
        
        foreach (SensorEndTouchEvent sensorEndTouchEvent in this.World.SensorEvents.EndEvents) {
            this.SensorEndTouch?.Invoke(sensorEndTouchEvent);
        }
        
        foreach (ContactBeginTouchEvent contactBeginTouchEvent in this.World.ContactEvents.BeginEvents) {
            this.ContactBeginTouch?.Invoke(contactBeginTouchEvent);
        }
        
        foreach (ContactEndTouchEvent contactEndTouchEvent in this.World.ContactEvents.EndEvents) {
            this.ContactEndTouch?.Invoke(contactEndTouchEvent);
        }
        
        foreach (ContactHitEvent contactHitEvent in this.World.ContactEvents.HitEvents) {
            this.ContactHit?.Invoke(contactHitEvent);
        }
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            this.World.Destroy();
        }
    }
}