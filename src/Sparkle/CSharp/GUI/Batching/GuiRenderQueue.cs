using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Primitives;
using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Sprites;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.GUI.Batching.Commands;
using Sparkle.CSharp.GUI.Elements;
using Veldrith;

namespace Sparkle.CSharp.GUI.Batching;

public class GuiRenderQueue {
    
    /// <summary>
    /// The total number of draw calls issued since the last <see cref="Begin"/>.
    /// </summary>
    public int DrawCallCount { get; private set; }
    
    /// <summary>
    /// Indicates whether the <see cref="GuiRenderQueue"/> is currently in an active render pass.
    /// </summary>
    private bool _begun;
    
    /// <summary>
    /// The <see cref="GraphicsContext"/> used for rendering.
    /// </summary>
    private GraphicsContext _context;
    
    /// <summary>
    /// The target <see cref="Framebuffer"/> rendered into during the active render pass.
    /// </summary>
    private Framebuffer _framebuffer;
    
    /// <summary>
    /// The currently active batch type, used to determine when a flush is required on batch switches.
    /// </summary>
    private GuiBatchType _guiBatchType;
    
    /// <summary>
    /// The <see cref="SpriteGuiRenderState"/> currently applied to the <see cref="SpriteBatch"/>.
    /// Reset to <see cref="SpriteGuiRenderState.Default"/> whenever the active batch switches away from sprite.
    /// </summary>
    private SpriteGuiRenderState _currentSpriteRenderState;
    
    /// <summary>
    /// The <see cref="PrimitiveGuiRenderState"/> currently applied to the <see cref="PrimitiveBatch"/>.
    /// Reset to <see cref="PrimitiveGuiRenderState.Default"/> whenever the active batch switches away from primitive.
    /// </summary>
    private PrimitiveGuiRenderState _currentPrimitiveRenderState;
    
    /// <summary>
    /// The current global <see cref="GuiElement"/> render order, set before each element's draw call and used to compute the final sort key for local draw submissions.
    /// </summary>
    private int _currentElementRenderOrder;
    
    /// <summary>
    /// Queued sprite draw commands submitted via <see cref="SubmitSprite{TState}"/>, sorted and flushed during <see cref="End"/>.
    /// </summary>
    private readonly List<SpriteDrawCommand> _spriteCommands;
    
    /// <summary>
    /// Queued primitive draw commands submitted via <see cref="SubmitPrimitive{TState}"/>, sorted and flushed during <see cref="End"/>.
    /// </summary>
    private readonly List<PrimitiveDrawCommand> _primitiveCommands;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="GuiRenderQueue"/> class.
    /// </summary>
    public GuiRenderQueue() {
        this._spriteCommands = new List<SpriteDrawCommand>();
        this._primitiveCommands = new List<PrimitiveDrawCommand>();
    }
    
    /// <summary>
    /// Begins a new GUI render pass, initializing both the <see cref="SpriteBatch"/> and <see cref="PrimitiveBatch"/> and resetting all render state to their defaults.
    /// </summary>
    /// <param name="context">The <see cref="GraphicsContext"/> used for rendering.</param>
    /// <param name="framebuffer">The target <see cref="Framebuffer"/> to render into.</param>
    /// <exception cref="InvalidOperationException">Thrown if the <see cref="GuiRenderQueue"/> has already begun.</exception>
    public void Begin(GraphicsContext context, Framebuffer framebuffer) {
        if (this._begun) {
            throw new InvalidOperationException("The GuiRenderQueue has already begun.");
        }
        
        this._begun = true;
        this._context = context;
        this._framebuffer = framebuffer;
        this._guiBatchType = GuiBatchType.None;
        this._currentSpriteRenderState = SpriteGuiRenderState.Default;
        this._currentPrimitiveRenderState = PrimitiveGuiRenderState.Default;
        this._currentElementRenderOrder = 0;
        this._spriteCommands.Clear();
        this._primitiveCommands.Clear();
        
        this.DrawCallCount = 0;
        
        // Begin sprite/primitive batch.
        this._context.SpriteBatch.Begin(this._context.CommandList, this._framebuffer.OutputDescription);
        this._context.PrimitiveBatch.Begin(this._context.CommandList, this._framebuffer.OutputDescription);
    }
    
