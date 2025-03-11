using System.Numerics;
using Bliss.CSharp;
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
    protected internal virtual void Update() { }
    
    /// <summary>
    /// Called after the main update phase to handle additional logic.
    /// </summary>
    protected internal virtual void AfterUpdate() { }
    
    /// <summary>
    /// Called at fixed time intervals for physics-related updates.
    /// </summary>
    protected internal virtual void FixedUpdate() { }

    /// <summary>
    /// Called to render the component.
    /// </summary>
    /// <param name="context">The graphics context used for rendering.</param>
    protected internal virtual void Draw(GraphicsContext context) { }
}