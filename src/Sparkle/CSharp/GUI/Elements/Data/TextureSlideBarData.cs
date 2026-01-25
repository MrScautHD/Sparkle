using Bliss.CSharp.Colors;
using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Sprites;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
using Veldrid;

namespace Sparkle.CSharp.GUI.Elements.Data;

public class TextureSlideBarData {
    
    /// <summary>
    /// The texture used to render the slide bar background.
    /// </summary>
    public Texture2D BarTexture;
    
    /// <summary>
    /// The optional texture used to render the filled portion of the slide bar.
    /// </summary>
    public Texture2D? FilledBarTexture;
    
    /// <summary>
    /// The optional texture used to render the slider handle.
    /// </summary>
    public Texture2D? SliderTexture;
    
    /// <summary>
    /// The sampler used for sampling the bar texture.
    /// </summary>
    public Sampler? BarSampler;
    
    /// <summary>
    /// The sampler used for sampling the filled bar texture.
    /// </summary>
    public Sampler? FilledBarSampler;
    
    /// <summary>
    /// The sampler used for sampling the slider texture.
    /// </summary>
    public Sampler? SliderSampler;
    
    /// <summary>
    /// The source rectangle defining the visible region of the bar texture.
    /// </summary>
    public Rectangle BarSourceRect;
    
    /// <summary>
    /// The source rectangle defining the visible region of the filled bar texture.
    /// </summary>
    public Rectangle FilledBarSourceRect;
    
    /// <summary>
    /// The source rectangle defining the visible region of the slider texture.
    /// </summary>
    public Rectangle SliderSourceRect;
    
    /// <summary>
    /// The resize mode applied to the bar texture.
    /// </summary>
    public ResizeMode BarResizeMode;
    
    /// <summary>
    /// The resize mode applied to the filled bar texture.
    /// </summary>
    public ResizeMode FilledBarResizeMode;
    
    /// <summary>
    /// The border insets used for nine-slice resizing of the bar texture.
    /// </summary>
    public BorderInsets BarBorderInsets;
    
    /// <summary>
    /// The border insets used for nine-slice resizing of the filled bar texture.
    /// </summary>
    public BorderInsets FilledBarBorderInsets;
    
    /// <summary>
    /// The base color applied to the bar texture.
    /// </summary>
    public Color BarColor;
    
    /// <summary>
    /// The base color applied to the filled bar texture.
    /// </summary>
    public Color FilledBarColor;

    /// <summary>
    /// The base color applied to the slider texture.
    /// </summary>
    public Color SliderColor;
    
    /// <summary>
    /// The color applied to the bar texture when hovered.
    /// </summary>
    public Color BarHoverColor;
    
    /// <summary>
    /// The color applied to the filled bar texture when hovered.
    /// </summary>
    public Color FilledBarHoverColor;
    
    /// <summary>
    /// The color applied to the slider texture when hovered.
    /// </summary>
    public Color SliderHoverColor;
    
    /// <summary>
    /// The color applied to the entire slide bar when disabled.
    /// </summary>
    public Color DisabledBarColor;
    
    /// <summary>
    /// The color applied to the filled bar texture when the slide bar is disabled.
    /// </summary>
    public Color DisabledFilledBarColor;
    
    /// <summary>
    /// The color used to render the slider when it is in a disabled state.
    /// </summary>
    public Color DisabledSliderColor;
    
    /// <summary>
    /// The flip mode applied to the bar texture.
    /// </summary>
    public SpriteFlip BarFlip;
    
    /// <summary>
    /// The flip mode applied to the filled bar texture.
    /// </summary>
    public SpriteFlip FilledBarFlip;
    
