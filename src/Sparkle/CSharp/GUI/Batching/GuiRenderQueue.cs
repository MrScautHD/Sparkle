using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Primitives;
using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Sprites;
using Sparkle.CSharp.Graphics;
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
        
        // End sprite batch.
        this._context.SpriteBatch.End();
        this.DrawCallCount += this._context.SpriteBatch.DrawCallCount;
        
        // End primitive batch.
        this._context.PrimitiveBatch.End();
        this.DrawCallCount += this._context.PrimitiveBatch.DrawCallCount;
        
        this._begun = false;
    }
    
    /// <summary>
    /// Switches the active batch to <see cref="SpriteBatch"/>, flushing the <see cref="PrimitiveBatch"/> if it
    /// was previously active, and applies the resolved <see cref="SpriteGuiRenderState"/> via Push/Pop.
    /// When <paramref name="state"/> is non-<c>null</c> it fully overrides the current state; when <c>null</c>
    /// the last active <see cref="SpriteGuiRenderState"/> is reused, or <see cref="SpriteGuiRenderState.Default"/>
    /// if the batch type has just switched.
    /// </summary>
    /// <param name="state">
    /// An optional <see cref="SpriteGuiRenderState"/> override, or <c>null</c> to retain the current state.
    /// </param>
    /// <returns>The <see cref="SpriteBatch"/> with the resolved state applied, ready to receive draw calls.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the <see cref="GuiRenderQueue"/> has not begun.</exception>
    public SpriteBatch UseSprite(SpriteGuiRenderState? state = null) {
        if (!this._begun) {
            throw new InvalidOperationException("The GuiRenderQueue has not begun.");
        }
        
        // Flush primitive batch.
        if (this._guiBatchType == GuiBatchType.Primitive) {
            this._context.PrimitiveBatch.Flush();
        }
        
        // Reset render state to default when a different batch type was used before.
        if (this._guiBatchType != GuiBatchType.Sprite) {
            this._currentSpriteRenderState = SpriteGuiRenderState.Default;
        }
        
        // Set sprite batch.
        this._guiBatchType = GuiBatchType.Sprite;
        
        // Apply render state.
        this.ApplySpriteState(state ?? this._currentSpriteRenderState);
        
        return this._context.SpriteBatch;
    }
    
    /// <summary>
    /// Switches the active batch to <see cref="PrimitiveBatch"/>, flushing the <see cref="SpriteBatch"/> if it
    /// was previously active, and applies the resolved <see cref="PrimitiveGuiRenderState"/> via Push/Pop.
    /// When <paramref name="state"/> is non-<c>null</c> it fully overrides the current state; when <c>null</c>
    /// the last active <see cref="PrimitiveGuiRenderState"/> is reused, or <see cref="PrimitiveGuiRenderState.Default"/>
    /// if the batch type has just switched.
    /// </summary>
    /// <param name="state">
    /// An optional <see cref="PrimitiveGuiRenderState"/> override, or <c>null</c> to retain the current state.
    /// </param>
    /// <returns>The <see cref="PrimitiveBatch"/> with the resolved state applied, ready to receive draw calls.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the <see cref="GuiRenderQueue"/> has not begun.</exception>
    public PrimitiveBatch UsePrimitive(PrimitiveGuiRenderState? state = null) {
        if (!this._begun) {
            throw new InvalidOperationException("The GuiRenderQueue has not begun.");
        }
        
        // Flush sprite batch.
        if (this._guiBatchType == GuiBatchType.Sprite) {
            this._context.SpriteBatch.Flush();
        }
        
        // Reset render state to default when a different batch type was used before.
        if (this._guiBatchType != GuiBatchType.Primitive) {
            this._currentPrimitiveRenderState = PrimitiveGuiRenderState.Default;
        }
        
        // Set primitive batch.
        this._guiBatchType = GuiBatchType.Primitive;
        
        // Apply render state.
        this.ApplyPrimitiveState(state ?? this._currentPrimitiveRenderState);
        
        return this._context.PrimitiveBatch;
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