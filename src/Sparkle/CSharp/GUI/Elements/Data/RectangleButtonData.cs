using Bliss.CSharp.Colors;

namespace Sparkle.CSharp.GUI.Elements.Data;

public class RectangleButtonData {
    
    /// <summary>
    /// The base fill color of the button.
    /// </summary>
    public Color Color;
    
    /// <summary>
    /// The fill color of the button when it is hovered.
    /// </summary>
    public Color HoverColor;
    
    /// <summary>
    /// The thickness of the button's outline. Set to 0 to disable outlining.
    /// </summary>
    public float OutlineThickness;
    
    /// <summary>
    /// The color of the outline when the button is not hovered.
    /// </summary>
    public Color OutlineColor;
    
    /// <summary>
    /// The color of the outline when the button is hovered.
    /// </summary>
    public Color OutlineHoverColor;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="RectangleButtonData"/> class.
    /// </summary>
    /// <param name="color">The base fill color of the button. Defaults to white.</param>
    /// <param name="hoverColor">The fill color when the button is hovered. Defaults to the base color.</param>
    /// <param name="outlineThickness">The thickness of the button outline. Defaults to 0 (no outline).</param>
    /// <param name="outlineColor">The color of the outline. Defaults to white.</param>
    /// <param name="outlineHoverColor">The outline color when hovered. Defaults to the regular outline color.</param>
    public RectangleButtonData(Color? color = null, Color? hoverColor = null, float outlineThickness = 0.0F, Color? outlineColor = null, Color? outlineHoverColor = null) {
        this.Color = color ?? Color.White;
        this.HoverColor = hoverColor ?? this.Color;
        this.OutlineThickness = outlineThickness;
        this.OutlineColor = outlineColor ?? Color.White;
        this.OutlineHoverColor = outlineHoverColor ?? this.OutlineColor;
    }
}