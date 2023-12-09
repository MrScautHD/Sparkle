using Raylib_cs;
using Sparkle.csharp;
using Sparkle.csharp.content.type;
using Sparkle.csharp.file.config;
using Sparkle.csharp.overlay;
using Sparkle.csharp.window;

namespace Test; 

public class TestGame : Game {

    // TEXTURES
    public static Texture2D PlayerTexture;
    public static Texture2D SpriteTexture;
    
    // MODELS
    public static Model PlayerModel;
    
    // MODEL ANIMATIONS
    public static ModelAnimation[] Animations;
    
    // OVERLAY
    public TestOverlay Overlay;
    
    public TestGame(GameSettings settings) : base(settings) { }

    protected override void Init() {
        base.Init();
        
        this.Overlay = new TestOverlay("Test");
        this.Overlay.Enabled = false;
        OverlayManager.AddOverlay(this.Overlay);
        
        Config config = new ConfigBuilder("config", "test")
            .Add("test", true)
            .Add("lol", 1000)
            .Add("hello", "Hello World!")
            .Build();
        
        Console.WriteLine(config.GetValue<string>("hello"));
        Console.WriteLine(config.GetValue<int>("lol"));
    }

    protected override void Load() {
        base.Load();
        
        // TEXTURES
        PlayerTexture = this.Content.Load(new TextureContent("texture.png"));
        SpriteTexture = this.Content.Load(new TextureContent("sprite.png"));
        
        // MODELS
        PlayerModel = this.Content.Load(new ModelContent("model.glb"));
        
        // MODEL ANIMATIONS
        Animations = this.Content.Load(new ModelAnimationContent("model.glb"));
    }

    protected override void Draw() {
        base.Draw();
        
        //this.VideoPlayer.Draw();
    }

    protected override void Update() {
        base.Update();
        
        if (Input.IsKeyPressed(KeyboardKey.KEY_F11)) {
            Window.Maximize();
            Window.ToggleFullscreen();
        }
    }
}