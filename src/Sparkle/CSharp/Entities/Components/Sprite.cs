using System.Numerics;
using Bliss.CSharp.Effects;
using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Sprites;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Veldrid;
using Vortice.Mathematics;
using Color = Bliss.CSharp.Colors.Color;

namespace Sparkle.CSharp.Entities.Components;

public class Sprite : InterpolatedComponent {
    
    /// <summary>
    /// Optional custom shader effect to apply during rendering.
    /// </summary>
    public Effect? Effect;
    
    /// <summary>
    /// Optional blend state override used when drawing the sprite.
    /// </summary>
    public BlendStateDescription? BlendState;

    /// <summary>
    /// Optional depth-stencil state override used when drawing the sprite.
    /// </summary>
    public DepthStencilStateDescription? DepthStencilState;

    /// <summary>
    /// Optional rasterizer state override used when drawing the sprite.
    /// </summary>
    public RasterizerStateDescription? RasterizerState;

    /// <summary>
    /// Optional scissor rectangle to define a clipping region during rendering.
    /// </summary>
    public Rectangle? ScissorRect;
    
    /// <summary>
    /// The texture used to render the sprite.
    /// </summary>
    public Texture2D Texture;

    /// <summary>
    /// Optional sampler state override used when sampling the sprite texture.
    /// </summary>
    public Sampler? Sampler;

    /// <summary>
    /// The portion of the texture to be used when rendering the sprite.
    /// </summary>
    public Rectangle? SourceRect;
    
    /// <summary>
    /// The size of the sprite in pixels. If not specified, defaults to the texture's dimensions.
    /// </summary>
    public Vector2 Size;

    /// <summary>
    /// Specifies the rendering order of the sprite.
    /// </summary>
    public float LayerDepth;
    
    /// <summary>
    /// The tint color applied when rendering the sprite. Defaults to white (no tint).
    /// </summary>
    public Color Color;
    
    /// <summary>
    /// The flip mode of the sprite (e.g., horizontal, vertical, or none).
    /// </summary>
    public SpriteFlip Flip;

    /// <summary>
    /// Initializes a new instance of the <see cref="Sprite"/> class with configurable rendering and transform options.
    /// </summary>
    /// <param name="texture">The texture to display for this sprite.</param>
    /// <param name="offsetPos">The local position offset of the sprite relative to its entity.</param>
    /// <param name="sampler">Optional sampler used to sample the texture.</param>
    /// <param name="sourceRect">The subsection of the texture to draw. If null, uses the full texture.</param>
    /// <param name="size">The size of the sprite. If null, defaults to the texture’s dimensions.</param>
    /// <param name="layerDepth">The depth layer of the sprite (used for Z-ordering). Lower values are drawn in front.</param>
    /// <param name="color">Optional color tint. Defaults to white if null.</param>
    /// <param name="flip">The flip mode for rendering. Defaults to <see cref="SpriteFlip.None"/>.</param>
    /// <param name="effect">Optional effect (shader) to apply to this sprite.</param>
    /// <param name="blendState">Optional blend state override.</param>
    /// <param name="depthStencilState">Optional depth-stencil state override.</param>
    /// <param name="rasterizerState">Optional rasterizer state override.</param>
    /// <param name="scissorRect">The rectangle used to define the scissor test area.</param>
    public Sprite(Texture2D texture, Vector2 offsetPos, Sampler? sampler = null, Rectangle? sourceRect = null, Vector2? size = null, float layerDepth = 0.5F, Color? color = null, SpriteFlip flip = SpriteFlip.None, Effect? effect = null, BlendStateDescription? blendState = null, DepthStencilStateDescription? depthStencilState = null, RasterizerStateDescription? rasterizerState = null, Rectangle? scissorRect = null) : base(new Vector3(offsetPos, 0)) {
        this.Texture = texture;
        this.Sampler = sampler;
        this.SourceRect = sourceRect;
        this.Size = size ?? new Vector2(texture.Width, texture.Height);
        this.LayerDepth = layerDepth;
        this.Color = color ?? Color.White;
        this.Flip = flip;
        this.Effect = effect;
        this.BlendState = blendState;
        this.DepthStencilState = depthStencilState;
        this.RasterizerState = rasterizerState;
        this.ScissorRect = scissorRect;
    }
    
    /// <summary>
    /// Draws the sprite using the provided graphics context and target framebuffer.
    /// </summary>
    /// <param name="context">The graphics context used for rendering operations.</param>
    /// <param name="framebuffer">The target framebuffer where the sprite will be drawn.</param>
    protected internal override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        base.Draw(context, framebuffer);
        
        Rectangle source = this.SourceRect ?? new Rectangle(0, 0, (int) this.Texture.Width, (int) this.Texture.Height);
        Vector2 origin = new Vector2(this.Size.X / 2.0F, this.Size.Y / 2.0F);
        Vector2 scale = new Vector2(this.LerpedScale.X, this.LerpedScale.Y);
        float rotation = float.RadiansToDegrees(this.LerpedRotation.ToEuler().Z);
        
        // Draw sprite.
        this.Entity.Scene.SpriteRenderer.DrawSprite(this.Texture, this.Sampler, new Vector2(this.LerpedGlobalPosition.X, this.LerpedGlobalPosition.Y), this.LayerDepth, source, scale, origin, rotation, this.Color, this.Flip, this.Effect, this.BlendState, this.DepthStencilState, this.RasterizerState, this.ScissorRect);
    }
}