using Bliss.CSharp.Colors;
using Bliss.CSharp.Effects;
using Bliss.CSharp.Fonts;
using FontStashSharp;
using Veldrith;

namespace Sparkle.CSharp.GUI.Elements.Data;

public class LabelData {
    
    /// <summary>
    /// The font used to render the text.
    /// </summary>
    public Font Font;
    
    /// <summary>
    /// The text to be displayed.
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
    /// The color of the rendered text.
    /// </summary>
    public Color Color;
    
    /// <summary>
    /// The color of the text when the label is hovered over.
    /// </summary>
    public Color HoverColor;
    
    /// <summary>
    /// The color applied when the toggle is in an inactive or disabled state.
    /// </summary>
    public Color DisabledColor;
    
    /// <summary>
    /// The style of the text, such as bold or italic.
    /// </summary>
    public TextStyle Style;
    
    /// <summary>
    /// The special visual effect applied to the font (e.g., shadow, outline).
    /// </summary>
    public FontSystemEffect FontSystemEffect;
    
    /// <summary>
    /// The intensity or size of the font effect.
    /// </summary>
    public int EffectAmount;
    
    /// <summary>
    /// The texture sampler used for rendering operations in the label.
    /// </summary>
    public Sampler? Sampler;
    
    /// <summary>
    /// When <c>true</c>, snaps the position and origin to whole pixels using floor, preventing sub-pixel blurriness.
    /// </summary>
    public bool PixelSnap;
    
    /// <summary>
    /// The effect used when rendering the font. When <c>null</c>, the default sprite effect is used.
    /// </summary>
    public Effect? Effect;
    
    /// <summary>
    /// The blend state used when rendering the font. When <c>null</c>, the batch's current blend state is used.
    /// </summary>
    public BlendStateDescription? BlendState;
    
    /// <summary>
    /// Constructs a new instance of <see cref="LabelData"/> with full customization for rendering text.
    /// </summary>
    /// <param name="font">The font to use for rendering the text.</param>
    /// <param name="text">The text content to display.</param>
    /// <param name="size">The font size of the text.</param>
    /// <param name="characterSpacing">Spacing between characters. Default is 0.</param>
    /// <param name="lineSpacing">Spacing between lines. Default is 0.</param>
    /// <param name="color">Optional color of the text. Defaults to white.</param>
    /// <param name="hoverColor">Optional color of the text. Defaults the same as the color parameter.</param>
    /// <param name="disabledColor">The color of the text when the label is in a disabled state. Defaults to gray if not specified.</param>
    /// <param name="style">The style of the text. Default is <c>TextStyle.None</c>.</param>
    /// <param name="fontSystemEffect">The font fontSystemEffect applied. Default is <c>FontSystemEffect.None</c>.</param>
    /// <param name="effectAmount">The intensity of the font fontSystemEffect. Default is 0.</param>
    /// <param name="sampler">The texture sampler used for rendering operations in the label.</param>
    /// <param name="pixelSnap">When <c>true</c>, snaps position and origin to whole pixels using floor, preventing sub-pixel blurriness. Default is <c>false</c>.</param>
    /// <param name="effect">Optional effect used when rendering. If <c>null</c>, the default sprite effect is used.</param>
    /// <param name="blendState">Optional blend state used when rendering. If <c>null</c>, the batch's current blend state is used.</param>
    public LabelData(
        Font font,
        string text,
        float size,
        float characterSpacing = 0.0F,
        float lineSpacing = 0.0F,
        Color? color = null,
        Color? hoverColor = null,
        Color? disabledColor = null,
        TextStyle style = TextStyle.None,
        FontSystemEffect fontSystemEffect = FontSystemEffect.None,
        int effectAmount = 0,
        Sampler? sampler = null,
        bool pixelSnap = false,
        Effect? effect = null,
        BlendStateDescription? blendState = null
    ) {
        this.Font = font;
        this.Text = text;
        this.Size = size;
        this.CharacterSpacing = characterSpacing;
        this.LineSpacing = lineSpacing;
        this.Color = color ?? Color.White;
        this.HoverColor = hoverColor ?? this.Color;
        this.DisabledColor = disabledColor ?? this.Color.AdjustSaturation(-0.35F) * new Color(140, 140, 140, 255);
        this.Style = style;
        this.FontSystemEffect = fontSystemEffect;
        this.EffectAmount = effectAmount;
        this.Sampler = sampler;
        this.PixelSnap = pixelSnap;
        this.Effect = effect;
        this.BlendState = blendState;
    }
}