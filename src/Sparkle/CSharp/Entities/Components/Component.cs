using System.Numerics;
using Bliss.CSharp;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;

namespace Sparkle.CSharp.Entities.Components;

public abstract class Component : Disposable {
    
    /// <summary>
    /// Gets or sets the entity to which this component is attached.
    /// </summary>
    protected internal Entity Entity { get; internal set; }
    
    /// <summary>
    /// Gets the global position of the component, calculated as the entity's position plus the offset.
    /// </summary>
    public Vector3 GlobalPos => this.Entity.Transform.Translation + this.OffsetPos;
    
    /// <summary>
    /// The local offset position relative to the entity's transform.
    /// </summary>
    public Vector3 OffsetPos;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Component"/> class with a specified offset position.
    /// </summary>
    /// <param name="offsetPos">The offset position relative to the entity's transform.</param>
    protected Component(Vector3 offsetPos) {
        this.OffsetPos = offsetPos;
    }

    /// <summary>
    /// Called when the component is first initialized.
    /// </summary>
    protected internal virtual void Init() { }

    /// <summary>
    /// Called every frame to update the component's logic.
    /// </summary>
    /// <param name="delta">The time delta since the last update.</param>
    protected internal virtual void Update(double delta) {
    }

    /// <summary>
    /// Called after the main update phase to handle additional logic.
    /// </summary>
    /// <param name="delta">The time delta since the last update.</param>
    protected internal virtual void AfterUpdate(double delta) { }

    /// <summary>
    /// Called at fixed time intervals for physics-related updates.
    /// </summary>
    /// <param name="timeStep">The fixed time step interval for the update.</param>
    protected internal virtual void FixedUpdate(double timeStep) {
    }

    /// <summary>
    /// Called to render the component.
    /// </summary>
    /// <param name="context">The graphics context used for rendering.</param>
    protected internal virtual void Draw(GraphicsContext context) { }
    
    /// <summary>
    /// Called when the window is resized.
    /// </summary>
    /// <param name="rectangle">The rectangle specifying the window's updated size.</param>
    protected internal virtual void Resize(Rectangle rectangle) { }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            this.Entity.Components.Remove(this.GetType());
        }
    }
}