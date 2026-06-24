using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Primitives;
using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Sprites;
using Sparkle.CSharp.Graphics;
using Veldrith;

namespace Sparkle.CSharp.GUI.Batching;

public class GuiRenderQueue {
    
    /// <summary>
    /// The total number of draw calls (GPU submissions) issued since the last <see cref="Begin"/> call.
    /// Each <see cref="Flush"/> that had an active batch increments this by one.
    /// </summary>
    public int DrawCallCount { get; private set; }
    
    /// <summary>
    /// The number of times the active batch type or render state changed since the last <see cref="Begin"/> call.
    /// A high value relative to element count indicates poor state locality.
    /// </summary>
    public int BatchChangesCount { get; private set; }
    
    /// <summary>
    /// Indicates whether a batch operation has begun.
    /// </summary>
    private bool _begun;
    
    private GraphicsContext? _context;
    private Framebuffer? _framebuffer;
    
    private GuiBatchType _activeBatch;
    private GuiRenderState _activeState;
    
    private GuiRenderState _lastSpriteState;
    private GuiRenderState _lastPrimitiveState;
    
    private bool _pushedSpriteDepthStencil;
    private bool _pushedSpriteRasterizer;
    private bool _pushedSpriteScissor;
    
    private bool _pushedPrimitiveDepthStencil;
    private bool _pushedPrimitiveRasterizer;
    private bool _pushedPrimitiveScissor;
    
    public void Begin(GraphicsContext context, Framebuffer framebuffer) {
        if (this._begun) {
            throw new Exception("The Batch has already begun!");
        }
        
        this._begun = true;
        
        this._context = context;
        this._framebuffer = framebuffer;

        this._activeBatch = GuiBatchType.None;
        this._activeState = default;

        this._lastSpriteState = default;
        this._lastPrimitiveState = default;
        
        this.DrawCallCount = 0;
        this.BatchChangesCount = 0;

        this.ResetPushFlags();
    }

    public void End() {
        if (!this._begun) {
            throw new Exception("The SpriteBatch has not begun yet!");
        }
        
        this.Flush();
        this._begun = false;

        this._context = null;
        this._framebuffer = null;

        this._lastSpriteState = default;
        this._lastPrimitiveState = default;
    }
    
    public void SetSpriteState(GuiRenderState state) {
        this._lastSpriteState = state;
    }
    
    public void SetPrimitiveState(GuiRenderState state) {
        this._lastPrimitiveState = state;
    }
    
    public SpriteBatch UseSprite() {
        return this.UseSprite(this._lastSpriteState);
    }

    public SpriteBatch UseSprite(GuiRenderState state) {
        this._lastSpriteState = state;
        this.EnsureSprite(state);
        return this._context!.SpriteBatch;
    }

    public PrimitiveBatch UsePrimitive() {
        return this.UsePrimitive(this._lastPrimitiveState);
    }

    public PrimitiveBatch UsePrimitive(GuiRenderState state) {
        this._lastPrimitiveState = state;
        this.EnsurePrimitive(state);
        return this._context!.PrimitiveBatch;
    }

    public void Flush() {
        if (this._context == null) {
            return;
        }

        switch (this._activeBatch) {
            case GuiBatchType.Sprite:
                this.PopSpriteStates();
                this._context.SpriteBatch.End();
                this.DrawCallCount += this._context.SpriteBatch.DrawCallCount;
                break;

            case GuiBatchType.Primitive:
                this.PopPrimitiveStates();
                this._context.PrimitiveBatch.End();
                this.DrawCallCount += this._context.PrimitiveBatch.DrawCallCount;
                break;
        }

        this._activeBatch = GuiBatchType.None;
        this._activeState = default;
    }

    private void EnsureSprite(GuiRenderState state) {
        this.EnsureStarted();

        if (this._activeBatch == GuiBatchType.Sprite && this._activeState == state) {
            return;
        }

        this.Flush();
        this.BatchChangesCount++;

        this._context!.SpriteBatch.Begin(this._context.CommandList, this._framebuffer!.OutputDescription, state.Sampler, state.Effect, state.BlendState);

        if (state.DepthStencilState.HasValue) {
            this._context.SpriteBatch.PushDepthStencilState(state.DepthStencilState.Value);
            this._pushedSpriteDepthStencil = true;
        }

        if (state.RasterizerState.HasValue) {
            this._context.SpriteBatch.PushRasterizerState(state.RasterizerState.Value);
            this._pushedSpriteRasterizer = true;
        }

        if (state.ScissorRect.HasValue) {
            this._context.SpriteBatch.PushScissorRect(state.ScissorRect.Value);
            this._pushedSpriteScissor = true;
        }

        this._activeBatch = GuiBatchType.Sprite;
        this._activeState = state;
    }

    private void EnsurePrimitive(GuiRenderState state) {
        this.EnsureStarted();

        if (this._activeBatch == GuiBatchType.Primitive && this._activeState == state) {
            return;
        }

        this.Flush();
        this.BatchChangesCount++;

        this._context!.PrimitiveBatch.Begin(this._context.CommandList, this._framebuffer!.OutputDescription, state.Effect, state.BlendState);

        if (state.DepthStencilState.HasValue) {
            this._context.PrimitiveBatch.PushDepthStencilState(state.DepthStencilState.Value);
            this._pushedPrimitiveDepthStencil = true;
        }

        if (state.RasterizerState.HasValue) {
            this._context.PrimitiveBatch.PushRasterizerState(state.RasterizerState.Value);
            this._pushedPrimitiveRasterizer = true;
        }

        if (state.ScissorRect.HasValue) {
            this._context.PrimitiveBatch.PushScissorRect(state.ScissorRect.Value);
            this._pushedPrimitiveScissor = true;
        }

        this._activeBatch = GuiBatchType.Primitive;
        this._activeState = state;
    }

    private void PopSpriteStates() {
        if (this._context == null) {
            return;
        }

        if (this._pushedSpriteScissor) {
            this._context.SpriteBatch.PopScissorRect();
            this._pushedSpriteScissor = false;
        }

        if (this._pushedSpriteRasterizer) {
            this._context.SpriteBatch.PopRasterizerState();
            this._pushedSpriteRasterizer = false;
        }

        if (this._pushedSpriteDepthStencil) {
            this._context.SpriteBatch.PopDepthStencilState();
            this._pushedSpriteDepthStencil = false;
        }
    }

    private void PopPrimitiveStates() {
        if (this._context == null) {
            return;
        }

        if (this._pushedPrimitiveScissor) {
            this._context.PrimitiveBatch.PopScissorRect();
            this._pushedPrimitiveScissor = false;
        }

        if (this._pushedPrimitiveRasterizer) {
            this._context.PrimitiveBatch.PopRasterizerState();
            this._pushedPrimitiveRasterizer = false;
        }

        if (this._pushedPrimitiveDepthStencil) {
            this._context.PrimitiveBatch.PopDepthStencilState();
            this._pushedPrimitiveDepthStencil = false;
        }
    }

    private void ResetPushFlags() {
        this._pushedSpriteDepthStencil = false;
        this._pushedSpriteRasterizer = false;
        this._pushedSpriteScissor = false;

        this._pushedPrimitiveDepthStencil = false;
        this._pushedPrimitiveRasterizer = false;
        this._pushedPrimitiveScissor = false;
    }

    private void EnsureStarted() {
        if (this._context == null || this._framebuffer == null) {
            throw new InvalidOperationException("GuiRenderQueue.Begin must be called before drawing.");
        }
    }
}