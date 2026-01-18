using Bliss.CSharp.Colors;
using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Sprites;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
using Veldrid;

namespace Sparkle.CSharp.GUI.Elements.Data;

public class TextureTextBoxData {
    
    /// <summary>
    /// The texture used for the text box.
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
    /// The color for the highlight.
    /// </summary>
    public Color HighlightColor;
    
    /// <summary>
    /// The color applied when the toggle is in an inactive or disabled state.
    /// </summary>
    public Color DisabledColor;
    
    /// <summary>
    /// The flip mode for the texture (none, horizontal, vertical, both).
    /// </summary>
    public SpriteFlip Flip;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TextureTextBoxData"/> class.
    /// </summary>
    /// <param name="texture">The texture used for the text box.</param>
    /// <param name="sampler">The sampler used to control how the texture is sampled.</param>
    /// <param name="sourceRect">The section of the texture to be displayed.</param>
    /// <param name="resizeMode">The mode used to resize the texture.</param>
    /// <param name="borderInsets">The border insets used for nine-slice resizing.</param>
    /// <param name="color">The primary color.</param>
    /// <param name="hoverColor">The color get's applied when the mouse is hover over.</param>
    /// <param name="highlightColor">The color for the highlight.</param>
    /// <param name="disabledColor">The color displayed when the text box is disabled.</param>
    /// <param name="flip">The flip mode for the texture (none, horizontal, vertical, both).</param>
    public TextureTextBoxData(Texture2D texture, Sampler? sampler = null, Rectangle? sourceRect = null, ResizeMode resizeMode = ResizeMode.None, BorderInsets? borderInsets = null, Color? color = null, Color? hoverColor = null, Color? highlightColor = null, Color? disabledColor = null, SpriteFlip flip = SpriteFlip.None) {
        this.Texture = texture;
        this.Sampler = sampler;
        this.SourceRect = sourceRect ?? new Rectangle(0, 0, (int) texture.Width, (int) texture.Height);
        this.ResizeMode = resizeMode;
        this.BorderInsets = borderInsets ?? BorderInsets.Zero;
        this.Color = color ?? Color.White;
        this.HoverColor = hoverColor ?? this.Color;
        this.HighlightColor = highlightColor ?? new Color(0, 128, 228, 128);
        this.DisabledColor = disabledColor ?? Color.Gray;
        this.Flip = flip;
    }
}