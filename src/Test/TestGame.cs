using Raylib_cs;
using Sparkle.CSharp;
using Sparkle.CSharp.Content.Types;
using Sparkle.CSharp.IO.Config;
using Sparkle.CSharp.Overlays;
using Sparkle.CSharp.Windowing;

namespace Test; 

public class TestGame : Game {

    // TEXTURES
    public static Texture2D PlayerTexture;
    public static Texture2D SpriteTexture;
    
    // MODELS
    public static Model PlayerModel;
    
    // MODEL ANIMATIONS
    //public static ModelAnimation[] Animations;
    
    // OVERLAY
    public TestOverlay Overlay;
    
    public TestGame(GameSettings settings) : base(settings) {
        Logger.Message += this.CustomLog;
    }
    
    protected override void Init() {
        base.Init();
        
        this.Overlay = new TestOverlay("Test");
        this.Overlay.Enabled = false;
        OverlayManager.Add(this.Overlay);
        
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
        PlayerTexture = this.Content.Load(new TextureContent("content/texture.png"));
        SpriteTexture = this.Content.Load(new TextureContent("content/sprite.png"));
        
        // MODELS
        PlayerModel = this.Content.Load(new ModelContent("content/model.glb"));

        // MODEL ANIMATIONS
        //Animations = this.Content.Load(new ModelAnimationContent("model.glb"));
    }

    protected override void Update() {
        base.Update();
        
        if (Input.IsKeyPressed(KeyboardKey.F11)) {
            Input.DisableCursor();
            Window.ToggleBorderless();
        }
    }

    private bool CustomLog(Logger.LogType type, string msg, int skipFrames, ConsoleColor color) {
        /*if (type == Logger.LogType.Debug) {
            Console.WriteLine(msg);
            return true;
        }*/
        
        return false;
    }
}