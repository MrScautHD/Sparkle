namespace Sparkle.csharp.overlay; 

public abstract class Overlay : IDisposable {
    
    public readonly string Name;
    public bool Enabled;

    public Overlay(string name) {
        this.Name = name;
        OverlayManager.Overlays.Add(this);
    }
    
    /// <summary>
    /// Used for Initializes objects.
    /// </summary>
    protected internal virtual void Init() { }
    
    /// <summary>
    /// Is invoked during each tick and is used for updating dynamic elements and game logic.
    /// </summary>
    protected internal virtual void Update() { }

    /// <summary>
    /// Is invoked at a fixed rate of every <see cref="GameSettings.FixedTimeStep"/> frames following the <see cref="Update"/> method.
    /// It is used for handling physics and other fixed-time operations.
    /// </summary>
    protected internal virtual void FixedUpdate() { }

    /// <summary>
    /// Is called every tick, used for rendering stuff.
    /// </summary>
    protected internal abstract void Draw();
    
    public virtual void Dispose() { }
}