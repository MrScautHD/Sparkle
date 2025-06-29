using Bliss.CSharp.Colors;

namespace Sparkle.CSharp.GUI.Elements.Data;

public class RectangleTextBoxData {
    
    /// <summary>
    /// The base fill color of the text box.
    /// </summary>
    public Color Color;
    
    /// <summary>
    /// The fill color of the text box when it is hovered.
    /// </summary>
    public Color HoverColor;
    
    /// <summary>
    /// The thickness of the text box's outline. Set to 0 to disable outlining.
    /// </summary>
    public float OutlineThickness;
    
    /// <summary>
    /// The color of the outline.
    /// </summary>
    public Color OutlineColor;
    
    /// <summary>
    /// The color of the outline when the text box is hovered.
    /// </summary>
    public Color OutlineHoverColor;
    
    /// <summary>
    /// The color for the highlight.
    /// </summary>
    public Color HighlightColor;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="RectangleButtonData"/> class.
    /// </summary>
    /// <param name="color">The base fill color of the text box. Defaults to white.</param>
    /// <param name="hoverColor">The fill color when the text box is hovered. Defaults to the base color.</param>
    /// <param name="outlineThickness">The thickness of the text box outline. Defaults to 0 (no outline).</param>
    /// <param name="outlineColor">The color of the outline. Defaults to white.</param>
    /// <param name="outlineHoverColor">The outline color when hovered. Defaults to the regular outline color.</param>
    /// <param name="highlightColor">The color for the highlight.</param>
    public RectangleTextBoxData(Color? color = null, Color? hoverColor = null, float outlineThickness = 0.0F, Color? outlineColor = null, Color? outlineHoverColor = null, Color? highlightColor = null) {
        this.Color = color ?? Color.White;
        this.HoverColor = hoverColor ?? this.Color;
        this.OutlineThickness = outlineThickness;
        this.OutlineColor = outlineColor ?? Color.White;
        this.OutlineHoverColor = outlineHoverColor ?? this.OutlineColor;
        this.HighlightColor = highlightColor ?? new Color(0, 128, 228, 128);
    }
}