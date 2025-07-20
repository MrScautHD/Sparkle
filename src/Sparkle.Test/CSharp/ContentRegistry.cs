using Bliss.CSharp.Effects;
using Bliss.CSharp.Fonts;
using Bliss.CSharp.Geometry;
using Bliss.CSharp.Textures;
using Sparkle.CSharp.Content;
using Sparkle.CSharp.Content.Types;
using Sparkle.CSharp.Effects.Filters;
using Sparkle.CSharp.Graphics.Rendering;
using Sparkle.CSharp.Registries;

namespace Sparkle.Test.CSharp;

public class ContentRegistry : Registry {
    
    public static Font Fontoe { get; private set; }
    public static Model PlayerModel { get; private set; }
    public static Texture2D Sprite { get; private set; }
    public static Texture2D Button { get; private set; }
    public static Texture2D TextBox { get; private set; }
    public static Texture2D ToggleBackground { get; private set; }
    public static Texture2D ToggleCheckmark { get; private set; }
    
    public static SkyBox SkyBox { get; private set; }
    
    protected override void Load(ContentManager content) {
        base.Load(content);
        
        Fontoe = content.Load(new FontContent("content/fontoe.ttf"));
        PlayerModel = content.Load(new ModelContent("content/model.glb"));
        Sprite = content.Load(new TextureContent("content/sprite.png"));
        Button = content.Load(new TextureContent("content/button.png"));
        TextBox = content.Load(new TextureContent("content/text-box.png"));
        ToggleBackground = content.Load(new TextureContent("content/toggle_background.png"));
        ToggleCheckmark = content.Load(new TextureContent("content/toggle_checkmark.png"));
        
        SkyBox = new SkyBox(content.GraphicsDevice, content.Load(new CubemapContent("content/skybox.png")));
    }
    
    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (disposing) {
            SkyBox.Dispose();
        }
    }
}