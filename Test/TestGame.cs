using Raylib_cs;
using Sparkle.csharp;
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
        PlayerTexture = Content.Load<Texture2D>("texture.png");
        SpriteTexture = Content.Load<Texture2D>("sprite.png");
        
        // MODELS
        PlayerModel = Content.Load<Model>("model.glb");
    }

    protected override void Update() {
        base.Update();
        
        if (Input.IsKeyPressed(KeyboardKey.KEY_F11)) {
            Window.Maximize();
            Window.ToggleFullscreen();
        }
    }
}