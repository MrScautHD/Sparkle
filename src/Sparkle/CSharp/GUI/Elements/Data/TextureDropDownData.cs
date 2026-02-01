using Bliss.CSharp.Colors;
using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Sprites;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
using Veldrid;

namespace Sparkle.CSharp.GUI.Elements.Data;

public class TextureDropDownData {
    
    /// <summary>
    /// The texture used to render the dropdown field (collapsed state).
    /// </summary>
    public Texture2D FieldTexture;
    
    /// <summary>
    /// The texture used to render the dropdown menu background.
    /// </summary>
    public Texture2D MenuTexture;
    
    /// <summary>
    /// The texture used to render the scrollbar track inside the dropdown menu.
    /// </summary>
    public Texture2D SliderBarTexture;
    
    /// <summary>
    /// The texture used to render the scrollbar handle inside the dropdown menu.
    /// </summary>
    public Texture2D SliderTexture;
    
    /// <summary>
    /// The texture used to render the dropdown arrow indicator.
    /// </summary>
    public Texture2D? ArrowTexture;
    
    /// <summary>
    /// The sampler used for sampling the dropdown field texture.
    /// </summary>
    public Sampler? FieldSampler;
    
    /// <summary>
    /// The sampler used for sampling the dropdown menu texture.
    /// </summary>
    public Sampler? MenuSampler;
    
    /// <summary>
    /// The sampler used for sampling the scrollbar track texture.
    /// </summary>
    public Sampler? SliderBarSampler;
    
    /// <summary>
    /// The sampler used for sampling the scrollbar handle texture.
    /// </summary>
    public Sampler? SliderSampler;
    
    /// <summary>
    /// The sampler used for sampling the dropdown arrow texture.
    /// </summary>
    public Sampler? ArrowSampler;
    
    /// <summary>
    /// The source rectangle defining the region of the field texture to draw.
    /// </summary>
    public Rectangle FieldSourceRect;
    
    /// <summary>
    /// The source rectangle defining the region of the menu texture to draw.
    /// </summary>
    public Rectangle MenuSourceRect;
    
    /// <summary>
    /// The source rectangle defining the region of the scrollbar track texture to draw.
    /// </summary>
    public Rectangle SliderBarSourceRect;
    
    /// <summary>
    /// The source rectangle defining the region of the scrollbar handle texture to draw.
    /// </summary>
    public Rectangle SliderSourceRect;
    
    /// <summary>
    /// The source rectangle defining the region of the arrow texture to draw.
    /// </summary>
    public Rectangle ArrowSourceRect;
    
    /// <summary>
    /// The resize mode applied to the dropdown field texture.
    /// </summary>
    public ResizeMode FieldResizeMode;
    
    /// <summary>
    /// The resize mode applied to the dropdown menu texture.
    /// </summary>
    public ResizeMode MenuResizeMode;
    
    /// <summary>
    /// The resize mode applied to the scrollbar track texture.
    /// </summary>
    public ResizeMode SliderBarResizeMode;
    
    /// <summary>
    /// The border insets used for nine-slice resizing of the field texture.
    /// </summary>
    public BorderInsets FieldBorderInsets;
    
    /// <summary>
    /// The border insets used for nine-slice resizing of the menu texture.
    /// </summary>
    public BorderInsets MenuBorderInsets;
    
    /// <summary>
    /// The border insets used for nine-slice resizing of the scrollbar track texture.
    /// </summary>
    public BorderInsets SliderBarBorderInsets;
    
    /// <summary>
    /// The base color applied to the dropdown field.
    /// </summary>
    public Color FieldColor;
    
    /// <summary>
    /// The base color applied to the dropdown menu.
    /// </summary>
    public Color MenuColor;
    
    /// <summary>
    /// The base color applied to the scrollbar track.
    /// </summary>
    public Color SliderBarColor;
    
    /// <summary>
    /// The base color applied to the scrollbar handle.
    /// </summary>
    public Color SliderColor;
    
