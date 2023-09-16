namespace Sparkle.csharp.overlay; 

public abstract class Overlay : IDisposable {
    
    public readonly string Name;
    private bool _enabled;
    
    public bool HasInitialized { get; private set; }
    public bool HasDisposed { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Overlay"/>, setting its name and adding it to the OverlayManager's list of Overlays.
    /// </summary>
    /// <param name="name">The name of the Overlay instance.</param>
    public Overlay(string name) {
        this.Name = name;
        OverlayManager.Overlays.Add(this);
    }
    
    /// <summary>
    /// Gets or sets a value indicating whether the overlay is enabled.
    /// If set to true for the first time, initializes the overlay.
    /// Logs an error if the overlay is already disposed.
    /// </summary>
    public bool Enabled {
        get => this._enabled;
        
        set {
            this.ThrowIfDisposed();
            this._enabled = value;
        
            if (!this.HasInitialized) {
                this.Init();
                this.HasInitialized = true;
            }
        }
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
    /// Is invoked at a fixed rate of every <see cref="GameSettings.FixedTimeStep"/> frames following the <see cref="Update"/> method.
    /// It is used for handling physics and other fixed-time operations.
    /// </summary>
    protected internal virtual void FixedUpdate() { }

    /// <summary>
    /// Is called every tick, used for rendering stuff.
    /// </summary>
    protected internal abstract void Draw();

    public void Dispose() {
        if (this.HasDisposed) return;
        
        this.Dispose(true);
        GC.SuppressFinalize(this);
        this.HasDisposed = true;
    }
    
    protected virtual void Dispose(bool disposing) {
        if (disposing) {
            this._enabled = false;
            OverlayManager.Overlays.Remove(this); // TODO IDK
        }
    }
    
    public void ThrowIfDisposed() {
        if (this.HasDisposed) {
            throw new ObjectDisposedException(this.GetType().Name);
        }
    }
}