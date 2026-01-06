using Bliss.CSharp.Colors;
using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Sprites;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
using Veldrid;

namespace Sparkle.CSharp.GUI.Elements.Data;

public class TextureButtonData {
    
    /// <summary>
    /// The texture used for the button.
    /// </summary>
    public Texture2D Texture;
    
    /// <summary>
    /// The sampler used to control how the texture is sampled.
    /// </summary>
    public Sampler? Sampler;
    
    /// <summary>
    /// The section of the texture to be displayed.
    /// </summary>
    public Rectangle SourceRect;
    
    /// <summary>
    /// The mode used to resize the texture.
    /// </summary>
    public ResizeMode ResizeMode;
    
    /// <summary>
    /// The border insets used for nine-slice resizing.
    /// </summary>
    public BorderInsets BorderInsets;
    
    /// <summary>
    /// The primary color.
    /// </summary>
    public Color Color;
    
    /// <summary>
    /// The color get's applied when the mouse is hover over.
    /// </summary>
    public Color HoverColor;
    
    /// <summary>
    /// The flip mode for the texture (none, horizontal, vertical, both).
    /// </summary>
    public SpriteFlip Flip;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TextureButtonData"/> class.
    /// </summary>
    /// <param name="texture">The texture used for the button.</param>
    /// <param name="sampler">The sampler used to control how the texture is sampled.</param>
    /// <param name="sourceRect">The section of the texture to be displayed.</param>
    /// <param name="resizeMode">The mode used to resize the texture.</param>
    /// <param name="borderInsets">The border insets used for nine-slice resizing.</param>
    /// <param name="color">The primary color.</param>
    /// <param name="hoverColor">The color gets applied when the mouse is hover over.</param>
    /// <param name="flip">The flip mode for the texture (none, horizontal, vertical, both).</param>
    public TextureButtonData(Texture2D texture, Sampler? sampler = null, Rectangle? sourceRect = null, ResizeMode resizeMode = ResizeMode.None, BorderInsets? borderInsets = null, Color? color = null, Color? hoverColor = null, SpriteFlip flip = SpriteFlip.None) {
        this.Texture = texture;
        this.Sampler = sampler;
        this.SourceRect = sourceRect ?? new Rectangle(0, 0, (int) texture.Width, (int) texture.Height);
        this.ResizeMode = resizeMode;
        this.BorderInsets = borderInsets ?? BorderInsets.Zero;
        this.Color = color ?? Color.White;
        this.HoverColor = hoverColor ?? this.Color;
        this.Flip = flip;
    }
}