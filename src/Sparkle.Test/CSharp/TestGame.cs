using Newtonsoft.Json.Linq;
using Raylib_cs;
using Sparkle.CSharp;
using Sparkle.CSharp.Content.Types;
using Sparkle.CSharp.IO.Configs.Json;
using Sparkle.CSharp.Overlays;
using Sparkle.CSharp.Rendering.Gifs;
using Sparkle.CSharp.Rendering.Helpers;
using Sparkle.CSharp.Windowing;

namespace Sparkle.Test.CSharp;

public class TestGame : Game {

    // TEXTURES
    public static Texture2D PlayerTexture;
    public static Texture2D SpriteTexture;
    
    // IMAGES
    public static Image Skybox;
    
    // GIF
    public static Gif Gif;
    
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
        
        this.Overlay = new TestOverlay("Sparkle.Test");
        this.Overlay.Enabled = false;
        OverlayManager.Add(this.Overlay);
        
        JArray jList = new JArray();
        jList.Add(21);
        jList.Add(22);
        jList.Add(23);
        
        JsonConfig jsonConfig = new JsonConfigBuilder("config", "jsonConfig")
            .Add("Number", 12345.02F)
            .Add("Check", true)
            .Add("Text", "Hello")
            .Add("List", jList)
            .Build();
        
        Logger.Debug($"Number: {jsonConfig.GetValue<float>("Number")}");
        Logger.Debug($"Check: {jsonConfig.GetValue<bool>("Check")}");
        Logger.Debug($"Text: {jsonConfig.GetValue<string>("Text")}");
        Logger.Debug($"List: {jsonConfig.GetValue<JArray>("List")}");
    }

    protected override void Load() {
        base.Load();
        
        // TEXTURES
        PlayerTexture = this.Content.Load(new TextureContent("content/texture.png"));
        SpriteTexture = this.Content.Load(new TextureContent("content/sprite.png"));
        
        // IMAGES
        Skybox = this.Content.Load(new ImageContent("content/skybox.png"));
        
        // GIF
        Gif = this.Content.Load(new GifContent("content/test.gif", 3));
        
        // MODELS
        PlayerModel = this.Content.Load(new ModelContent("content/model.glb"));

        // MODEL ANIMATIONS
        //Animations = this.Content.Load(new ModelAnimationContent("content/model.glb"));
    }

    protected override void Update() {
        base.Update();
        
        if (Input.IsKeyPressed(KeyboardKey.F11)) {
            Input.DisableCursor();
            Window.ToggleBorderless();
        }
    }

    protected override void Draw() {
        base.Draw();
        FontHelper.DrawFps(50, 50);
    }

    private bool CustomLog(Logger.LogType type, string msg, int skipFrames, ConsoleColor color) {
        /*if (type == Logger.LogType.Debug) {
            Console.WriteLine(msg);
            return true;
        }*/
        return false;
    }
}