    /// <summary>
    /// Ends the current GUI render pass, flushing and finalizing both the <see cref="SpriteBatch"/> and <see cref="PrimitiveBatch"/> and accumulating their draw call counts into <see cref="DrawCallCount"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the <see cref="GuiRenderQueue"/> has not begun.</exception>
    public void End() {
        if (!this._begun) {
            throw new InvalidOperationException("The GuiRenderQueue has not begun.");
        }
        
        // Flush all pending commands.
        this.FlushCommands();
        
        // End sprite batch.
        this._context.SpriteBatch.End();
        this.DrawCallCount += this._context.SpriteBatch.DrawCallCount;
        
        // End primitive batch.
        this._context.PrimitiveBatch.End();
        this.DrawCallCount += this._context.PrimitiveBatch.DrawCallCount;
        
        this._begun = false;
    }
    
    /// <summary>
    /// Sets the current element render order used as the global sort key component for subsequent
    /// <see cref="SubmitSprite{TState}"/> and <see cref="SubmitPrimitive{TState}"/> calls.
    /// Called by <see cref="Gui"/> before invoking each element's draw method.
    /// </summary>
    /// <param name="renderOrder">The render order of the element about to be drawn.</param>
    internal void SetCurrentElementRenderOrder(int renderOrder) {
        this._currentElementRenderOrder = renderOrder;
    }
    
    /// <summary>
    /// Queues a sprite draw command with a local order offset relative to the current element's render order.
    /// All queued sprite and primitive commands are merged, sorted by <c>(RenderOrder * 1000) + localOrder</c>,
    /// and executed together during <see cref="End"/>.
    /// </summary>
    /// <typeparam name="TState">The type of the caller-supplied state passed to <paramref name="draw"/>.</typeparam>
    /// <param name="localOrder">The local draw order offset within this element. Lower values are drawn first. For example, pass <c>0</c> for a background texture and <c>1</c> for label text so all backgrounds across all elements batch together before all labels.</param>
    /// <param name="draw">
    /// A delegate receiving the active <see cref="SpriteBatch"/> and the forwarded <paramref name="state"/>.
    /// Use a static lambda and pass all required data via <paramref name="state"/> to avoid closure captures.
    /// </param>
    /// <param name="state">The state value forwarded to <paramref name="draw"/>.</param>
    /// <param name="renderState">An optional <see cref="SpriteGuiRenderState"/> override for this command.</param>
    /// <exception cref="InvalidOperationException">Thrown if the <see cref="GuiRenderQueue"/> has not begun.</exception>
    public void SubmitSprite<TState>(int localOrder, Action<SpriteBatch, TState> draw, TState state, SpriteGuiRenderState? renderState = null) {
        if (!this._begun) {
            throw new InvalidOperationException("The GuiRenderQueue has not begun.");
        }
        
        this._spriteCommands.Add(new SpriteDrawCommand(this._currentElementRenderOrder, localOrder, renderState ?? SpriteGuiRenderState.Default, batch => draw(batch, state)));
    }
    
    /// <summary>
    /// Queues a primitive draw command with a local order offset relative to the current element's render order.
    /// All queued sprite and primitive commands are merged, sorted by <c>(RenderOrder * 1000) + localOrder</c>,
    /// and executed together during <see cref="End"/>.
    /// </summary>
    /// <typeparam name="TState">The type of the caller-supplied state passed to <paramref name="draw"/>.</typeparam>
    /// <param name="localOrder">The local draw order offset within this element. Lower values are drawn first. For example, pass <c>0</c> for a background shape and <c>1</c> for an overlay so all backgrounds batch together before overlays.</param>
    /// <param name="draw">
    /// A delegate receiving the active <see cref="PrimitiveBatch"/> and the forwarded <paramref name="state"/>.
    /// Use a static lambda and pass all required data via <paramref name="state"/> to avoid closure captures.
    /// </param>
    /// <param name="state">The state value forwarded to <paramref name="draw"/>.</param>
    /// <param name="renderState">An optional <see cref="PrimitiveGuiRenderState"/> override for this command.</param>
    /// <exception cref="InvalidOperationException">Thrown if the <see cref="GuiRenderQueue"/> has not begun.</exception>
    public void SubmitPrimitive<TState>(int localOrder, Action<PrimitiveBatch, TState> draw, TState state, PrimitiveGuiRenderState? renderState = null) {
        if (!this._begun) {
            throw new InvalidOperationException("The GuiRenderQueue has not begun.");
        }
        
        this._primitiveCommands.Add(new PrimitiveDrawCommand(this._currentElementRenderOrder, localOrder, renderState ?? PrimitiveGuiRenderState.Default, batch => draw(batch, state)));
    }
    
