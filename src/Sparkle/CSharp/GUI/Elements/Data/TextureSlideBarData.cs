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

    public Texture2D? FilledBarTexture;
    
    /// <summary>
    /// The texture used to render the slider handle.
    /// </summary>
    public Texture2D? SliderTexture;
    
    /// <summary>
    /// The sampler used for sampling the bar texture.
    /// </summary>
    public Sampler? BarSampler;
    
    public Sampler? FilledBarSampler;
    
    /// <summary>
    /// The sampler used for sampling the slider texture.
    /// </summary>
    public Sampler? SliderSampler;
    
    /// <summary>
    /// The source rectangle defining the bar texture region.
    /// </summary>
    public Rectangle BarSourceRect;
    
    public Rectangle FilledBarSourceRect;
    
    /// <summary>
    /// The source rectangle defining the slider texture region.
    /// </summary>
    public Rectangle SliderSourceRect;
    
    /// <summary>
    /// The resize mode applied to the bar texture.
    /// </summary>
    public ResizeMode BarResizeMode;
    
    public ResizeMode FilledBarResizeMode;
    
    /// <summary>
    /// The border insets used for nine-slice bar resizing.
    /// </summary>
    public BorderInsets BarBorderInsets;
    
    public BorderInsets FilledBarBorderInsets;
    
    /// <summary>
    /// The base color applied to the bar texture.
    /// </summary>
    public Color BarColor;
    
    public Color FilledBarColor;

    /// <summary>
    /// The base color applied to the slider texture.
    /// </summary>
    public Color SliderColor;
    
    /// <summary>
    /// The color applied to the bar texture when hovered.
    /// </summary>
    public Color BarHoverColor;
    
    public Color FilledBarHoverColor;
    
    /// <summary>
    /// The color applied to the slider texture when hovered.
    /// </summary>
    public Color SliderHoverColor;
    
    /// <summary>
    /// The color applied to the entire slide bar when disabled.
    /// </summary>
    public Color DisabledBarColor;
    
    public Color DisabledFilledBarColor;
    
    /// <summary>
    /// The color used to render the slider when it is in a disabled state.
    /// </summary>
    public Color DisabledSliderColor;
    
    /// <summary>
    /// The flip mode applied to the bar texture.
    /// </summary>
    public SpriteFlip BarFlip;
    
    public SpriteFlip FilledBarFlip;
    
    /// <summary>
    /// The flip mode applied to the slider texture.
    /// </summary>
    public SpriteFlip SliderFlip;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TextureSlideBarData"/> class.
    /// </summary>
    /// <param name="barTexture">The texture used to render the slide bar background.</param>
    /// <param name="sliderTexture">The texture used to render the slider handle.</param>
    /// <param name="barSampler">The sampler used for sampling the bar texture.</param>
    /// <param name="sliderSampler">The sampler used for sampling the slider texture.</param>
    /// <param name="barSourceRect">The source rectangle for the bar texture.</param>
    /// <param name="sliderSourceRect">The source rectangle for the slider texture.</param>
    /// <param name="barResizeMode">The resize mode applied to the bar texture.</param>
    /// <param name="barBorderInsets">The border insets used for nine-slice resizing.</param>
    /// <param name="barColor">The base color applied to the bar texture.</param>
    /// <param name="sliderColor">The base color applied to the slider texture.</param>
    /// <param name="barHoverColor">The color applied to the bar texture when hovered.</param>
    /// <param name="sliderHoverColor">The color applied to the slider texture when hovered.</param>
    /// <param name="disabledBarColor">The color applied when the slide bar is disabled.</param>
    /// <param name="disabledSliderColor">The color applied when the slider is disabled.</param>
    /// <param name="barFlip">The flip mode applied to the bar texture.</param>
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
        this.FilledBarSourceRect = filledBarSourceRect ?? new Rectangle(0, 0, (int) filledBarTexture.Width, (int) filledBarTexture.Height);
        this.SliderSourceRect = sliderSourceRect ?? new Rectangle(0, 0, (int) sliderTexture.Width, (int) sliderTexture.Height);
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
        this.DisabledFilledBarColor = disabledFilledBarColor ?? Color.DarkGray;
        this.DisabledSliderColor = disabledSliderColor ?? Color.DarkGray;
        this.BarFlip = barFlip;
        this.FilledBarFlip = filledBarFlip;
        this.SliderFlip = sliderFlip;
    }
}