    /// <summary>
    /// The base color applied to the dropdown arrow.
    /// </summary>
    public Color ArrowColor;
    
    /// <summary>
    /// The color applied to the dropdown field when hovered.
    /// </summary>
    public Color FieldHoverColor;
    
    /// <summary>
    /// The color applied to the dropdown menu when hovered.
    /// </summary>
    public Color MenuHoverColor;
    
    /// <summary>
    /// The color applied to the scrollbar track when hovered.
    /// </summary>
    public Color SliderBarHoverColor;
    
    /// <summary>
    /// The color applied to the scrollbar handle when hovered.
    /// </summary>
    public Color SliderHoverColor;
    
    /// <summary>
    /// The color applied to the dropdown arrow when hovered.
    /// </summary>
    public Color ArrowHoverColor;
    
    /// <summary>
    /// The color used to highlight hovered or selected menu items.
    /// </summary>
    public Color HighlightColor;
    
    /// <summary>
    /// The color applied to the dropdown field when disabled.
    /// </summary>
    public Color DisabledFieldColor;
    
    /// <summary>
    /// The color applied to the dropdown menu when disabled.
    /// </summary>
    public Color DisabledMenuColor;
    
    /// <summary>
    /// The color applied to the scrollbar track when the dropdown is disabled.
    /// </summary>
    public Color DisabledSliderBarColor;
    
    /// <summary>
    /// The color applied to the scrollbar handle when the dropdown is disabled.
    /// </summary>
    public Color DisabledSliderColor;
    
    /// <summary>
    /// The color applied to the dropdown arrow when disabled.
    /// </summary>
    public Color DisabledArrowColor;
    
    /// <summary>
    /// The flip mode applied to the dropdown field texture.
    /// </summary>
    public SpriteFlip FieldFlip;
    
    /// <summary>
    /// The flip mode applied to the dropdown menu texture.
    /// </summary>
    public SpriteFlip MenuFlip;
    
    /// <summary>
    /// The flip mode applied to the scrollbar track texture.
    /// </summary>
    public SpriteFlip SliderBarFlip;
    
    /// <summary>
    /// The flip mode applied to the scrollbar handle texture.
    /// </summary>
    public SpriteFlip SliderFlip;
    
    /// <summary>
    /// The flip mode applied to the dropdown arrow texture.
    /// </summary>
    public SpriteFlip ArrowFlip;
    