    /// <summary>
    /// Ends both the <see cref="SpriteBatch"/> and <see cref="PrimitiveBatch"/>, executes the given draw action
    /// immediately, then restarts both batches — preserving render order between batched and direct draw calls.
    /// The <paramref name="state"/> parameter is passed through to <paramref name="draw"/> without allocation, avoiding closure captures.
    /// </summary>
    /// <typeparam name="TState">The type of the caller-supplied state passed to <paramref name="draw"/>.</typeparam>
    /// <param name="draw"> A static delegate that performs the direct draw call. Must not capture any variables — pass all required state via <paramref name="state"/> to avoid heap allocation.</param>
    /// <param name="state">The state value forwarded to <paramref name="draw"/>.</param>
    /// <exception cref="InvalidOperationException">Thrown if the <see cref="GuiRenderQueue"/> has not begun.</exception>
    public void SubmitDirect<TState>(Action<GraphicsContext, Framebuffer, TState> draw, TState state) {
        if (!this._begun) {
            throw new InvalidOperationException("The GuiRenderQueue has not begun.");
        }
        
        // Flush all pending commands.
        this.FlushCommands();
        
        // End sprite batch.
        this._context.SpriteBatch.End();
        this.DrawCallCount += this._context.SpriteBatch.DrawCallCount;
        
        // End primitive batch.
        this._context.PrimitiveBatch.End();
        this.DrawCallCount += this._context.PrimitiveBatch.DrawCallCount;
        
        // Reset GUI batch type.
        this._guiBatchType = GuiBatchType.None;
        this._currentSpriteRenderState = SpriteGuiRenderState.Default;
        this._currentPrimitiveRenderState = PrimitiveGuiRenderState.Default;
        
        // Execute the direct draw call.
        draw(this._context, this._framebuffer, state);
        
        // Begin sprite/primitive batch.
        this._context.SpriteBatch.Begin(this._context.CommandList, this._framebuffer.OutputDescription);
        this._context.PrimitiveBatch.Begin(this._context.CommandList, this._framebuffer.OutputDescription);
    }
    
    /// <summary>
    /// Sorts all pending sprite and primitive commands by <see cref="GuiElement.RenderOrder"/> then local order,
    /// and executes them via an interleaved merge — switching between <see cref="SpriteBatch"/> and <see cref="PrimitiveBatch"/> only when the next command requires it.
    /// </summary>
    private void FlushCommands() {
        if (this._spriteCommands.Count == 0 && this._primitiveCommands.Count == 0) {
            return;
        }
        
        this._spriteCommands.Sort(static (a, b) => {
            int result = a.RenderOrder.CompareTo(b.RenderOrder);
            return result != 0 ? result : a.LocalOrder.CompareTo(b.LocalOrder);
        });
        
        this._primitiveCommands.Sort(static (a, b) => {
            int result = a.RenderOrder.CompareTo(b.RenderOrder);
            return result != 0 ? result : a.LocalOrder.CompareTo(b.LocalOrder);
        });
        
        int spriteIndex = 0;
        int primitiveIndex = 0;
        
        while (spriteIndex < this._spriteCommands.Count || primitiveIndex < this._primitiveCommands.Count) {
            bool hasSpriteCommand = spriteIndex < this._spriteCommands.Count;
            bool hasPrimitiveCommand = primitiveIndex < this._primitiveCommands.Count;
            
            bool pickSprite;
            
            if (hasSpriteCommand && !hasPrimitiveCommand) {
                pickSprite = true;
            }
            else if (!hasSpriteCommand && hasPrimitiveCommand) {
                pickSprite = false;
            }
            else {
                SpriteDrawCommand nextSprite = this._spriteCommands[spriteIndex];
                PrimitiveDrawCommand nextPrimitive = this._primitiveCommands[primitiveIndex];
                
                int renderOrderDiff = nextSprite.RenderOrder.CompareTo(nextPrimitive.RenderOrder);
                
                if (renderOrderDiff != 0) {
                    pickSprite = renderOrderDiff < 0;
                }
                else {
                    pickSprite = nextSprite.LocalOrder <= nextPrimitive.LocalOrder;
                }
            }
            
            if (pickSprite) {
                SpriteDrawCommand command = this._spriteCommands[spriteIndex++];
                
                if (this._guiBatchType == GuiBatchType.Primitive) {
                    this._context.PrimitiveBatch.Flush();
                }
                
                if (this._guiBatchType != GuiBatchType.Sprite) {
                    this._currentSpriteRenderState = SpriteGuiRenderState.Default;
                }
                
                this._guiBatchType = GuiBatchType.Sprite;
                this.ApplySpriteState(command.RenderState);
                command.Execute(this._context.SpriteBatch);
            }
            else {
                PrimitiveDrawCommand command = this._primitiveCommands[primitiveIndex++];
                
                if (this._guiBatchType == GuiBatchType.Sprite) {
                    this._context.SpriteBatch.Flush();
                }
                
                if (this._guiBatchType != GuiBatchType.Primitive) {
                    this._currentPrimitiveRenderState = PrimitiveGuiRenderState.Default;
                }
                
                this._guiBatchType = GuiBatchType.Primitive;
                this.ApplyPrimitiveState(command.RenderState);
                command.Execute(this._context.PrimitiveBatch);
            }
        }
        
        this._spriteCommands.Clear();
        this._primitiveCommands.Clear();
    }
    
