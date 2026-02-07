using System.Numerics;
using Bliss.CSharp.Colors;

namespace Sparkle.CSharp.GUI.Elements.Data;

public class RectangleDropDownData {
    
    /// <summary>
    /// The base color used to render the dropdown field.
    /// </summary>
    public Color FieldColor;
    
    /// <summary>
    /// The color applied to the dropdown field when hovered.
    /// </summary>
    public Color FieldHoverColor;
    
    /// <summary>
    /// The color applied to the dropdown field when disabled.
    /// </summary>
    public Color DisabledFieldColor;
    
    /// <summary>
    /// The thickness of the outline drawn around the dropdown field.
    /// </summary>
    public float FieldOutlineThickness;
    
    /// <summary>
    /// The color of the dropdown field outline.
    /// </summary>
    public Color FieldOutlineColor;
    
    /// <summary>
    /// The color of the dropdown field outline when hovered.
    /// </summary>
    public Color FieldOutlineHoverColor;
    
    /// <summary>
    /// The color of the dropdown field outline when disabled.
    /// </summary>
    public Color DisabledFieldOutlineColor;
    
    /// <summary>
    /// The base color used to render the dropdown menu background.
    /// </summary>
    public Color MenuColor;
    
    /// <summary>
    /// The color applied to the dropdown menu when hovered.
    /// </summary>
    public Color MenuHoverColor;
    
    /// <summary>
    /// The color applied to the dropdown menu when disabled.
    /// </summary>
    public Color DisabledMenuColor;
    
    /// <summary>
    /// The thickness of the outline drawn around the dropdown menu.
    /// </summary>
    public float MenuOutlineThickness;
    
    /// <summary>
    /// The color of the dropdown menu outline.
    /// </summary>
    public Color MenuOutlineColor;
    
    /// <summary>
    /// The color of the dropdown menu outline when hovered.
    /// </summary>
    public Color MenuOutlineHoverColor;
    
    /// <summary>
    /// The color of the dropdown menu outline when disabled.
    /// </summary>
    public Color DisabledMenuOutlineColor;
    
    /// <summary>
    /// The width of the scrollbar bar within the dropdown menu.
    /// </summary>
    public int SliderBarWidth;
    
    /// <summary>
    /// The base color used to render the scrollbar bar.
    /// </summary>
    public Color SliderBarColor;
    
    /// <summary>
    /// The color applied to the scrollbar bar when hovered.
    /// </summary>
    public Color SliderBarHoverColor;
    
    /// <summary>
    /// The color applied to the scrollbar bar when disabled.
    /// </summary>
    public Color DisabledSliderBarColor;
    
    /// <summary>
    /// The size of the scrollbar slider handle.
    /// </summary>
    public Vector2 SliderSize;
    
    /// <summary>
    /// The base color used to render the scrollbar slider handle.
    /// </summary>
    public Color SliderColor;
    
    /// <summary>
    /// The color applied to the scrollbar slider handle when hovered.
    /// </summary>
    public Color SliderHoverColor;
    
    /// <summary>
    /// The color applied to the scrollbar slider handle when disabled.
    /// </summary>
    public Color DisabledSliderColor;
    
    /// <summary>
    /// The thickness of the outline drawn around the scrollbar slider handle.
    /// </summary>
    public float SliderOutlineThickness;
    
    /// <summary>
    /// The color of the scrollbar slider outline.
    /// </summary>
    public Color SliderOutlineColor;
    
    /// <summary>
    /// The color of the scrollbar slider outline when hovered.
    /// </summary>
    public Color SliderOutlineHoverColor;
    
    /// <summary>
    /// The color of the scrollbar slider outline when disabled.
    /// </summary>
    public Color DisabledSliderOutlineColor;
    
    /// <summary>
    /// The size of the dropdown arrow indicator.
    /// </summary>
    public Vector2? ArrowSize;
    
    /// <summary>
    /// The base color used to render the dropdown arrow.
    /// </summary>
    public Color ArrowColor;
    
    /// <summary>
    /// The color applied to the dropdown arrow when hovered.
    /// </summary>
    public Color ArrowHoverColor;
    
    /// <summary>
    /// The color applied to the dropdown arrow when disabled.
    /// </summary>
    public Color DisabledArrowColor;
    
