using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Effects;
using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Sprites;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
using Veldrid;

namespace Sparkle.CSharp.Graphics.Rendering.Sprites;

public struct SpriteData {
    
    /// <summary>
    /// Optional custom effect (shader) applied when rendering this sprite.
    /// </summary>
    public Effect? Effect;
    
    /// <summary>
    /// Optional blend state override used for this sprite.
    /// </summary>
    public BlendStateDescription? BlendState;
    
    /// <summary>
    /// Optional depth-stencil state override used for this sprite.
    /// </summary>
    public DepthStencilStateDescription? DepthStencilState;
    
    /// <summary>
    /// Optional rasterizer state override used for this sprite.
    /// </summary>
    public RasterizerStateDescription? RasterizerState;

    /// <summary>
    /// Optional scissor rectangle to define a restricted rendering area for this sprite.
    /// </summary>
    public Rectangle? ScissorRect;
    
    /// <summary>
    /// The texture to render for this sprite.
    /// </summary>
    public Texture2D Texture;
    
    /// <summary>
    /// Optional sampler to control how the texture is sampled.
    /// </summary>
    public Sampler? Sampler;
    
    /// <summary>
    /// The world-space position where the sprite should be rendered.
    /// </summary>
    public Vector2 Position;
    
    /// <summary>
    /// The layer depth used to determine draw order.
    /// </summary>
    public float LayerDepth;
    
    /// <summary>
    /// The source rectangle defining the portion of the texture to use.
    /// </summary>
    public Rectangle SourceRect;
    
    /// <summary>
    /// The scale factor to apply when rendering the sprite.
    /// </summary>
    public Vector2 Scale;
    
    /// <summary>
    /// The origin point for scaling and rotation, typically the center or a corner of the sprite.
    /// </summary>
    public Vector2 Origin;
    
    /// <summary>
    /// The rotation to apply when rendering the sprite.
    /// </summary>
    public float Rotation;
    
    /// <summary>
    /// The color tint to apply to the sprite.
    /// </summary>
    public Color Color;
    
    /// <summary>
    /// The sprite flip mode.
    /// </summary>
    public SpriteFlip Flip;

    /// <summary>
    /// Initializes a new <see cref="SpriteData"/> instance with the specified rendering configuration.
    /// </summary>
    /// <param name="texture">The texture to draw.</param>
    /// <param name="sampler">Optional sampler for texture sampling.</param>
    /// <param name="position">The world position where the sprite will be rendered.</param>
    /// <param name="layerDepth">The Z-order value controlling render order.</param>
    /// <param name="sourceRect">The portion of the texture to draw.</param>
    /// <param name="scale">The scale factor for the sprite.</param>
    /// <param name="origin">The pivot point for rotation and scaling.</param>
    /// <param name="rotation">The rotation angle in radians.</param>
    /// <param name="color">A color tint to blend with the sprite texture.</param>
    /// <param name="flip">The flip mode to apply when rendering the sprite.</param>
    /// <param name="effect">Optional rendering effect (shader).</param>
    /// <param name="blendState">Optional custom blend state.</param>
    /// <param name="depthStencilState">Optional depth-stencil state.</param>
    /// <param name="rasterizerState">Optional rasterizer state.</param>
    /// <param name="scissorRect">Optional scissor rectangle to define a restricted rendering area.</param>
    public SpriteData(Texture2D texture, Sampler? sampler, Vector2 position, float layerDepth, Rectangle sourceRect, Vector2 scale, Vector2 origin, float rotation, Color color, SpriteFlip flip, Effect? effect, BlendStateDescription? blendState, DepthStencilStateDescription? depthStencilState, RasterizerStateDescription? rasterizerState, Rectangle? scissorRect) {
        this.Texture = texture;
        this.Sampler = sampler;
        this.Position = position;
        this.LayerDepth = layerDepth;
        this.SourceRect = sourceRect;
        this.Scale = scale;
        this.Origin = origin;
        this.Rotation = rotation;
        this.Color = color;
        this.Flip = flip;
        this.Effect = effect;
        this.BlendState = blendState;
        this.DepthStencilState = depthStencilState;
        this.RasterizerState = rasterizerState;
        this.ScissorRect = scissorRect;
    }
}