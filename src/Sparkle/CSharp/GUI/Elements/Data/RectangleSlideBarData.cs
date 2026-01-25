using System.Numerics;
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
    /// The thickness of the outline drawn around the slide bar background.
    /// </summary>
    public float BarOutlineThickness;
    
    /// <summary>
    /// The color of the slide bar background outline.
    /// </summary>
    public Color BarOutlineColor;
    
    /// <summary>
    /// The color of the slide bar background outline when hovered.
    /// </summary>
    public Color BarOutlineHoverColor;
    
    /// <summary>
    /// The color of the slide bar background outline when disabled.
    /// </summary>
    public Color DisabledBarOutlineColor;
    
    /// <summary>
    /// The base color used to render the filled portion of the slide bar.
    /// </summary>
    public Color FilledBarColor;
    
    /// <summary>
    /// The color applied to the filled portion of the slide bar when hovered.
    /// </summary>
    public Color FilledBarHoverColor;
    
    /// <summary>
    /// The color applied to the filled portion of the slide bar when disabled.
    /// </summary>
    public Color DisabledFilledBarColor;
    
    /// <summary>
    /// The thickness of the outline drawn around the filled portion of the slide bar.
    /// </summary>
    public float FilledBarOutlineThickness;
    
    /// <summary>
    /// The color of the filled portion outline.
    /// </summary>
    public Color FilledBarOutlineColor;
    
    /// <summary>
    /// The color of the filled portion outline when hovered.
    /// </summary>
    public Color FilledBarOutlineHoverColor;
    
    /// <summary>
    /// The color of the filled portion outline when disabled.
    /// </summary>
    public Color DisabledFilledBarOutlineColor;
    
    /// <summary>
    /// The optional size override for the slider handle.
    /// </summary>
    public Vector2? SliderSize;
    
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
    /// <param name="barOutlineThickness">The thickness of the outline drawn around the slide bar background.</param>
    /// <param name="barOutlineColor">The color of the slide bar background outline.</param>
    /// <param name="barOutlineHoverColor">The color of the slide bar background outline when hovered.</param>
    /// <param name="disabledBarOutlineColor">The color of the slide bar background outline when disabled.</param>
    /// <param name="filledBarColor">The base color used to render the filled portion of the slide bar.</param>
    /// <param name="filledBarHoverColor">The color applied to the filled portion when hovered.</param>
    /// <param name="disabledFilledBarColor">The color applied to the filled portion when disabled.</param>
    /// <param name="filledBarOutlineThickness">The thickness of the outline drawn around the filled portion.</param>
    /// <param name="filledBarOutlineColor">The color of the filled portion outline.</param>
    /// <param name="filledBarOutlineHoverColor">The color of the filled portion outline when hovered.</param>
    /// <param name="disabledFilledBarOutlineColor">The color of the filled portion outline when disabled.</param>
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
        Color? filledBarColor = null,
        Color? filledBarHoverColor = null,
        Color? disabledFilledBarColor = null,
        float filledBarOutlineThickness = 0.0F,
        Color? filledBarOutlineColor = null,
        Color? filledBarOutlineHoverColor = null,
        Color? disabledFilledBarOutlineColor = null,
        Color? sliderColor = null,
        Color? sliderHoverColor = null,
        Color? disabledSliderColor = null,
        float sliderOutlineThickness = 0.0F,
        Color? sliderOutlineColor = null,
        Color? sliderOutlineHoverColor = null,
        Color? disabledSliderOutlineColor = null) {
        this.BarColor = barColor ?? Color.Gray;
        this.BarHoverColor = barHoverColor ?? this.BarColor;
        this.DisabledBarColor = disabledBarColor ?? Color.DarkGray;
        this.BarOutlineThickness = barOutlineThickness;
        this.BarOutlineColor = barOutlineColor ?? Color.DarkGray;
        this.BarOutlineHoverColor = barOutlineHoverColor ?? this.BarOutlineColor;
        this.DisabledBarOutlineColor = disabledBarOutlineColor ?? Color.DarkGray;
        this.FilledBarColor = filledBarColor ?? Color.White;
        this.FilledBarHoverColor = filledBarHoverColor ?? this.FilledBarColor;
        this.DisabledFilledBarColor = disabledFilledBarColor ?? Color.Gray;
        this.FilledBarOutlineThickness = filledBarOutlineThickness;
        this.FilledBarOutlineColor = filledBarOutlineColor ?? Color.DarkGray;
        this.FilledBarOutlineHoverColor = filledBarOutlineHoverColor ?? this.FilledBarOutlineColor;
        this.DisabledFilledBarOutlineColor = disabledFilledBarOutlineColor ?? Color.DarkGray;
        this.SliderColor = sliderColor ?? Color.LightGray;
        this.SliderHoverColor = sliderHoverColor ?? this.SliderColor;
        this.DisabledSliderColor = disabledSliderColor ?? Color.Gray;
        this.SliderOutlineThickness = sliderOutlineThickness;
        this.SliderOutlineColor = sliderOutlineColor ?? Color.DarkGray;
        this.SliderOutlineHoverColor = sliderOutlineHoverColor ?? this.SliderOutlineColor;
        this.DisabledSliderOutlineColor = disabledSliderOutlineColor ?? Color.DarkGray;
    }
}