using Raylib_cs;
using Sparkle.csharp;
using Sparkle.csharp.file.config;
using Sparkle.csharp.window;

namespace Test; 

public class TestGame : Game {

    public TestOverlay Overlay;

    public TestGame(GameSettings settings) : base(settings) { }

    protected override void Init() {
        base.Init();

        this.Overlay = new TestOverlay("Test");
        this.Overlay.Enabled = true;
        
        Config config = new ConfigBuilder("config", "test")
            .Add("test", true)
            .Add("lol", 1000)
            .Add("hello", "Hello World!")
            .Build();
        
        Console.WriteLine(config.GetValue<string>("hello"));
        Console.WriteLine(config.GetValue<int>("lol"));
    }

    protected override void Update() {
        base.Update();
        
        if (Input.IsKeyPressed(KeyboardKey.KEY_F11)) {
            Window.Maximize();
            Window.ToggleFullscreen();
        }
    }
}