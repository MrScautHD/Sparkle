using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Veldrid;

namespace Sparkle.CSharp.Overlays;

public abstract class Overlay {

    /// <summary>
    /// Gets the name of the overlay.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Determines whether the overlay is enabled.
    /// </summary>
    public bool Enabled;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Overlay"/> class.
    /// </summary>
    /// <param name="name">The name of the overlay.</param>
    /// <param name="enabled">Whether the overlay is initially enabled (default is false).</param>
    protected Overlay(string name, bool enabled = false) {
        this.Name = name;
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
    /// Draws the overlay.
    /// </summary>
    /// <param name="context">The graphics context used for rendering.</param>
    /// <param name="framebuffer">The framebuffer to which the overlay is rendered.</param>
    protected internal abstract void Draw(GraphicsContext context, Framebuffer framebuffer);

    /// <summary>
    /// Executes when the window is resized.
    /// </summary>
    /// <param name="rectangle">The rectangle specifying the window's updated size.</param>
    protected internal virtual void Resize(Rectangle rectangle) {}
}