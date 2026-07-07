using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Effects;
using Veldrith;

namespace Sparkle.CSharp.GUI.Elements.Data;

public class RectangleScrollViewData {
    
    /// <summary>
    /// The base color used to render the scroll view menu.
    /// </summary>
    public Color MenuColor;
    
    /// <summary>
    /// The color applied to the menu when hovered.
    /// </summary>
    public Color MenuHoverColor;
    
    /// <summary>
    /// The color applied to the menu when disabled.
    /// </summary>
    public Color DisabledMenuColor;
    
    /// <summary>
    /// The thickness of the outline drawn around the menu.
    /// </summary>
    public float MenuOutlineThickness;
    
    /// <summary>
    /// The color of the menu outline.
    /// </summary>
    public Color MenuOutlineColor;
    
    /// <summary>
    /// The color of the menu outline when hovered.
    /// </summary>
    public Color MenuOutlineHoverColor;
    
    /// <summary>
    /// The color of the menu outline when disabled.
    /// </summary>
    public Color DisabledMenuOutlineColor;
    
    /// <summary>
    /// The width of the slider bar.
    /// </summary>
    public int SliderBarWidth;
    
    /// <summary>
    /// The base color used to render the slider bar.
    /// </summary>
    public Color SliderBarColor;
    
    /// <summary>
    /// The color applied to the slider bar when hovered.
    /// </summary>
    public Color SliderBarHoverColor;
    
    /// <summary>
    /// The color applied to the slider bar when disabled.
    /// </summary>
    public Color DisabledSliderBarColor;
    
    /// <summary>
    /// The thickness of the outline drawn around the slider bar.
    /// </summary>
    public float SliderBarOutlineThickness;
    
    /// <summary>
    /// The color of the slider bar outline.
    /// </summary>
    public Color SliderBarOutlineColor;
    
    /// <summary>
    /// The color of the slider bar outline when hovered.
    /// </summary>
    public Color SliderBarOutlineHoverColor;
    
    /// <summary>
    /// The color of the slider bar outline when disabled.
    /// </summary>
    public Color DisabledSliderBarOutlineColor;
    
    /// <summary>
    /// The size of the slider handle.
    /// </summary>
    public Vector2 SliderSize;
    
    /// <summary>
    /// The base color used to render the slider.
    /// </summary>
    public Color SliderColor;
    
    /// <summary>
    /// The color applied to the slider when hovered.
    /// </summary>
    public Color SliderHoverColor;
    
    /// <summary>
    /// The color applied to the slider when disabled.
    /// </summary>
    public Color DisabledSliderColor;
    
    /// <summary>
    /// The thickness of the outline drawn around the slider.
    /// </summary>
    public float SliderOutlineThickness;
    
    /// <summary>
    /// The color of the slider outline.
    /// </summary>
    public Color SliderOutlineColor;
    
    /// <summary>
    /// The color of the slider outline when hovered.
    /// </summary>
    public Color SliderOutlineHoverColor;
    
    /// <summary>
    /// The color of the slider outline when disabled.
    /// </summary>
    public Color DisabledSliderOutlineColor;
    
    /// <summary>
    /// The effect used when rendering the scroll view. When <c>null</c>, the default effect is used.
    /// </summary>
    public Effect? Effect;
    
