using Bliss.CSharp.Colors;
using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Sprites;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
using Veldrid;

namespace Sparkle.CSharp.GUI.Elements.Data;

public class ImageData {
    
    /// <summary>
    /// The texture used for rendering the image.
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
    /// The color applied when the toggle is in an inactive or disabled state.
    /// </summary>
    public Color DisabledColor;
    
    /// <summary>
    /// The flip mode for the texture (none, horizontal, vertical, both).
    /// </summary>
    public SpriteFlip Flip;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageData"/> class.
    /// </summary>
    /// <param name="texture">The texture used for rendering.</param>
    /// <param name="sampler">Optional sampler controlling how the texture is sampled.</param>
    /// <param name="sourceRect">Optional rectangular region of the texture to display. If <c>null</c>, the full texture is used.</param>
    /// <param name="resizeMode">The resizing mode applied when scaling the image.</param>
    /// <param name="borderInsets">Optional border insets for nine-slice resizing. If <c>null</c>, zero insets are used.</param>
    /// <param name="color">Optional primary tint color. If <c>null</c>, <see cref="Color.White"/> is used.</param>
    /// <param name="hoverColor">Optional hover tint color. If <c>null</c>, the primary color is used.</param>
    /// <param name="disabledColor">Optional disabled tint color. If <c>null</c>, <see cref="Color.Gray"/> is used.</param>
    /// <param name="flip">The flip mode applied to the texture.</param>
    public ImageData(Texture2D texture, Sampler? sampler = null, Rectangle? sourceRect = null, ResizeMode resizeMode = ResizeMode.None, BorderInsets? borderInsets = null, Color? color = null, Color? hoverColor = null, Color? disabledColor = null, SpriteFlip flip = SpriteFlip.None) {
        this.Texture = texture;
        this.Sampler = sampler;
        this.SourceRect = sourceRect ?? new Rectangle(0, 0, (int) texture.Width, (int) texture.Height);
        this.ResizeMode = resizeMode;
        this.BorderInsets = borderInsets ?? BorderInsets.Zero;
        this.Color = color ?? Color.White;
        this.HoverColor = hoverColor ?? this.Color;
        this.DisabledColor = disabledColor ?? Color.Gray;
        this.Flip = flip;
    }
}