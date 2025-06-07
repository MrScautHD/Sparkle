using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Graphics.Rendering.Batches.Sprites;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;

namespace Sparkle.CSharp.GUI.Elements.Data;

public class TextureButtonData {
    
    /// <summary>
    /// The texture used for the button.
    /// </summary>
    public Texture2D Texture;
    
    /// <summary>
    /// The rectangular region of the texture to use when drawing.
    /// </summary>
    public Rectangle SourceRect;
    
    /// <summary>
    /// The scale to apply to the texture when rendering.
    /// </summary>
    public Vector2 Scale;
    
    /// <summary>
    /// The base color applied to the texture (acts like a tint).
    /// </summary>
    public Color Color;
    
    /// <summary>
    /// The color to apply when the button is hovered. Defaults to the base color.
    /// </summary>
    public Color HoverColor;
    
    /// <summary>
    /// The flipping mode to apply when rendering the texture.
    /// </summary>
    public SpriteFlip Flip;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TextureButtonData"/> class.
    /// </summary>
    /// <param name="texture">The texture to be used as the button's visual.</param>
    /// <param name="sourceRect">The portion of the texture to use. If null, the entire texture is used.</param>
    /// <param name="scale">The scale factor applied to the texture. Defaults to <c>Vector2.One</c>.</param>
    /// <param name="color">The base color (tint) applied to the texture. Defaults to white.</param>
    /// <param name="hoverColor">The color applied when the button is hovered. Defaults to the base color.</param>
    /// <param name="flip">The flipping mode applied to the texture (e.g., horizontal/vertical).</param>
    public TextureButtonData(Texture2D texture, Rectangle? sourceRect = null, Vector2? scale = null, Color? color = null, Color? hoverColor = null, SpriteFlip flip = SpriteFlip.None) {
        this.Texture = texture;
        this.SourceRect = sourceRect ?? new Rectangle(0, 0, (int) texture.Width, (int) texture.Height);
        this.Scale = scale ?? Vector2.One;
        this.Color = color ?? Color.White;
        this.HoverColor = hoverColor ?? this.Color;
        this.Flip = flip;
    }
}