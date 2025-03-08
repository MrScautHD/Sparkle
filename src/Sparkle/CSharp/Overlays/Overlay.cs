using Sparkle.CSharp.Graphics;

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
    protected internal virtual void Update() { }
    
    /// <summary>
    /// Executes logic after the update step.
    /// </summary>
    protected internal virtual void AfterUpdate() { }
    
    /// <summary>
    /// Executes fixed-step updates for the overlay.
    /// </summary>
    protected internal virtual void FixedUpdate() { }

    /// <summary>
    /// Draws the overlay.
    /// </summary>
    /// <param name="context">The graphics context used for rendering.</param>
    protected internal abstract void Draw(GraphicsContext context);
}