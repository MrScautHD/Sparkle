using Bliss.CSharp.Colors;
using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Sprites;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
using Veldrith;

namespace Sparkle.CSharp.GUI.Elements.Data;

public class TextureScrollViewData {
    
    /// <summary>
    /// The texture used to render the scroll view menu.
    /// </summary>
    public Texture2D MenuTexture;
    
    /// <summary>
    /// The texture used to render the slider bar track.
    /// </summary>
    public Texture2D SliderBarTexture;
    
    /// <summary>
    /// The texture used to render the slider handle.
    /// </summary>
    public Texture2D SliderTexture;
    
    /// <summary>
    /// The sampler used for sampling the menu texture.
    /// </summary>
    public Sampler? MenuSampler;
    
    /// <summary>
    /// The sampler used for sampling the slider bar texture.
    /// </summary>
    public Sampler? SliderBarSampler;
    
    /// <summary>
    /// The sampler used for sampling the slider texture.
    /// </summary>
    public Sampler? SliderSampler;
    
    /// <summary>
    /// The source rectangle defining the region of the menu texture to draw.
    /// </summary>
    public Rectangle MenuSourceRect;
    
    /// <summary>
    /// The source rectangle defining the region of the slider bar texture to draw.
    /// </summary>
    public Rectangle SliderBarSourceRect;
    
    /// <summary>
    /// The source rectangle defining the region of the slider texture to draw.
    /// </summary>
    public Rectangle SliderSourceRect;
    
    /// <summary>
    /// The resize mode applied to the menu texture.
    /// </summary>
    public ResizeMode MenuResizeMode;
    
    /// <summary>
    /// The resize mode applied to the slider bar texture.
    /// </summary>
    public ResizeMode SliderBarResizeMode;
    
    /// <summary>
    /// The border insets used for nine-slice resizing of the menu texture.
    /// </summary>
    public BorderInsets MenuBorderInsets;
    
    /// <summary>
    /// The border insets used for nine-slice resizing of the slider bar texture.
    /// </summary>
    public BorderInsets SliderBarBorderInsets;
    
    /// <summary>
    /// The base color applied to the menu.
    /// </summary>
    public Color MenuColor;
    
    /// <summary>
    /// The base color applied to the slider bar.
    /// </summary>
    public Color SliderBarColor;
    
    /// <summary>
    /// The base color applied to the slider handle.
    /// </summary>
    public Color SliderColor;
    
    /// <summary>
    /// The color applied to the menu when hovered.
    /// </summary>
    public Color MenuHoverColor;
    
    /// <summary>
    /// The color applied to the slider bar when hovered.
    /// </summary>
    public Color SliderBarHoverColor;
    
    /// <summary>
    /// The color applied to the slider handle when hovered.
    /// </summary>
    public Color SliderHoverColor;
    
    /// <summary>
    /// The color applied to the menu when disabled.
    /// </summary>
    public Color DisabledMenuColor;
    
    /// <summary>
    /// The color applied to the slider bar when disabled.
    /// </summary>
    public Color DisabledSliderBarColor;
    
    /// <summary>
    /// The color applied to the slider handle when disabled.
    /// </summary>
    public Color DisabledSliderColor;
    
    /// <summary>
    /// The flip mode applied to the menu texture.
    /// </summary>
    public SpriteFlip MenuFlip;
    
    /// <summary>
    /// The flip mode applied to the slider bar texture.
    /// </summary>
    public SpriteFlip SliderBarFlip;
    
    /// <summary>
    /// The flip mode applied to the slider texture.
    /// </summary>
    public SpriteFlip SliderFlip;
    
