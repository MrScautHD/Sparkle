namespace Sparkle.csharp.entity.components; 

public abstract class Component : IDisposable {
    
    protected internal Entity Entity { get; internal set; }

    /// <summary>
    /// Used for Initializes objects.
    /// </summary>
    protected internal virtual void Init() { }

    /// <summary>
    /// Is invoked during each tick and is used for updating dynamic elements and game logic.
    /// </summary>
    protected internal virtual void Update() { }
    
    /// <summary>
    /// Is invoked at a fixed rate of every 60 frames following the <see cref="Update"/> method.
    /// It is used for handling physics and other fixed-time operations.
    /// </summary>
    protected internal virtual void FixedUpdate() { }
    
    /// <summary>
    /// Is called every tick, used for rendering stuff.
    /// </summary>
    protected internal virtual void Draw() { }
    
    public virtual void Dispose() { }
}