    /// <summary>
    /// The color used to highlight hovered or selected menu items.
    /// </summary>
    public Color HighlightColor;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="RectangleDropDownData"/> class,
    /// defining all visual styles for a rectangle-based dropdown element.
    /// </summary>
    /// <param name="fieldColor">Base color of the dropdown field.</param>
    /// <param name="fieldHoverColor">Color applied to the field when hovered.</param>
    /// <param name="disabledFieldColor">Color applied to the field when disabled.</param>
    /// <param name="fieldOutlineThickness">Outline thickness of the dropdown field.</param>
    /// <param name="fieldOutlineColor">Outline color of the dropdown field.</param>
    /// <param name="fieldOutlineHoverColor">Outline color of the field when hovered.</param>
    /// <param name="disabledFieldOutlineColor">Outline color of the field when disabled.</param>
    /// <param name="menuColor">Base color of the dropdown menu.</param>
    /// <param name="menuHoverColor">Color applied to the menu when hovered.</param>
    /// <param name="disabledMenuColor">Color applied to the menu when disabled.</param>
    /// <param name="menuOutlineThickness">Outline thickness of the dropdown menu.</param>
    /// <param name="menuOutlineColor">Outline color of the dropdown menu.</param>
    /// <param name="menuOutlineHoverColor">Outline color of the menu when hovered.</param>
    /// <param name="disabledMenuOutlineColor">Outline color of the menu when disabled.</param>
    /// <param name="sliderBarWidth">Width of the scrollbar bar.</param>
    /// <param name="sliderBarColor">Base color of the scrollbar bar.</param>
    /// <param name="sliderBarHoverColor">Color applied to the scrollbar bar when hovered.</param>
    /// <param name="disabledSliderBarColor">Color applied to the scrollbar bar when disabled.</param>
    /// <param name="sliderSize">Size of the scrollbar slider handle.</param>
    /// <param name="sliderColor">Base color of the scrollbar slider handle.</param>
    /// <param name="sliderHoverColor">Color applied to the slider when hovered.</param>
    /// <param name="disabledSliderColor">Color applied to the slider when disabled.</param>
    /// <param name="sliderOutlineThickness">Outline thickness of the slider handle.</param>
    /// <param name="sliderOutlineColor">Outline color of the slider handle.</param>
    /// <param name="sliderOutlineHoverColor">Outline color of the slider when hovered.</param>
    /// <param name="disabledSliderOutlineColor">Outline color of the slider when disabled.</param>
    /// <param name="arrowSize">Size of the dropdown arrow indicator.</param>
    /// <param name="arrowColor">Base color of the dropdown arrow.</param>
    /// <param name="arrowHoverColor">Color applied to the arrow when hovered.</param>
    /// <param name="disabledArrowColor">Color applied to the arrow when disabled.</param>
    /// <param name="highlightColor">Highlight color for hovered or selected menu items.</param>
    public RectangleDropDownData(
        Color? fieldColor = null,
        Color? fieldHoverColor = null,
        Color? disabledFieldColor = null,
        float fieldOutlineThickness = 0.0F,
        Color? fieldOutlineColor = null,
        Color? fieldOutlineHoverColor = null,
        Color? disabledFieldOutlineColor = null,
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
        Vector2? sliderSize = null,
        Color? sliderColor = null,
        Color? sliderHoverColor = null,
        Color? disabledSliderColor = null,
        float sliderOutlineThickness = 0.0F,
        Color? sliderOutlineColor = null,
        Color? sliderOutlineHoverColor = null,
        Color? disabledSliderOutlineColor = null,
        Vector2? arrowSize = null,
        Color? arrowColor = null,
        Color? arrowHoverColor = null,
        Color? disabledArrowColor = null,
        Color? highlightColor = null) {
        this.FieldColor = fieldColor ?? Color.Gray;
        this.FieldHoverColor = fieldHoverColor ?? this.FieldColor;
        this.DisabledFieldColor = disabledFieldColor ?? Color.DarkGray;
        this.FieldOutlineThickness = fieldOutlineThickness;
        this.FieldOutlineColor = fieldOutlineColor ?? Color.DarkGray;
        this.FieldOutlineHoverColor = fieldOutlineHoverColor ?? this.FieldOutlineColor;
        this.DisabledFieldOutlineColor = disabledFieldOutlineColor ?? Color.DarkGray;
        
        this.MenuColor = menuColor ?? Color.Gray;
        this.MenuHoverColor = menuHoverColor ?? this.MenuColor;
        this.DisabledMenuColor = disabledMenuColor ?? Color.DarkGray;
        this.MenuOutlineThickness = menuOutlineThickness;
        this.MenuOutlineColor = menuOutlineColor ?? Color.DarkGray;
        this.MenuOutlineHoverColor = menuOutlineHoverColor ?? this.MenuOutlineColor;
        this.DisabledMenuOutlineColor = disabledMenuOutlineColor ?? Color.DarkGray;
        
        this.SliderBarWidth = sliderBarWidth;
        this.SliderBarColor = sliderBarColor ?? Color.Gray;
        this.SliderBarHoverColor = sliderBarHoverColor ?? this.SliderBarColor;
        this.DisabledSliderBarColor = disabledSliderBarColor ?? Color.DarkGray;
        
        this.SliderSize = sliderSize ?? new Vector2(10, 10);
        this.SliderColor = sliderColor ?? Color.LightGray;
        this.SliderHoverColor = sliderHoverColor ?? this.SliderColor;
        this.DisabledSliderColor = disabledSliderColor ?? Color.Gray;
        this.SliderOutlineThickness = sliderOutlineThickness;
        this.SliderOutlineColor = sliderOutlineColor ?? Color.DarkGray;
        this.SliderOutlineHoverColor = sliderOutlineHoverColor ?? this.SliderOutlineColor;
        this.DisabledSliderOutlineColor = disabledSliderOutlineColor ?? Color.DarkGray;
        
        this.ArrowSize = arrowSize ?? new Vector2(14, 10);
        this.ArrowColor = arrowColor ?? Color.White;
        this.ArrowHoverColor = arrowHoverColor ?? this.ArrowColor;
        this.DisabledArrowColor = disabledArrowColor ?? Color.Gray;
        
        this.HighlightColor = highlightColor ?? new Color(100, 100, 100, 255);
    }
}