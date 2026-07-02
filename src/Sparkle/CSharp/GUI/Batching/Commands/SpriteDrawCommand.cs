using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Sprites;
using Sparkle.CSharp.GUI.Elements;

namespace Sparkle.CSharp.GUI.Batching.Commands;

internal readonly struct SpriteDrawCommand {
    
    /// <summary>
    /// The global render order of the owning <see cref="GuiElement"/>, used as the primary sort key.
    /// </summary>
    public readonly int RenderOrder;
    
    /// <summary>
    /// The local draw order offset within the owning element, used as the secondary sort key.
    /// </summary>
    public readonly int LocalOrder;
    
    /// <summary>
    /// The <see cref="SpriteGuiRenderState"/> to apply before executing this command.
    /// </summary>
    public readonly SpriteGuiRenderState RenderState;
    
    /// <summary>
    /// The draw action to execute against the <see cref="SpriteBatch"/>.
    /// </summary>
    private readonly Action<SpriteBatch> _draw;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SpriteDrawCommand"/> struct.
    /// </summary>
    /// <param name="renderOrder">The global render order of the owning element.</param>
    /// <param name="localOrder">The local draw order offset within the owning element.</param>
    /// <param name="renderState">The render state to apply before drawing.</param>
    /// <param name="draw">The draw action to execute.</param>
    public SpriteDrawCommand(int renderOrder, int localOrder, SpriteGuiRenderState renderState, Action<SpriteBatch> draw) {
        this.RenderOrder = renderOrder;
        this.LocalOrder = localOrder;
        this.RenderState = renderState;
        this._draw = draw;
    }
    
    /// <summary>
    /// Executes this draw command against the given <see cref="SpriteBatch"/>.
    /// </summary>
    /// <param name="batch">The <see cref="SpriteBatch"/> to draw into.</param>
    public void Execute(SpriteBatch batch) {
        this._draw(batch);
    }
}