    /// <summary>
    /// Applies the given <see cref="SpriteGuiRenderState"/> to the <see cref="SpriteBatch"/> using its Push/Pop API.
    /// Each field is pushed when a non-<c>null</c> value is present, or popped back to the batch default otherwise.
    /// </summary>
    /// <param name="state">The <see cref="SpriteGuiRenderState"/> to apply.</param>
    private void ApplySpriteState(SpriteGuiRenderState state) {
        SpriteBatch batch = this._context.SpriteBatch;
        
        if (state.Sampler != null) {
            batch.PushSampler(state.Sampler);
        }
        else {
            batch.PopSampler();
        }
        
        if (state.Effect != null) {
            batch.PushEffect(state.Effect);
        }
        else {
            batch.PopEffect();
        }
        
        if (state.BlendState.HasValue) {
            batch.PushBlendState(state.BlendState.Value);
        }
        else {
            batch.PopBlendState();
        }
        
        if (state.DepthStencilState.HasValue) {
            batch.PushDepthStencilState(state.DepthStencilState.Value);
        }
        else {
            batch.PopDepthStencilState();
        }
        
        if (state.RasterizerState.HasValue) {
            batch.PushRasterizerState(state.RasterizerState.Value);
        }
        else {
            batch.PopRasterizerState();
        }
        
        if (state.ScissorRect.HasValue) {
            batch.PushScissorRect(state.ScissorRect.Value);
        }
        else {
            batch.PopScissorRect();
        }
    }
    
    /// <summary>
    /// Applies the given <see cref="PrimitiveGuiRenderState"/> to the <see cref="PrimitiveBatch"/> using its Push/Pop API.
    /// Each field is pushed when a non-<c>null</c> value is present, or popped back to the batch default otherwise.
    /// </summary>
    /// <param name="state">The <see cref="PrimitiveGuiRenderState"/> to apply.</param>
    private void ApplyPrimitiveState(PrimitiveGuiRenderState state) {
        PrimitiveBatch batch = this._context.PrimitiveBatch;
        
        if (state.Effect != null) {
            batch.PushEffect(state.Effect);
        }
        else {
            batch.PopEffect();
        }
        
        if (state.BlendState.HasValue) {
            batch.PushBlendState(state.BlendState.Value);
        }
        else {
            batch.PopBlendState();
        }
        
        if (state.DepthStencilState.HasValue) {
            batch.PushDepthStencilState(state.DepthStencilState.Value);
        }
        else {
            batch.PopDepthStencilState();
        }
        
        if (state.RasterizerState.HasValue) {
            batch.PushRasterizerState(state.RasterizerState.Value);
        }
        else {
            batch.PopRasterizerState();
        }
        
        if (state.ScissorRect.HasValue) {
            batch.PushScissorRect(state.ScissorRect.Value);
        }
        else {
            batch.PopScissorRect();
        }
    }
}