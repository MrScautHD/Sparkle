using Bliss.CSharp.Colors;
using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Sprites;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
using Veldrith;

namespace Sparkle.CSharp.GUI.Elements.Data;

public class TextureScrollViewData {
    
    /// <summary>
    /// The texture used to render the scroll view background.
    /// </summary>
    public Texture2D BackgroundTexture;
    
    /// <summary>
    /// The texture used to render the slider bar track.
    /// </summary>
    public Texture2D SliderBarTexture;
    
    /// <summary>
    /// The texture used to render the slider handle.
    /// </summary>
    public Texture2D SliderTexture;
    
    /// <summary>
    /// The sampler used for sampling the background texture.
    /// </summary>
    public Sampler? BackgroundSampler;
    
    /// <summary>
    /// The sampler used for sampling the slider bar texture.
    /// </summary>
    public Sampler? SliderBarSampler;
    
    /// <summary>
    /// The sampler used for sampling the slider texture.
    /// </summary>
    public Sampler? SliderSampler;
    
    /// <summary>
    /// The source rectangle defining the region of the background texture to draw.
    /// </summary>
    public Rectangle BackgroundSourceRect;
    
    /// <summary>
    /// The source rectangle defining the region of the slider bar texture to draw.
    /// </summary>
    public Rectangle SliderBarSourceRect;
    
    /// <summary>
    /// The source rectangle defining the region of the slider texture to draw.
    /// </summary>
    public Rectangle SliderSourceRect;
    
    /// <summary>
    /// The resize mode applied to the background texture.
    /// </summary>
    public ResizeMode BackgroundResizeMode;
    
    /// <summary>
    /// The resize mode applied to the slider bar texture.
    /// </summary>
    public ResizeMode SliderBarResizeMode;
    
    /// <summary>
    /// The border insets used for nine-slice resizing of the background texture.
    /// </summary>
    public BorderInsets BackgroundBorderInsets;
    
    /// <summary>
    /// The border insets used for nine-slice resizing of the slider bar texture.
    /// </summary>
    public BorderInsets SliderBarBorderInsets;
    
    /// <summary>
    /// The base color applied to the background.
    /// </summary>
    public Color BackgroundColor;
    
    /// <summary>
    /// The base color applied to the slider bar.
    /// </summary>
    public Color SliderBarColor;
    
    /// <summary>
    /// The base color applied to the slider handle.
    /// </summary>
    public Color SliderColor;
    
    /// <summary>
    /// The color applied to the background when hovered.
    /// </summary>
    public Color BackgroundHoverColor;
    
    /// <summary>
    /// The color applied to the slider bar when hovered.
    /// </summary>
    public Color SliderBarHoverColor;
    
    /// <summary>
    /// The color applied to the slider handle when hovered.
    /// </summary>
    public Color SliderHoverColor;
    
    /// <summary>
    /// The color applied to the background when disabled.
    /// </summary>
    public Color DisabledBackgroundColor;
    
    /// <summary>
    /// The color applied to the slider bar when disabled.
    /// </summary>
    public Color DisabledSliderBarColor;
    
    /// <summary>
    /// The color applied to the slider handle when disabled.
    /// </summary>
    public Color DisabledSliderColor;
    
    /// <summary>
    /// The flip mode applied to the background texture.
    /// </summary>
    public SpriteFlip BackgroundFlip;
    
    /// <summary>
    /// The flip mode applied to the slider bar texture.
    /// </summary>
    public SpriteFlip SliderBarFlip;
    
    /// <summary>
    /// The flip mode applied to the slider texture.
    /// </summary>
    public SpriteFlip SliderFlip;
    
    /// <summary>
    /// When <c>true</c>, snaps the background texture position and origin to whole pixels.
    /// </summary>
    public bool BackgroundPixelSnap;
    
    /// <summary>
    /// When <c>true</c>, snaps the slider bar texture position and origin to whole pixels.
    /// </summary>
    public bool SliderBarPixelSnap;
    