    /// <summary>
    /// The flip mode applied to the slider texture.
    /// </summary>
    public SpriteFlip SliderFlip;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TextureSlideBarData"/> class.
    /// </summary>
    /// <param name="barTexture">The texture used to render the slide bar background.</param>
    /// <param name="filledBarTexture">The optional texture used to render the filled portion of the slide bar.</param>
    /// <param name="sliderTexture">The optional texture used to render the slider handle.</param>
    /// <param name="barSampler">The sampler used for sampling the bar texture.</param>
    /// <param name="filledBarSampler">The sampler used for sampling the filled bar texture.</param>
    /// <param name="sliderSampler">The sampler used for sampling the slider texture.</param>
    /// <param name="barSourceRect">The source rectangle defining the visible region of the bar texture.</param>
    /// <param name="filledBarSourceRect">The source rectangle defining the visible region of the filled bar texture.</param>
    /// <param name="sliderSourceRect">The source rectangle defining the visible region of the slider texture.</param>
    /// <param name="barResizeMode">The resize mode applied to the bar texture.</param>
    /// <param name="filledBarResizeMode">The resize mode applied to the filled bar texture.</param>
    /// <param name="barBorderInsets">The border insets used for nine-slice resizing of the bar texture.</param>
    /// <param name="filledBarBorderInsets">The border insets used for nine-slice resizing of the filled bar texture.</param>
    /// <param name="barColor">The base color applied to the bar texture.</param>
    /// <param name="filledBarColor">The base color applied to the filled bar texture.</param>
    /// <param name="sliderColor">The base color applied to the slider texture.</param>
    /// <param name="barHoverColor">The color applied to the bar texture when hovered.</param>
    /// <param name="filledBarHoverColor">The color applied to the filled bar texture when hovered.</param>
    /// <param name="sliderHoverColor">The color applied to the slider texture when hovered.</param>
    /// <param name="disabledBarColor">The color applied to the bar texture when disabled.</param>
    /// <param name="disabledFilledBarColor">The color applied to the filled bar texture when disabled.</param>
    /// <param name="disabledSliderColor">The color applied to the slider texture when disabled.</param>
    /// <param name="barFlip">The flip mode applied to the bar texture.</param>
    /// <param name="filledBarFlip">The flip mode applied to the filled bar texture.</param>
    /// <param name="sliderFlip">The flip mode applied to the slider texture.</param>
    public TextureSlideBarData(
        Texture2D barTexture,
        Texture2D? filledBarTexture,
        Texture2D? sliderTexture,
        Sampler? barSampler = null,
        Sampler? filledBarSampler = null,
        Sampler? sliderSampler = null,
        Rectangle? barSourceRect = null,
        Rectangle? filledBarSourceRect = null,
        Rectangle? sliderSourceRect = null,
        ResizeMode barResizeMode = ResizeMode.None,
        ResizeMode filledBarResizeMode = ResizeMode.None,
        BorderInsets? barBorderInsets = null,
        BorderInsets? filledBarBorderInsets = null,
        Color? barColor = null,
        Color? filledBarColor = null,
        Color? sliderColor = null,
        Color? barHoverColor = null,
        Color? filledBarHoverColor = null,
        Color? sliderHoverColor = null,
        Color? disabledBarColor = null,
        Color? disabledFilledBarColor = null,
        Color? disabledSliderColor = null,
        SpriteFlip barFlip = SpriteFlip.None,
        SpriteFlip filledBarFlip = SpriteFlip.None,
        SpriteFlip sliderFlip = SpriteFlip.None) {
        this.BarTexture = barTexture;
        this.FilledBarTexture = filledBarTexture;
        this.SliderTexture = sliderTexture;
        this.BarSampler = barSampler;
        this.FilledBarSampler = filledBarSampler;
        this.SliderSampler = sliderSampler;
        this.BarSourceRect = barSourceRect ?? new Rectangle(0, 0, (int) barTexture.Width, (int) barTexture.Height);
        this.FilledBarSourceRect = filledBarSourceRect ?? (filledBarTexture != null ? new Rectangle(0, 0, (int) filledBarTexture.Width, (int) filledBarTexture.Height) : new Rectangle());
        this.SliderSourceRect = sliderSourceRect ?? (sliderTexture != null ? new Rectangle(0, 0, (int) sliderTexture.Width, (int) sliderTexture.Height) : new Rectangle());
        this.BarResizeMode = barResizeMode;
        this.FilledBarResizeMode = filledBarResizeMode;
        this.BarBorderInsets = barBorderInsets ?? BorderInsets.Zero;
        this.FilledBarBorderInsets = filledBarBorderInsets ?? BorderInsets.Zero;
        this.BarColor = barColor ?? Color.White;
        this.FilledBarColor = filledBarColor ?? Color.White;
        this.SliderColor = sliderColor ?? Color.White;
        this.BarHoverColor = barHoverColor ?? this.BarColor;
        this.FilledBarHoverColor = filledBarHoverColor ?? this.FilledBarColor;
        this.SliderHoverColor = sliderHoverColor ?? this.SliderColor;
        this.DisabledBarColor = disabledBarColor ?? Color.Gray;
        this.DisabledFilledBarColor = disabledFilledBarColor ?? Color.Gray;
        this.DisabledSliderColor = disabledSliderColor ?? Color.Gray;
        this.BarFlip = barFlip;
        this.FilledBarFlip = filledBarFlip;
        this.SliderFlip = sliderFlip;
    }
}