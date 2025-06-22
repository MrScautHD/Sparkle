using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Fonts;
using FontStashSharp;
using Veldrid;

namespace Sparkle.CSharp.GUI.Elements.Data;

public class LabelData {
    
    /// <summary>
    /// The font used to render the text.
    /// </summary>
    public Font Font;
    
    /// <summary>
    /// The text content to be displayed.
    /// </summary>
    public string Text;
    
    /// <summary>
    /// The font size of the text.
    /// </summary>
    public float Size;
    
    /// <summary>
    /// The spacing between individual characters.
    /// </summary>
    public float CharacterSpacing;
    
    /// <summary>
    /// The spacing between lines of text.
    /// </summary>
    public float LineSpacing;
    
    /// <summary>
    /// The scaling factor applied to the rendered text.
    /// </summary>
    public Vector2 Scale;
    
    /// <summary>
    /// The color of the rendered text.
    /// </summary>
    public Color Color;

    /// <summary>
    /// The color of the text when the label is hovered over.
    /// </summary>
    public Color HoverColor;
    
    /// <summary>
    /// The style of the text, such as bold or italic.
    /// </summary>
    public TextStyle Style;
    
    /// <summary>
    /// The special visual effect applied to the font (e.g., shadow, outline).
    /// </summary>
    public FontSystemEffect Effect;
    
    /// <summary>
    /// The intensity or size of the font effect.
    /// </summary>
    public int EffectAmount;
    
    /// <summary>
    /// The texture sampler used for rendering operations in the label.
    /// </summary>
    public Sampler? Sampler;

    /// <summary>
    /// Constructs a new instance of <see cref="LabelData"/> with full customization for rendering text.
    /// </summary>
    /// <param name="font">The font to use for rendering the text.</param>
    /// <param name="text">The text content to display.</param>
    /// <param name="size">The font size of the text.</param>
    /// <param name="characterSpacing">Spacing between characters. Default is 0.</param>
    /// <param name="lineSpacing">Spacing between lines. Default is 0.</param>
    /// <param name="scale">Optional scale applied to the text. Defaults to <c>Vector2.One</c>.</param>
    /// <param name="color">Optional color of the text. Defaults to white.</param>
    /// <param name="hoverColor">Optional color of the text. Defaults the same as the color parameter.</param>
    /// <param name="style">The style of the text. Default is <c>TextStyle.None</c>.</param>
    /// <param name="effect">The font effect applied. Default is <c>FontSystemEffect.None</c>.</param>
    /// <param name="effectAmount">The intensity of the font effect. Default is 0.</param>
    /// <param name="sampler">The texture sampler used for rendering operations in the label.</param>
    public LabelData(Font font, string text, float size, float characterSpacing = 0.0F, float lineSpacing = 0.0F, Vector2? scale = null, Color? color = null, Color? hoverColor = null, TextStyle style = TextStyle.None, FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0, Sampler? sampler = null) {
        this.Font = font;
        this.Text = text;
        this.Size = size;
        this.CharacterSpacing = characterSpacing;
        this.LineSpacing = lineSpacing;
        this.Scale = scale ?? Vector2.One;
        this.Color = color ?? Color.White;
        this.HoverColor = hoverColor ?? this.Color;
        this.Style = style;
        this.Effect = effect;
        this.EffectAmount = effectAmount;
        this.Sampler = sampler;
    }
}