    /// <summary>
    /// When <c>true</c>, snaps the slider texture position and origin to whole pixels.
    /// </summary>
    public bool SliderPixelSnap;
    
    /// <summary>
    /// The width of the slider bar.
    /// </summary>
    public int SliderBarWidth;

    /// <summary>
    /// Initializes a new instance of the <see cref="TextureScrollViewData"/> class.
    /// </summary>
    /// <param name="backgroundTexture">The texture used to render the scroll view background.</param>
    /// <param name="sliderBarTexture">The texture used to render the slider bar track.</param>
    /// <param name="sliderTexture">The texture used to render the slider handle.</param>
    /// <param name="backgroundSampler">The sampler used for sampling the background texture. Defaults to <c>null</c>.</param>
    /// <param name="sliderBarSampler">The sampler used for sampling the slider bar texture. Defaults to <c>null</c>.</param>
    /// <param name="sliderSampler">The sampler used for sampling the slider texture. Defaults to <c>null</c>.</param>
    /// <param name="backgroundSourceRect">The source rectangle defining the region of the background texture to draw. Defaults to the full texture bounds.</param>
    /// <param name="sliderBarSourceRect">The source rectangle defining the region of the slider bar texture to draw. Defaults to the full texture bounds.</param>
    /// <param name="sliderSourceRect">The source rectangle defining the region of the slider texture to draw. Defaults to the full texture bounds.</param>
    /// <param name="backgroundResizeMode">The resize mode applied to the background texture. Defaults to <see cref="ResizeMode.None"/>.</param>
    /// <param name="sliderBarResizeMode">The resize mode applied to the slider bar texture. Defaults to <see cref="ResizeMode.None"/>.</param>
    /// <param name="backgroundBorderInsets">The border insets used for nine-slice resizing of the background texture. Defaults to <see cref="BorderInsets.Zero"/>.</param>
    /// <param name="sliderBarBorderInsets">The border insets used for nine-slice resizing of the slider bar texture. Defaults to <see cref="BorderInsets.Zero"/>.</param>
    /// <param name="backgroundColor">The base color applied to the background. Defaults to <see cref="Color.White"/>.</param>
    /// <param name="sliderBarColor">The base color applied to the slider bar. Defaults to <see cref="Color.White"/>.</param>
    /// <param name="sliderColor">The base color applied to the slider handle. Defaults to <see cref="Color.White"/>.</param>
    /// <param name="backgroundHoverColor">The color applied to the background when hovered. Defaults to <paramref name="backgroundColor"/>.</param>
    /// <param name="sliderBarHoverColor">The color applied to the slider bar when hovered. Defaults to <paramref name="sliderBarColor"/>.</param>
    /// <param name="sliderHoverColor">The color applied to the slider handle when hovered. Defaults to <paramref name="sliderColor"/>.</param>
    /// <param name="disabledBackgroundColor">The color applied to the background when disabled. Defaults to <see cref="Color.Gray"/>.</param>
    /// <param name="disabledSliderBarColor">The color applied to the slider bar when disabled. Defaults to <see cref="Color.Gray"/>.</param>
    /// <param name="disabledSliderColor">The color applied to the slider handle when disabled. Defaults to <see cref="Color.Gray"/>.</param>
    /// <param name="backgroundFlip">The flip mode applied to the background texture. Defaults to <see cref="SpriteFlip.None"/>.</param>
    /// <param name="sliderBarFlip">The flip mode applied to the slider bar texture. Defaults to <see cref="SpriteFlip.None"/>.</param>
    /// <param name="sliderFlip">The flip mode applied to the slider texture. Defaults to <see cref="SpriteFlip.None"/>.</param>
    /// <param name="backgroundPixelSnap">When <c>true</c>, snaps the background texture position and origin to whole pixels. Defaults to <c>false</c>.</param>
    /// <param name="sliderBarPixelSnap">When <c>true</c>, snaps the slider bar texture position and origin to whole pixels. Defaults to <c>false</c>.</param>
    /// <param name="sliderPixelSnap">When <c>true</c>, snaps the slider texture position and origin to whole pixels. Defaults to <c>false</c>.</param>
    /// <param name="sliderBarWidth">The width of the slider bar in pixels. Defaults to <c>16</c>.</param>
    public TextureScrollViewData(
        Texture2D backgroundTexture,
        Texture2D sliderBarTexture,
        Texture2D sliderTexture,
        Sampler? backgroundSampler = null,
        Sampler? sliderBarSampler = null,
        Sampler? sliderSampler = null,
        Rectangle? backgroundSourceRect = null,
        Rectangle? sliderBarSourceRect = null,
        Rectangle? sliderSourceRect = null,
        ResizeMode backgroundResizeMode = ResizeMode.None,
        ResizeMode sliderBarResizeMode = ResizeMode.None,
        BorderInsets? backgroundBorderInsets = null,
        BorderInsets? sliderBarBorderInsets = null,
        Color? backgroundColor = null,
        Color? sliderBarColor = null,
        Color? sliderColor = null,
        Color? backgroundHoverColor = null,
        Color? sliderBarHoverColor = null,
        Color? sliderHoverColor = null,
        Color? disabledBackgroundColor = null,
        Color? disabledSliderBarColor = null,
        Color? disabledSliderColor = null,
        SpriteFlip backgroundFlip = SpriteFlip.None,
        SpriteFlip sliderBarFlip = SpriteFlip.None,
        SpriteFlip sliderFlip = SpriteFlip.None,
        bool backgroundPixelSnap = false,
        bool sliderBarPixelSnap = false,
        bool sliderPixelSnap = false,
        int sliderBarWidth = 16
    ) {
        this.BackgroundTexture = backgroundTexture;
        this.SliderBarTexture = sliderBarTexture;
        this.SliderTexture = sliderTexture;
        this.BackgroundSampler = backgroundSampler;
        this.SliderBarSampler = sliderBarSampler;
        this.SliderSampler = sliderSampler;
        this.BackgroundSourceRect = backgroundSourceRect ?? new Rectangle(0, 0, (int) backgroundTexture.Width, (int) backgroundTexture.Height);
        this.SliderBarSourceRect = sliderBarSourceRect ?? new Rectangle(0, 0, (int) sliderBarTexture.Width, (int) sliderBarTexture.Height);
        this.SliderSourceRect = sliderSourceRect ?? new Rectangle(0, 0, (int) sliderTexture.Width, (int) sliderTexture.Height);
        this.BackgroundResizeMode = backgroundResizeMode;
        this.SliderBarResizeMode = sliderBarResizeMode;
        this.BackgroundBorderInsets = backgroundBorderInsets ?? BorderInsets.Zero;
        this.SliderBarBorderInsets = sliderBarBorderInsets ?? BorderInsets.Zero;
        this.BackgroundColor = backgroundColor ?? Color.White;
        this.SliderBarColor = sliderBarColor ?? Color.White;
        this.SliderColor = sliderColor ?? Color.White;
        this.BackgroundHoverColor = backgroundHoverColor ?? this.BackgroundColor;
        this.SliderBarHoverColor = sliderBarHoverColor ?? this.SliderBarColor;
        this.SliderHoverColor = sliderHoverColor ?? this.SliderColor;
        this.DisabledBackgroundColor = disabledBackgroundColor ?? Color.Gray;
        this.DisabledSliderBarColor = disabledSliderBarColor ?? Color.Gray;
        this.DisabledSliderColor = disabledSliderColor ?? Color.Gray;
        this.BackgroundFlip = backgroundFlip;
        this.SliderBarFlip = sliderBarFlip;
        this.SliderFlip = sliderFlip;
        this.BackgroundPixelSnap = backgroundPixelSnap;
        this.SliderBarPixelSnap = sliderBarPixelSnap;
        this.SliderPixelSnap = sliderPixelSnap;
        this.SliderBarWidth = sliderBarWidth;
    }
}