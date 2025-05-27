using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Fonts;
using FontStashSharp;

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
    public Vector2? Scale;
    
    /// <summary>
    /// The depth layer at which the text is rendered, controlling draw order.
    /// </summary>
    public float LayerDepth;
    
    /// <summary>
    /// The color of the rendered text.
    /// </summary>
    public Color Color;
    
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
    /// Constructs a new instance of <see cref="LabelData"/> with full customization for rendering text.
    /// </summary>
    /// <param name="font">The font to use for rendering the text.</param>
    /// <param name="text">The text content to display.</param>
    /// <param name="size">The font size of the text.</param>
    /// <param name="characterSpacing">Spacing between characters. Default is 0.</param>
    /// <param name="lineSpacing">Spacing between lines. Default is 0.</param>
    /// <param name="scale">Optional scale applied to the text. Defaults to <c>Vector2.One</c>.</param>
    /// <param name="layerDepth">The depth layer for rendering. Default is 0.5.</param>
    /// <param name="color">Optional color of the text. Defaults to white.</param>
    /// <param name="style">The style of the text. Default is <c>TextStyle.None</c>.</param>
    /// <param name="effect">The font effect applied. Default is <c>FontSystemEffect.None</c>.</param>
    /// <param name="effectAmount">The intensity of the font effect. Default is 0.</param>
    public LabelData(Font font, string text, float size, float characterSpacing = 0.0F, float lineSpacing = 0.0F, Vector2? scale = null, float layerDepth = 0.5F, Color? color = null, TextStyle style = TextStyle.None, FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0) {
        this.Font = font;
        this.Text = text;
        this.Size = size;
        this.CharacterSpacing = characterSpacing;
        this.LineSpacing = lineSpacing;
        this.Scale = scale ?? Vector2.One;
        this.LayerDepth = layerDepth;
        this.Color = color ?? Color.White;
        this.Style = style;
        this.Effect = effect;
        this.EffectAmount = effectAmount;
    }
}