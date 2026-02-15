using Bliss.CSharp.Fonts;
using Bliss.CSharp.Images;
using Bliss.CSharp.Textures;
using Sparkle.CSharp.Content;
using Sparkle.CSharp.Content.Types;
using Sparkle.CSharp.Registries;

namespace Sparkle.Test.CSharp;

public class ContentRegistry : Registry {
    
    // Fonts:
    public static Font Fontoe { get; private set; }
    
    // Textures:
    public static Texture2D Sprite { get; private set; }
    public static Texture2D Button { get; private set; }
    public static Texture2D TextBox { get; private set; }
    public static Texture2D ToggleBackground { get; private set; }
    public static Texture2D ToggleCheckmark { get; private set; }
    public static Texture2D SlideBar { get; private set; }
    public static Texture2D FilledSlideBar { get; private set; }
    public static Texture2D Slider { get; private set; }
    public static Texture2D UiBannerTexture { get; private set; }
    public static Texture2D UiBannerEdgeLessTexture { get; private set; }
    public static Texture2D UiSliderTexture { get; private set; }
    public static Texture2D UiArrowTexture { get; private set; }
    
    // Gifs:
    public static AnimatedImage AnimatedImage;
    public static Texture2D Gif;
    
    /// <summary>
    /// Loads the content for the registry, including fonts, textures, models, and other assets.
    /// This method initializes the necessary assets required by the registry, utilizing the provided
    /// <see cref="ContentManager"/> to load resources.
    /// </summary>
    /// <param name="content">The <see cref="ContentManager"/> instance used to load resources.</param>
    protected override void Load(ContentManager content) {
        base.Load(content);
        
        // Fonts:
        Fontoe = content.Load(new FontContent("content/fontoe.ttf"));
        
        // Textures:
        Sprite = content.Load(new TextureContent("content/sprite.png"));
        Button = content.Load(new TextureContent("content/button.png"));
        TextBox = content.Load(new TextureContent("content/text-box.png"));
        ToggleBackground = content.Load(new TextureContent("content/toggle_background.png"));
        ToggleCheckmark = content.Load(new TextureContent("content/toggle_checkmark.png"));
        SlideBar = content.Load(new TextureContent("content/bar.png"));
        FilledSlideBar = content.Load(new TextureContent("content/filled_bar.png"));
        Slider = content.Load(new TextureContent("content/slider.png"));
        UiBannerTexture = content.Load(new TextureContent("content/ui_banner.png"));
        UiBannerEdgeLessTexture = content.Load(new TextureContent("content/ui_banner_edgeless.png"));
        UiSliderTexture = content.Load(new TextureContent("content/slider_high_res.png"));
        UiArrowTexture = content.Load(new TextureContent("content/ui_arrow.png"));
        
        // Gifs:
        AnimatedImage = new AnimatedImage("content/test.gif");
        Gif = new Texture2D(content.GraphicsDevice, AnimatedImage.SpriteSheet);
    }
}