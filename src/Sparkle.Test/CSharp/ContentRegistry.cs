using Bliss.CSharp.Effects;
using Bliss.CSharp.Fonts;
using Bliss.CSharp.Geometry;
using Bliss.CSharp.Graphics.Pipelines.Textures;
using Bliss.CSharp.Graphics.VertexTypes;
using Bliss.CSharp.Textures;
using Sparkle.CSharp;
using Sparkle.CSharp.Content;
using Sparkle.CSharp.Content.Types;
using Sparkle.CSharp.Graphics.Rendering;
using Sparkle.CSharp.Registries;

namespace Sparkle.Test.CSharp;

public class ContentRegistry : Registry {
    
    public static Font Fontoe { get; private set; }
    public static Model PlayerModel { get; private set; }
    public static Texture2D Sprite { get; private set; }
    public static Texture2D Button { get; private set; }
    
    public static SkyBox SkyBox { get; private set; }
    
    public static Effect GrayScaleEffect { get; private set; }

    private static SimpleTextureLayout _textureLayout;
    
    protected override void Load(ContentManager content) {
        base.Load(content);
        
        Fontoe = content.Load(new FontContent("content/fontoe.ttf"));
        PlayerModel = content.Load(new ModelContent("content/model.glb"));
        Sprite = content.Load(new TextureContent("content/sprite.png"));
        Button = content.Load(new TextureContent("content/button.png"));
        
        SkyBox = new SkyBox(content.GraphicsDevice, content.Load(new CubemapContent("content/skybox.png")));
        
        _textureLayout = new SimpleTextureLayout(Game.Instance?.GraphicsDevice!, "fTexture");
        GrayScaleEffect = content.Load(new EffectContent(SpriteVertex2D.VertexLayout, "content/shaders/full_screen_render_pass.vert", "content/shaders/filters/gray_scale.frag"));
        GrayScaleEffect.AddTextureLayout(_textureLayout);
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (disposing) {
            _textureLayout.Dispose();
        }
    }
}