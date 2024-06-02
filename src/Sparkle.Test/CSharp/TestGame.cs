using Newtonsoft.Json.Linq;
using OpenTK.Graphics.OpenGL;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Geometry;
using Raylib_CSharp.Images;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Materials;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Textures;
using Raylib_CSharp.Windowing;
using Sparkle.CSharp;
using Sparkle.CSharp.Content.Types;
using Sparkle.CSharp.IO.Configs.Json;
using Sparkle.CSharp.Logging;
using Sparkle.CSharp.Overlays;
using Sparkle.CSharp.Registries.Types;
using Sparkle.CSharp.Rendering.Gifs;
using Sparkle.CSharp.Rendering.Models;

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
    //public static ReadOnlySpanData<ModelAnimation> Animations;
    
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
        Gif = this.Content.Load(new GifContent("content/flame.gif", 20));
        
        // MODEL ANIMATIONS
        //Animations = this.Content.Load(new ModelAnimationContent("content/model.glb"));
        
        // MODELS
        MaterialManipulator manipulator = new MaterialManipulator()
            .Set(1, EffectRegistry.Pbr)
            .Set(1, MaterialMapIndex.Albedo, PlayerTexture)
            .Set(1, MaterialMapIndex.Metalness, PlayerTexture)
            .Set(1, MaterialMapIndex.Normal, PlayerTexture)
            .Set(1, MaterialMapIndex.Emission, PlayerTexture)
            
            .Set(1, MaterialMapIndex.Albedo, Color.White)
            .Set(1, MaterialMapIndex.Emission, new Color(255, 162, 0, 255))
            
            .Set(1, MaterialMapIndex.Metalness, 0.0F)
            .Set(1, MaterialMapIndex.Roughness, 0.0F)
            .Set(1, MaterialMapIndex.Occlusion, 1.0F)
            .Set(1, MaterialMapIndex.Emission, 0.01F)
            .Set(1, 0, 0.5F)
            .Set(1, 1, 0.5F);
        
        PlayerModel = this.Content.Load(new ModelContent("content/model.glb", manipulator));
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
        Graphics.DrawFPS(50, 50);
    }

    private bool CustomLog(LogType type, string msg, int skipFrames, ConsoleColor color) {
        /*if (type == Logger.LogType.Debug) {
            Console.WriteLine(msg);
            return true;
        }*/
        return false;
    }
}