    /// <summary>
    /// When <c>true</c>, snaps the menu texture position and origin to whole pixels.
    /// </summary>
    public bool MenuPixelSnap;
    
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
    /// <param name="menuTexture">The texture used to render the scroll view menu.</param>
    /// <param name="sliderBarTexture">The texture used to render the slider bar track.</param>
    /// <param name="sliderTexture">The texture used to render the slider handle.</param>
    /// <param name="menuSampler">The sampler used for sampling the menu texture. Defaults to <c>null</c>.</param>
    /// <param name="sliderBarSampler">The sampler used for sampling the slider bar texture. Defaults to <c>null</c>.</param>
    /// <param name="sliderSampler">The sampler used for sampling the slider texture. Defaults to <c>null</c>.</param>
    /// <param name="menuSourceRect">The source rectangle defining the region of the menu texture to draw. Defaults to the full texture bounds.</param>
    /// <param name="sliderBarSourceRect">The source rectangle defining the region of the slider bar texture to draw. Defaults to the full texture bounds.</param>
    /// <param name="sliderSourceRect">The source rectangle defining the region of the slider texture to draw. Defaults to the full texture bounds.</param>
    /// <param name="menuResizeMode">The resize mode applied to the menu texture. Defaults to <see cref="ResizeMode.None"/>.</param>
    /// <param name="sliderBarResizeMode">The resize mode applied to the slider bar texture. Defaults to <see cref="ResizeMode.None"/>.</param>
    /// <param name="menuBorderInsets">The border insets used for nine-slice resizing of the menu texture. Defaults to <see cref="BorderInsets.Zero"/>.</param>
    /// <param name="sliderBarBorderInsets">The border insets used for nine-slice resizing of the slider bar texture. Defaults to <see cref="BorderInsets.Zero"/>.</param>
    /// <param name="menuColor">The base color applied to the menu. Defaults to <see cref="Color.White"/>.</param>
    /// <param name="sliderBarColor">The base color applied to the slider bar. Defaults to <see cref="Color.White"/>.</param>
    /// <param name="sliderColor">The base color applied to the slider handle. Defaults to <see cref="Color.White"/>.</param>
    /// <param name="menuHoverColor">The color applied to the menu when hovered. Defaults to <paramref name="menuColor"/>.</param>
    /// <param name="sliderBarHoverColor">The color applied to the slider bar when hovered. Defaults to <paramref name="sliderBarColor"/>.</param>
    /// <param name="sliderHoverColor">The color applied to the slider handle when hovered. Defaults to <paramref name="sliderColor"/>.</param>
    /// <param name="disabledMenuColor">The color applied to the menu when disabled. Defaults to <see cref="Color.Gray"/>.</param>
    /// <param name="disabledSliderBarColor">The color applied to the slider bar when disabled. Defaults to <see cref="Color.Gray"/>.</param>
    /// <param name="disabledSliderColor">The color applied to the slider handle when disabled. Defaults to <see cref="Color.Gray"/>.</param>
    /// <param name="menuFlip">The flip mode applied to the menu texture. Defaults to <see cref="SpriteFlip.None"/>.</param>
    /// <param name="sliderBarFlip">The flip mode applied to the slider bar texture. Defaults to <see cref="SpriteFlip.None"/>.</param>
    /// <param name="sliderFlip">The flip mode applied to the slider texture. Defaults to <see cref="SpriteFlip.None"/>.</param>
    /// <param name="menuPixelSnap">When <c>true</c>, snaps the menu texture position and origin to whole pixels. Defaults to <c>false</c>.</param>
    /// <param name="sliderBarPixelSnap">When <c>true</c>, snaps the slider bar texture position and origin to whole pixels. Defaults to <c>false</c>.</param>
    /// <param name="sliderPixelSnap">When <c>true</c>, snaps the slider texture position and origin to whole pixels. Defaults to <c>false</c>.</param>
    /// <param name="sliderBarWidth">The width of the slider bar in pixels. Defaults to <c>16</c>.</param>
    public TextureScrollViewData(
        Texture2D menuTexture,
        Texture2D sliderBarTexture,
        Texture2D sliderTexture,
        Sampler? menuSampler = null,
        Sampler? sliderBarSampler = null,
        Sampler? sliderSampler = null,
        Rectangle? menuSourceRect = null,
        Rectangle? sliderBarSourceRect = null,
        Rectangle? sliderSourceRect = null,
        ResizeMode menuResizeMode = ResizeMode.None,
        ResizeMode sliderBarResizeMode = ResizeMode.None,
        BorderInsets? menuBorderInsets = null,
        BorderInsets? sliderBarBorderInsets = null,
        Color? menuColor = null,
        Color? sliderBarColor = null,
        Color? sliderColor = null,
        Color? menuHoverColor = null,
        Color? sliderBarHoverColor = null,
        Color? sliderHoverColor = null,
        Color? disabledMenuColor = null,
        Color? disabledSliderBarColor = null,
        Color? disabledSliderColor = null,
        SpriteFlip menuFlip = SpriteFlip.None,
        SpriteFlip sliderBarFlip = SpriteFlip.None,
        SpriteFlip sliderFlip = SpriteFlip.None,
        bool menuPixelSnap = false,
        bool sliderBarPixelSnap = false,
        bool sliderPixelSnap = false,
        int sliderBarWidth = 16
    ) {
        this.MenuTexture = menuTexture;
        this.SliderBarTexture = sliderBarTexture;
        this.SliderTexture = sliderTexture;
        this.MenuSampler = menuSampler;
        this.SliderBarSampler = sliderBarSampler;
        this.SliderSampler = sliderSampler;
        this.MenuSourceRect = menuSourceRect ?? new Rectangle(0, 0, (int) menuTexture.Width, (int) menuTexture.Height);
        this.SliderBarSourceRect = sliderBarSourceRect ?? new Rectangle(0, 0, (int) sliderBarTexture.Width, (int) sliderBarTexture.Height);
        this.SliderSourceRect = sliderSourceRect ?? new Rectangle(0, 0, (int) sliderTexture.Width, (int) sliderTexture.Height);
        this.MenuResizeMode = menuResizeMode;
        this.SliderBarResizeMode = sliderBarResizeMode;
        this.MenuBorderInsets = menuBorderInsets ?? BorderInsets.Zero;
        this.SliderBarBorderInsets = sliderBarBorderInsets ?? BorderInsets.Zero;
        this.MenuColor = menuColor ?? Color.White;
        this.SliderBarColor = sliderBarColor ?? Color.White;
        this.SliderColor = sliderColor ?? Color.White;
        this.MenuHoverColor = menuHoverColor ?? this.MenuColor;
        this.SliderBarHoverColor = sliderBarHoverColor ?? this.SliderBarColor;
        this.SliderHoverColor = sliderHoverColor ?? this.SliderColor;
        this.DisabledMenuColor = disabledMenuColor ?? Color.Gray;
        this.DisabledSliderBarColor = disabledSliderBarColor ?? Color.Gray;
        this.DisabledSliderColor = disabledSliderColor ?? Color.Gray;
        this.MenuFlip = menuFlip;
        this.SliderBarFlip = sliderBarFlip;
        this.SliderFlip = sliderFlip;
        this.MenuPixelSnap = menuPixelSnap;
        this.SliderBarPixelSnap = sliderBarPixelSnap;
        this.SliderPixelSnap = sliderPixelSnap;
        this.SliderBarWidth = sliderBarWidth;
    }
}