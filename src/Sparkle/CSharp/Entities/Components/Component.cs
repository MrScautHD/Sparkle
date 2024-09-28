using System.Numerics;

namespace Sparkle.CSharp.Entities.Components;

public abstract class Component : Disposable {
    
    protected internal Entity Entity { get; internal set; }

    public Vector3 GlobalPos => this.Entity.Position + this.OffsetPos;
    public Vector3 OffsetPos;
    
    public bool HasInitialized { get; private set; }
    
    /// <summary>
    /// Constructor for creating a Component object.
    /// </summary>
    /// <param name="offsetPos">Offset position of the component.</param>
    public Component(Vector3 offsetPos) {
        this.OffsetPos = offsetPos;
    }
    
    /// <summary>
    /// Used for Initializes objects.
    /// </summary>
    protected internal virtual void Init() {
        this.HasInitialized = true;
    }

    /// <summary>
    /// Is invoked during each tick and is used for updating dynamic elements and game logic.
    /// </summary>
    protected internal virtual void Update() { }
    
    /// <summary>
    /// Called after the Update method on each tick to further update dynamic elements and game logic.
    /// </summary>
    protected internal virtual void AfterUpdate() { }
    
    /// <summary>
    /// Is invoked at a fixed rate of every <see cref="GameSettings.FixedTimeStep"/> frames following the <see cref="AfterUpdate"/> method.
    /// It is used for handling physics and other fixed-time operations.
    /// </summary>
    protected internal virtual void FixedUpdate() { }
    
    /// <summary>
    /// Is called every tick, used for rendering stuff.
    /// </summary>
    protected internal virtual void Draw() { }

    /// <summary>
    /// Creates a copy of the current component instance.
    /// </summary>
    /// <returns>A new instance of the <see cref="Component"/> that is a clone of the original.</returns>
    public abstract Component Clone();
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            this.Entity.Components.Remove(this.GetType());
        }
    }
}