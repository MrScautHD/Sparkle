using Bliss.CSharp.Colors;

namespace Sparkle.CSharp.GUI.Elements.Data;

public class RectangleSlideBarData {
    
    /// <summary>
    /// The base color used to render the slide bar background.
    /// </summary>
    public Color BarColor;
    
    /// <summary>
    /// The color applied to the slide bar background when hovered.
    /// </summary>
    public Color BarHoverColor;
    
    /// <summary>
    /// The color applied to the slide bar background when disabled.
    /// </summary>
    public Color DisabledBarColor;
    
    /// <summary>
    /// The thickness of the outline drawn around the slide bar.
    /// </summary>
    public float BarOutlineThickness;
    
    /// <summary>
    /// The color of the slide bar outline.
    /// </summary>
    public Color BarOutlineColor;
    
    /// <summary>
    /// The color of the slide bar outline when hovered.
    /// </summary>
    public Color BarOutlineHoverColor;
    
    /// <summary>
    /// The color of the slide bar outline when disabled.
    /// </summary>
    public Color DisabledBarOutlineColor;
    
    /// <summary>
    /// The base color used to render the slider handle.
    /// </summary>
    public Color SliderColor;
    
    /// <summary>
    /// The color applied to the slider handle when hovered.
    /// </summary>
    public Color SliderHoverColor;
    
    /// <summary>
    /// The color applied to the slider handle when disabled.
    /// </summary>
    public Color DisabledSliderColor;
    
    /// <summary>
    /// The thickness of the outline drawn around the slider handle.
    /// </summary>
    public float SliderOutlineThickness;
    
    /// <summary>
    /// The color of the slider handle outline.
    /// </summary>
    public Color SliderOutlineColor;
    
    /// <summary>
    /// The color of the slider handle outline when hovered.
    /// </summary>
    public Color SliderOutlineHoverColor;
    
    /// <summary>
    /// The color of the slider handle outline when disabled.
    /// </summary>
    public Color DisabledSliderOutlineColor;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="RectangleSlideBarData"/> class.
    /// </summary>
    /// <param name="barColor">The base color used to render the slide bar background.</param>
    /// <param name="barHoverColor">The color applied to the slide bar background when hovered.</param>
    /// <param name="disabledBarColor">The color applied to the slide bar background when disabled.</param>
    /// <param name="barOutlineThickness">The thickness of the outline drawn around the slide bar.</param>
    /// <param name="barOutlineColor">The color of the slide bar outline.</param>
    /// <param name="barOutlineHoverColor">The color of the slide bar outline when hovered.</param>
    /// <param name="disabledBarOutlineColor">The color of the slide bar outline when disabled.</param>
    /// <param name="sliderColor">The base color used to render the slider handle.</param>
    /// <param name="sliderHoverColor">The color applied to the slider handle when hovered.</param>
    /// <param name="disabledSliderColor">The color applied to the slider handle when disabled.</param>
    /// <param name="sliderOutlineThickness">The thickness of the outline drawn around the slider handle.</param>
    /// <param name="sliderOutlineColor">The color of the slider handle outline.</param>
    /// <param name="sliderOutlineHoverColor">The color of the slider handle outline when hovered.</param>
    /// <param name="disabledSliderOutlineColor">The color of the slider handle outline when disabled.</param>
    public RectangleSlideBarData(
        Color? barColor = null,
        Color? barHoverColor = null,
        Color? disabledBarColor = null,
        float barOutlineThickness = 0.0F,
        Color? barOutlineColor = null,
        Color? barOutlineHoverColor = null,
        Color? disabledBarOutlineColor = null,
        Color? sliderColor = null,
        Color? sliderHoverColor = null,
        Color? disabledSliderColor = null,
        float sliderOutlineThickness = 0.0F,
        Color? sliderOutlineColor = null,
        Color? sliderOutlineHoverColor = null,
        Color? disabledSliderOutlineColor = null) {
        this.BarColor = barColor ?? Color.White;
        this.BarHoverColor = barHoverColor ?? this.BarColor;
        this.DisabledBarColor = disabledBarColor ?? Color.Gray;
        this.BarOutlineThickness = barOutlineThickness;
        this.BarOutlineColor = barOutlineColor ?? Color.White;
        this.BarOutlineHoverColor = barOutlineHoverColor ?? this.BarOutlineColor;
        this.DisabledBarOutlineColor = disabledBarOutlineColor ?? Color.DarkGray;
        this.SliderColor = sliderColor ?? Color.White;
        this.SliderHoverColor = sliderHoverColor ?? this.SliderColor;
        this.DisabledSliderColor = disabledSliderColor ?? Color.Gray;
        this.SliderOutlineThickness = sliderOutlineThickness;
        this.SliderOutlineColor = sliderOutlineColor ?? Color.White;
        this.SliderOutlineHoverColor = sliderOutlineHoverColor ?? this.SliderOutlineColor;
        this.DisabledSliderOutlineColor = disabledSliderOutlineColor ?? Color.DarkGray;
    }
}