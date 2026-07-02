using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Primitives;
using Sparkle.CSharp.GUI.Elements;

namespace Sparkle.CSharp.GUI.Batching.Commands;

internal readonly struct PrimitiveDrawCommand {
    
    /// <summary>
    /// The global render order of the owning <see cref="GuiElement"/>, used as the primary sort key.
    /// </summary>
    public readonly int RenderOrder;
    
    /// <summary>
    /// The local draw order offset within the owning element, used as the secondary sort key.
    /// </summary>
    public readonly int LocalOrder;
    
    /// <summary>
    /// The <see cref="PrimitiveGuiRenderState"/> to apply before executing this command.
    /// </summary>
    public readonly PrimitiveGuiRenderState RenderState;
    
    /// <summary>
    /// The draw action to execute against the <see cref="PrimitiveBatch"/>.
    /// </summary>
    private readonly Action<PrimitiveBatch> _draw;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="PrimitiveDrawCommand"/> struct.
    /// </summary>
    /// <param name="renderOrder">The global render order of the owning element.</param>
    /// <param name="localOrder">The local draw order offset within the owning element.</param>
    /// <param name="renderState">The render state to apply before drawing.</param>
    /// <param name="draw">The draw action to execute.</param>
    public PrimitiveDrawCommand(int renderOrder, int localOrder, PrimitiveGuiRenderState renderState, Action<PrimitiveBatch> draw) {
        this.RenderOrder = renderOrder;
        this.LocalOrder = localOrder;
        this.RenderState = renderState;
        this._draw = draw;
    }
    
    /// <summary>
    /// Executes this draw command against the given <see cref="PrimitiveBatch"/>.
    /// </summary>
    /// <param name="batch">The <see cref="PrimitiveBatch"/> to draw into.</param>
    public void Execute(PrimitiveBatch batch) {
        this._draw(batch);
    }
}