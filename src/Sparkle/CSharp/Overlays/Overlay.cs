using Bliss.CSharp;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Veldrith;

namespace Sparkle.CSharp.Overlays;

public abstract class Overlay : Disposable {
    
    /// <summary>
    /// Gets the name of the overlay.
    /// </summary>
    public string Name { get; private set; }
    
    /// <summary>
    /// Determines whether camera movement is allowed while the Overlay is active.
    /// </summary>
    public bool AllowCameraMovement;
    
    /// <summary>
    /// Whether this overlay is active.
    /// </summary>
    public bool Enabled {
        get;
        set {
            field = value;
            this.StateChanged?.Invoke(this, value);
        }
    }
    
    /// <summary>
    /// Event raised when the overlay's enabled state changes.
    /// </summary>
    public event Action<Overlay, bool>? StateChanged;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Overlay"/> class.
    /// </summary>
    /// <param name="name">The name of the overlay.</param>
    /// <param name="enabled">Whether the overlay is initially enabled (default is false).</param>
    protected Overlay(string name, bool enabled = false) {
        this.Name = name;
        this.AllowCameraMovement = true;
        this.Enabled = enabled;
    }
    
    /// <summary>
    /// Updates the overlay each frame.
    /// </summary>
    /// <param name="delta">The time delta since the last update.</param>
    protected internal virtual void Update(double delta) { }
    
    /// <summary>
    /// Executes logic after the update step.
    /// </summary>
    /// <param name="delta">The time delta since the last update.</param>
    protected internal virtual void AfterUpdate(double delta) { }
    
    /// <summary>
    /// Executes fixed-step updates for the overlay.
    /// </summary>
    /// <param name="timeStep">The fixed time step interval for the update.</param>
    protected internal virtual void FixedUpdate(double timeStep) { }
    
    /// <summary>
    /// Draws the overlay. Called once per frame while the overlay is enabled,
    /// after the scene has been drawn and before the GUI.
    /// </summary>
    /// <param name="context">The graphics context used for rendering.</param>
    /// <param name="framebuffer">The framebuffer to which the overlay is rendered.</param>
    protected internal abstract void Draw(GraphicsContext context, Framebuffer framebuffer);
    
    /// <summary>
    /// Executes logic after the overlay and the GUI have been drawn, and before the ImGui overlays are drawn.
    /// Useful for rendering on top of the GUI.
    /// </summary>
    /// <param name="context">The graphics context used for rendering.</param>
    /// <param name="framebuffer">The framebuffer to which the overlay is rendered.</param>
    protected internal virtual void AfterDraw(GraphicsContext context, Framebuffer framebuffer) { }
    
    /// <summary>
    /// Executes when the window is resized.
    /// </summary>
    /// <param name="rectangle">The rectangle specifying the window's updated size.</param>
    protected internal virtual void Resize(Rectangle rectangle) { }
}