using Newtonsoft.Json.Linq;
using Sparkle.CSharp;
using Sparkle.CSharp.IO.Configs.Json;
using Sparkle.CSharp.Logging;
using Sparkle.CSharp.Overlays;
using Sparkle.CSharp.Registries;
using Sparkle.Test.CSharp.Dim3;

namespace Sparkle.Test.CSharp;

public class TestGame : Game {
    
    // OVERLAY
    public TestOverlay Overlay;
    
    private float _frameTimer;
    private int _frame;
    
    public TestGame(GameSettings settings) : base(settings) {
        Logger.Message += this.CustomLog;
    }

    protected override void OnRun() {
        base.OnRun();
        RegistryManager.Add(new ContentRegistry());
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
        
        JsonConfig jsonConfig = new JsonConfigBuilder("config", "jsonConfig", "0856wjfgao8314asfrjtj2948tqapkgf")
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

    protected override void Update() {
        base.Update();
        
        // Gif
        this._frameTimer += Time.GetFrameTime();
        float step = 1.0F / 10.0F;
        
        if (this._frameTimer >= step) {
            this._frame++;
            
            if (this._frame >= ContentRegistry.Gif.Frames) {
                this._frame = 0;
            }

            this._frameTimer -= step;
        }
        
        // Borderless
        if (Input.IsKeyPressed(KeyboardKey.F11)) {
            Input.DisableCursor();
            Window.ToggleBorderless();
        }
    }

    protected override void Draw() {
        base.Draw();
        
        // FPS
        Graphics.DrawFPS(50, 50);
        
        // GIF
        Graphics.DrawTexture(ContentRegistry.Gif.GetFrame(this._frame), 100, 100, Color.White);
    }

    private bool CustomLog(LogType type, string msg, int skipFrames, ConsoleColor color) {
        /*if (type == Logger.LogType.Debug) {
            Console.WriteLine(msg);
            return true;
        }*/
        return false;
    }
}