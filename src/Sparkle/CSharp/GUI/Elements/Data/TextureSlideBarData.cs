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
    /// The texture used to render the slider handle.
    /// </summary>
    public Texture2D SliderTexture;
    
    /// <summary>
    /// The sampler used for sampling the bar texture.
    /// </summary>
    public Sampler? BarSampler;
    
    /// <summary>
    /// The sampler used for sampling the slider texture.
    /// </summary>
    public Sampler? SliderSampler;
    
    /// <summary>
    /// The source rectangle defining the bar texture region.
    /// </summary>
    public Rectangle BarSourceRect;
    
    /// <summary>
    /// The source rectangle defining the slider texture region.
    /// </summary>
    public Rectangle SliderSourceRect;
    
    /// <summary>
    /// The resize mode applied to the bar texture.
    /// </summary>
    public ResizeMode BarResizeMode;
    
    /// <summary>
    /// The border insets used for nine-slice bar resizing.
    /// </summary>
    public BorderInsets BarBorderInsets;
    
    /// <summary>
    /// The base color applied to the bar texture.
    /// </summary>
    public Color BarColor;
    
    /// <summary>
    /// The base color applied to the slider texture.
    /// </summary>
    public Color SliderColor;
    
    /// <summary>
    /// The color applied to the bar texture when hovered.
    /// </summary>
    public Color BarHoverColor;
    
    /// <summary>
    /// The color applied to the slider texture when hovered.
    /// </summary>
    public Color SliderHoverColor;
    
    /// <summary>
    /// The color applied to the entire slide bar when disabled.
    /// </summary>
    public Color DisabledBarColor;
    
    /// <summary>
    /// The color used to render the slider when it is in a disabled state.
    /// </summary>
    public Color DisabledSliderColor;
    
    /// <summary>
    /// The flip mode applied to the bar texture.
    /// </summary>
    public SpriteFlip BarFlip;
    
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
    /// <param name="emptyBarSourceRect">The source rectangle for the bar texture.</param>
    /// <param name="sliderSourceRect">The source rectangle for the slider texture.</param>
    /// <param name="barResizeMode">The resize mode applied to the bar texture.</param>
    /// <param name="barBorderInsets">The border insets used for nine-slice resizing.</param>
    /// <param name="emptyBarColor">The base color applied to the bar texture.</param>
    /// <param name="sliderColor">The base color applied to the slider texture.</param>
    /// <param name="emptyBarHoverColor">The color applied to the bar texture when hovered.</param>
    /// <param name="sliderHoverColor">The color applied to the slider texture when hovered.</param>
    /// <param name="disabledBarColor">The color applied when the slide bar is disabled.</param>
    /// <param name="disabledSliderColor">The color applied when the slider is disabled.</param>
    /// <param name="barFlip">The flip mode applied to the bar texture.</param>
    /// <param name="sliderFlip">The flip mode applied to the slider texture.</param>
    public TextureSlideBarData(
        Texture2D barTexture,
        Texture2D sliderTexture,
        Sampler? barSampler = null,
        Sampler? sliderSampler = null,
        Rectangle? emptyBarSourceRect = null,
        Rectangle? sliderSourceRect = null,
        ResizeMode barResizeMode = ResizeMode.None,
        BorderInsets? barBorderInsets = null,
        Color? emptyBarColor = null,
        Color? sliderColor = null,
        Color? emptyBarHoverColor = null,
        Color? sliderHoverColor = null,
        Color? disabledBarColor = null,
        Color? disabledSliderColor = null,
        SpriteFlip barFlip = SpriteFlip.None,
        SpriteFlip sliderFlip = SpriteFlip.None) {
        this.BarTexture = barTexture;
        this.SliderTexture = sliderTexture;
        this.BarSampler = barSampler;
        this.SliderSampler = sliderSampler;
        this.BarSourceRect = emptyBarSourceRect ?? new Rectangle(0, 0, (int) barTexture.Width, (int) barTexture.Height);
        this.SliderSourceRect = sliderSourceRect ?? new Rectangle(0, 0, (int) sliderTexture.Width, (int) sliderTexture.Height);
        this.BarResizeMode = barResizeMode;
        this.BarBorderInsets = barBorderInsets ?? BorderInsets.Zero;
        this.BarColor = emptyBarColor ?? Color.White;
        this.SliderColor = sliderColor ?? Color.White;
        this.BarHoverColor = emptyBarHoverColor ?? this.BarColor;
        this.SliderHoverColor = sliderHoverColor ?? this.SliderColor;
        this.DisabledBarColor = disabledBarColor ?? Color.Gray;
        this.DisabledSliderColor = disabledSliderColor ?? Color.DarkGray;
        this.BarFlip = barFlip;
        this.SliderFlip = sliderFlip;
    }
}