using System.Numerics;
using Bliss.CSharp.Graphics.Rendering.Batches.Primitives;
using Bliss.CSharp.Graphics.Rendering.Batches.Sprites;

namespace Sparkle.CSharp.Entities.Components;

public abstract class BatchComponent : InterpolatedComponent {
    
    /// <summary>
    /// The order type that determines whether sprites or primitives are drawn first in the batch.
    /// </summary>
    public BatchOrderType OrderType { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchComponent"/> class.
    /// </summary>
    /// <param name="offsetPosition">The local offset position of the component.</param>
    /// <param name="orderType">The rendering order type for this batch component.</param>
    public BatchComponent(Vector3 offsetPosition, BatchOrderType orderType = BatchOrderType.DrawSpritesFirst) : base(offsetPosition) {
        this.OrderType = orderType;
    }

    /// <summary>
    /// Initializes the batch component and registers it with the scene's batch renderer.
    /// </summary>
    protected internal override void Init() {
        base.Init();
        this.Entity.Scene.BatchRenderer.Add(this);
    }

    /// <summary>
    /// Draws a sprite using the specified <see cref="SpriteBatch"/>.
    /// </summary>
    /// <param name="spriteBatch">The sprite batch used for rendering the sprite.</param>
    protected internal virtual void DrawSprite(SpriteBatch spriteBatch) { }

    /// <summary>
    /// Draws a primitive using the specified <see cref="PrimitiveBatch"/>.
    /// </summary>
    /// <param name="primitiveBatch">The primitive batch used for rendering the primitive.</param>
    protected internal virtual void DrawPrimitive(PrimitiveBatch primitiveBatch) { }

    /// <summary>
    /// Defines the rendering order options for the <see cref="BatchComponent"/>.
    /// </summary>
    public enum BatchOrderType {
        
        /// <summary>
        /// Sprites are rendered before primitives.
        /// </summary>
        DrawSpritesFirst,
        
        /// <summary>
        /// Primitives are rendered before sprites.
        /// </summary>
        DrawPrimitivesFirst
    }
    
    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (disposing) {
            this.Entity.Scene.BatchRenderer.Remove(this);
        }
    }
}