    /// <summary>
    /// The width of the scrollbar rendered inside the dropdown menu.
    /// </summary>
    public int ScrollBarWidth;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TextureDropDownData"/> class, defining all visual, layout, and interaction states for a texture-based dropdown.
    /// </summary>
    /// <param name="fieldTexture">The texture used to render the dropdown field in its collapsed state.</param>
    /// <param name="menuTexture">The texture used to render the dropdown menu background.</param>
    /// <param name="sliderBarTexture">Optional texture for the dropdown menu's scrollbar track.</param>
    /// <param name="sliderTexture">Optional texture for the dropdown menu's scrollbar handle.</param>
    /// <param name="arrowTexture">The texture used to render the dropdown arrow indicator.</param>
    /// <param name="fieldSampler">Optional sampler used for sampling the field texture.</param>
    /// <param name="menuSampler">Optional sampler used for sampling the menu texture.</param>
    /// <param name="sliderBarSampler">Optional sampler used for sampling the slider bar texture.</param>
    /// <param name="sliderSampler">Optional sampler used for sampling the slider texture.</param>
    /// <param name="arrowSampler">Optional sampler used for sampling the arrow texture.</param>
    /// <param name="fieldSourceRect">Optional source rectangle for the field texture. Defaults to the full texture.</param>
    /// <param name="menuSourceRect">Optional source rectangle for the menu texture. Defaults to the full texture.</param>
    /// <param name="sliderBarSourceRect">Optional source rectangle for the slider bar texture. Defaults to the full texture.</param>
    /// <param name="sliderSourceRect">Optional source rectangle for the slider texture. Defaults to the full texture.</param>
    /// <param name="arrowSourceRect">Optional source rectangle for the arrow texture. Defaults to the full texture.</param>
    /// <param name="fieldResizeMode">Resize mode applied to the field texture.</param>
    /// <param name="menuResizeMode">Resize mode applied to the menu texture.</param>
    /// <param name="sliderBarResizeMode">Resize mode applied to the slider bar texture.</param>
    /// <param name="fieldBorderInsets">Border insets used for nine-slice resizing of the field texture.</param>
    /// <param name="menuBorderInsets">Border insets used for nine-slice resizing of the menu texture.</param>
    /// <param name="sliderBarBorderInsets">Border insets used for nine-slice resizing of the slider bar texture.</param>
    /// <param name="fieldColor">Base color applied to the dropdown field.</param>
    /// <param name="menuColor">Base color applied to the dropdown menu.</param>
    /// <param name="sliderBarColor">Base color applied to the scrollbar bar.</param>
    /// <param name="sliderColor">Base color applied to the scrollbar handle.</param>
    /// <param name="arrowColor">Base color applied to the dropdown arrow.</param>
    /// <param name="fieldHoverColor">Color applied to the field when hovered.</param>
    /// <param name="menuHoverColor">Color applied to the menu when hovered.</param>
    /// <param name="sliderBarHoverColor">Color applied to the scrollbar bar when hovered.</param>
    /// <param name="sliderHoverColor">Color applied to the scrollbar handle when hovered.</param>
    /// <param name="arrowHoverColor">Color applied to the arrow when hovered.</param>
    /// <param name="highlightColor">Color used to highlight hovered or selected menu entries.</param>
    /// <param name="disabledFieldColor">Color applied to the field when the dropdown is disabled.</param>
    /// <param name="disabledMenuColor">Color applied to the menu when the dropdown is disabled.</param>
    /// <param name="disabledSliderBarColor">Color applied to the scrollbar bar when the dropdown is disabled.</param>
    /// <param name="disabledSliderColor">Color applied to the scrollbar handle when the dropdown is disabled.</param>
    /// <param name="disabledArrowColor">Color applied to the arrow when the dropdown is disabled.</param>
    /// <param name="fieldFlip">Flip mode applied to the field texture.</param>
    /// <param name="menuFlip">Flip mode applied to the menu texture.</param>
    /// <param name="sliderBarFlip">Flip mode applied to the slider bar texture.</param>
    /// <param name="sliderFlip">Flip mode applied to the slider texture.</param>
    /// <param name="arrowFlip">Flip mode applied to the arrow texture.</param>
    /// <param name="scrollBarWidth">The width of the scrollbar in the dropdown menu.</param>
    public TextureDropDownData(
        Texture2D fieldTexture,
        Texture2D menuTexture,
        Texture2D sliderBarTexture,
        Texture2D sliderTexture,
        Texture2D? arrowTexture,
        Sampler? fieldSampler = null,
        Sampler? menuSampler = null,
        Sampler? sliderBarSampler = null,
        Sampler? sliderSampler = null,
        Sampler? arrowSampler = null,
        Rectangle? fieldSourceRect = null,
        Rectangle? menuSourceRect = null,
        Rectangle? sliderBarSourceRect = null,
        Rectangle? sliderSourceRect = null,
        Rectangle? arrowSourceRect = null,
        ResizeMode fieldResizeMode = ResizeMode.None,
        ResizeMode menuResizeMode = ResizeMode.None,
        ResizeMode sliderBarResizeMode = ResizeMode.None,
        BorderInsets? fieldBorderInsets = null,
        BorderInsets? menuBorderInsets = null,
        BorderInsets? sliderBarBorderInsets = null,
        Color? fieldColor = null,
        Color? menuColor = null,
        Color? sliderBarColor = null,
        Color? sliderColor = null,
        Color? arrowColor = null,
        Color? fieldHoverColor = null,
        Color? menuHoverColor = null,
        Color? sliderBarHoverColor = null,
        Color? sliderHoverColor = null,
        Color? arrowHoverColor = null,
        Color? highlightColor = null,
        Color? disabledFieldColor = null,
        Color? disabledMenuColor = null,
        Color? disabledSliderBarColor = null,
        Color? disabledSliderColor = null,
        Color? disabledArrowColor = null,
        SpriteFlip fieldFlip = SpriteFlip.None,
        SpriteFlip menuFlip = SpriteFlip.None,
        SpriteFlip sliderBarFlip = SpriteFlip.None,
        SpriteFlip sliderFlip = SpriteFlip.None,
        SpriteFlip arrowFlip = SpriteFlip.None,
        int scrollBarWidth = 16) {
        this.FieldTexture = fieldTexture;
        this.MenuTexture = menuTexture;
        this.ArrowTexture = arrowTexture;
        this.SliderBarTexture = sliderBarTexture;
        this.SliderTexture = sliderTexture;
        this.FieldSampler = fieldSampler;
        this.MenuSampler = menuSampler;
        this.ArrowSampler = arrowSampler;
        this.SliderBarSampler = sliderBarSampler;
        this.SliderSampler = sliderSampler;
        this.FieldSourceRect = fieldSourceRect ?? new Rectangle(0, 0, (int) fieldTexture.Width, (int) fieldTexture.Height);
        this.MenuSourceRect = menuSourceRect ?? new Rectangle(0, 0, (int) menuTexture.Width, (int) menuTexture.Height);
        this.SliderBarSourceRect = sliderBarSourceRect ?? new Rectangle(0, 0, (int) sliderBarTexture.Width, (int) sliderBarTexture.Height);
        this.SliderSourceRect = sliderSourceRect ?? new Rectangle(0, 0, (int) sliderTexture.Width, (int) sliderTexture.Height);
        this.ArrowSourceRect = arrowSourceRect ?? (arrowTexture != null ? new Rectangle(0, 0, (int) arrowTexture.Width, (int) arrowTexture.Height) : new Rectangle());
        this.FieldResizeMode = fieldResizeMode;
        this.MenuResizeMode = menuResizeMode;
        this.SliderBarResizeMode = sliderBarResizeMode;
        this.FieldBorderInsets = fieldBorderInsets ?? BorderInsets.Zero;
        this.MenuBorderInsets = menuBorderInsets ?? BorderInsets.Zero;
        this.SliderBarBorderInsets = sliderBarBorderInsets ?? BorderInsets.Zero;
        this.FieldColor = fieldColor ?? Color.White;
        this.MenuColor = menuColor ?? Color.White;
        this.SliderBarColor = sliderBarColor ?? Color.White;
        this.SliderColor = sliderColor ?? Color.White;
        this.ArrowColor = arrowColor ?? Color.White;
        this.FieldHoverColor = fieldHoverColor ?? this.FieldColor;
        this.MenuHoverColor = menuHoverColor ?? this.MenuColor;
        this.SliderBarHoverColor = sliderBarHoverColor ?? this.SliderBarColor;
        this.SliderHoverColor = sliderHoverColor ?? this.SliderColor;
        this.ArrowHoverColor = arrowHoverColor ?? this.ArrowColor;
        this.HighlightColor = highlightColor ?? Color.LightGray;
        this.DisabledFieldColor = disabledFieldColor ?? Color.Gray;
        this.DisabledMenuColor = disabledMenuColor ?? Color.Gray;
        this.DisabledSliderBarColor = disabledSliderBarColor ?? Color.Gray;
        this.DisabledSliderColor = disabledSliderColor ?? Color.Gray;
        this.DisabledArrowColor = disabledArrowColor ?? Color.Gray;
        this.FieldFlip = fieldFlip;
        this.MenuFlip = menuFlip;
        this.SliderBarFlip = sliderBarFlip;
        this.SliderFlip = sliderFlip; 
        this.ArrowFlip = arrowFlip; 
        this.ScrollBarWidth = scrollBarWidth; 
    }
}