    /// <summary>
    /// The blend state used when rendering the scroll view. When <c>null</c>, the batch's current blend state is used.
    /// </summary>
    public BlendStateDescription? BlendState;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="RectangleScrollViewData"/> class.
    /// </summary>
    /// <param name="menuColor">The base color used to render the scroll view menu.</param>
    /// <param name="menuHoverColor">The color applied to the menu when hovered.</param>
    /// <param name="disabledMenuColor">The color applied to the menu when disabled.</param>
    /// <param name="menuOutlineThickness">The thickness of the outline drawn around the menu.</param>
    /// <param name="menuOutlineColor">The color of the menu outline.</param>
    /// <param name="menuOutlineHoverColor">The color of the menu outline when hovered.</param>
    /// <param name="disabledMenuOutlineColor">The color of the menu outline when disabled.</param>
    /// <param name="sliderBarWidth">The width of the slider bar.</param>
    /// <param name="sliderBarColor">The base color used to render the slider bar.</param>
    /// <param name="sliderBarHoverColor">The color applied to the slider bar when hovered.</param>
    /// <param name="disabledSliderBarColor">The color applied to the slider bar when disabled.</param>
    /// <param name="sliderBarOutlineThickness">The thickness of the outline drawn around the slider bar.</param>
    /// <param name="sliderBarOutlineColor">The color of the slider bar outline.</param>
    /// <param name="sliderBarOutlineHoverColor">The color of the slider bar outline when hovered.</param>
    /// <param name="disabledSliderBarOutlineColor">The color of the slider bar outline when disabled.</param>
    /// <param name="sliderSize">The size of the slider handle.</param>
    /// <param name="sliderColor">The base color used to render the slider.</param>
    /// <param name="sliderHoverColor">The color applied to the slider when hovered.</param>
    /// <param name="disabledSliderColor">The color applied to the slider when disabled.</param>
    /// <param name="sliderOutlineThickness">The thickness of the outline drawn around the slider.</param>
    /// <param name="sliderOutlineColor">The color of the slider outline.</param>
    /// <param name="sliderOutlineHoverColor">The color of the slider outline when hovered.</param>
    /// <param name="disabledSliderOutlineColor">The color of the slider outline when disabled.</param>
    /// <param name="effect">Optional effect used when rendering. If <c>null</c>, the default effect is used.</param>
    /// <param name="blendState">Optional blend state used when rendering. If <c>null</c>, the batch's current blend state is used.</param>
    public RectangleScrollViewData(
        Color? menuColor = null,
        Color? menuHoverColor = null,
        Color? disabledMenuColor = null,
        float menuOutlineThickness = 0.0F,
        Color? menuOutlineColor = null,
        Color? menuOutlineHoverColor = null,
        Color? disabledMenuOutlineColor = null,
        int sliderBarWidth = 16,
        Color? sliderBarColor = null,
        Color? sliderBarHoverColor = null,
        Color? disabledSliderBarColor = null,
        float sliderBarOutlineThickness = 0.0F,
        Color? sliderBarOutlineColor = null,
        Color? sliderBarOutlineHoverColor = null,
        Color? disabledSliderBarOutlineColor = null,
        Vector2? sliderSize = null,
        Color? sliderColor = null,
        Color? sliderHoverColor = null,
        Color? disabledSliderColor = null,
        float sliderOutlineThickness = 0.0F,
        Color? sliderOutlineColor = null,
        Color? sliderOutlineHoverColor = null,
        Color? disabledSliderOutlineColor = null,
        Effect? effect = null,
        BlendStateDescription? blendState = null
    ) {
        this.MenuColor = menuColor ?? Color.Gray;
        this.MenuHoverColor = menuHoverColor ?? this.MenuColor;
        this.DisabledMenuColor = disabledMenuColor ?? this.MenuColor.AdjustSaturation(-0.35F) * new Color(140, 140, 140, 255);
        this.MenuOutlineThickness = menuOutlineThickness;
        this.MenuOutlineColor = menuOutlineColor ?? Color.DarkGray;
        this.MenuOutlineHoverColor = menuOutlineHoverColor ?? this.MenuOutlineColor;
        this.DisabledMenuOutlineColor = disabledMenuOutlineColor ?? this.MenuOutlineColor.AdjustSaturation(-0.35F) * new Color(140, 140, 140, 255);
        
        this.SliderBarWidth = sliderBarWidth;
        this.SliderBarColor = sliderBarColor ?? Color.Gray;
        this.SliderBarHoverColor = sliderBarHoverColor ?? this.SliderBarColor;
        this.DisabledSliderBarColor = disabledSliderBarColor ?? this.SliderBarColor.AdjustSaturation(-0.35F) * new Color(140, 140, 140, 255);
        this.SliderBarOutlineThickness = sliderBarOutlineThickness;
        this.SliderBarOutlineColor = sliderBarOutlineColor ?? Color.DarkGray;
        this.SliderBarOutlineHoverColor = sliderBarOutlineHoverColor ?? this.SliderBarOutlineColor;
        this.DisabledSliderBarOutlineColor = disabledSliderBarOutlineColor ?? this.SliderBarOutlineColor.AdjustSaturation(-0.35F) * new Color(140, 140, 140, 255);
        
        this.SliderSize = sliderSize ?? new Vector2(10.0F, 20.0F);
        this.SliderColor = sliderColor ?? Color.LightGray;
        this.SliderHoverColor = sliderHoverColor ?? this.SliderColor;
        this.DisabledSliderColor = disabledSliderColor ?? this.SliderColor.AdjustSaturation(-0.35F) * new Color(140, 140, 140, 255);
        this.SliderOutlineThickness = sliderOutlineThickness;
        this.SliderOutlineColor = sliderOutlineColor ?? Color.DarkGray;
        this.SliderOutlineHoverColor = sliderOutlineHoverColor ?? this.SliderOutlineColor;
        this.DisabledSliderOutlineColor = disabledSliderOutlineColor ?? this.SliderOutlineColor.AdjustSaturation(-0.35F) * new Color(140, 140, 140, 255);
        
        this.Effect = effect;
        this.BlendState = blendState;
    }
}
