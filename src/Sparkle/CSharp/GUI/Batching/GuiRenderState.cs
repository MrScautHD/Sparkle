using Bliss.CSharp.Effects;
using Bliss.CSharp.Transformations;
using Veldrith;

namespace Sparkle.CSharp.GUI.Batching;

public readonly struct GuiRenderState : IEquatable<GuiRenderState> {
    
    /// <summary>
    /// The default GUI render state.
    /// </summary>
    public static readonly GuiRenderState Default = new GuiRenderState();
    
    /// <summary>
    /// The sampler used for texture sampling, or <c>null</c> to fall back to the default sampler.
    /// </summary>
    public readonly Sampler? Sampler;
    
    /// <summary>
    /// The effect (shader program) applied during rendering, or <c>null</c> to use the default effect.
    /// </summary>
    public readonly Effect? Effect;
    
    /// <summary>
    /// The blend state controlling how source and destination colors are combined, or <c>null</c> for the default.
    /// </summary>
    public readonly BlendStateDescription? BlendState;
    
    /// <summary>
    /// The depth-stencil state controlling depth and stencil testing, or <c>null</c> for the default.
    /// </summary>
    public readonly DepthStencilStateDescription? DepthStencilState;
    
    /// <summary>
    /// The rasterizer state controlling culling, fill mode and related settings, or <c>null</c> for the default.
    /// </summary>
    public readonly RasterizerStateDescription? RasterizerState;
    
    /// <summary>
    /// The scissor rectangle clipping rendered output to a region, or <c>null</c> to disable scissor clipping.
    /// </summary>
    public readonly Rectangle? ScissorRect;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="GuiRenderState"/> struct with the specified render state parameters.
    /// </summary>
    /// <param name="sampler">The sampler used for texture sampling, or <c>null</c> for the default.</param>
    /// <param name="effect">The effect applied during rendering, or <c>null</c> for the default.</param>
    /// <param name="blendState">The blend state, or <c>null</c> for the default.</param>
    /// <param name="depthStencilState">The depth-stencil state, or <c>null</c> for the default.</param>
    /// <param name="rasterizerState">The rasterizer state, or <c>null</c> for the default.</param>
    /// <param name="scissorRect">The scissor rectangle, or <c>null</c> to disable scissor clipping.</param>
    public GuiRenderState(Sampler? sampler = null, Effect? effect = null, BlendStateDescription? blendState = null, DepthStencilStateDescription? depthStencilState = null, RasterizerStateDescription? rasterizerState = null, Rectangle? scissorRect = null) {
        this.Sampler = sampler;
        this.Effect = effect;
        this.BlendState = blendState;
        this.DepthStencilState = depthStencilState;
        this.RasterizerState = rasterizerState;
        this.ScissorRect = scissorRect;
    }
    
    /// <summary>
    /// Determines whether two <see cref="GuiRenderState"/> instances are equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><c>true</c> if both instances represent the same render state; otherwise, <c>false</c>.</returns>
    public static bool operator ==(GuiRenderState left, GuiRenderState right) {
        return left.Equals(right);
    }
    
    /// <summary>
    /// Determines whether two <see cref="GuiRenderState"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><c>true</c> if the instances represent different render states; otherwise, <c>false</c>.</returns>
    public static bool operator !=(GuiRenderState left, GuiRenderState right) {
        return !left.Equals(right);
    }
    
    /// <summary>
    /// Determines whether this instance is equal to another <see cref="GuiRenderState"/>.
    /// </summary>
    /// <param name="other">The instance to compare against.</param>
    /// <returns><c>true</c> if both instances represent the same render state; otherwise, <c>false</c>.</returns>
    public bool Equals(GuiRenderState other) {
        return this.Sampler == other.Sampler &&
               this.Effect == other.Effect &&
               Nullable.Equals(this.BlendState, other.BlendState) &&
               Nullable.Equals(this.DepthStencilState, other.DepthStencilState) &&
               Nullable.Equals(this.RasterizerState, other.RasterizerState) &&
               Nullable.Equals(this.ScissorRect, other.ScissorRect);
    }
    
    /// <summary>
    /// Determines whether this instance is equal to the specified object.
    /// </summary>
    /// <param name="obj">The object to compare against.</param>
    /// <returns><c>true</c> if <paramref name="obj"/> is a <see cref="GuiRenderState"/> representing the same render state; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj) {
        return obj is GuiRenderState other && this.Equals(other);
    }
    
    /// <summary>
    /// Returns a hash code computed from all render state fields.
    /// </summary>
    /// <returns>A hash code suitable for use in hash-based collections.</returns>
    public override int GetHashCode() {
        return HashCode.Combine(this.Sampler, this.Effect, this.BlendState, this.DepthStencilState, this.RasterizerState, this.ScissorRect);
    }
}