using Raylib_cs;
using Sparkle.csharp;
using Sparkle.csharp.file.config;
using Sparkle.csharp.scene;

namespace Test; 

public class TestGame : Game {

    public TestGame(GameSettings settings) : base(settings) {
    }

    protected override void Init() {
        base.Init();
        
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
            this.Window.Maximize();
            this.Window.ToggleFullscreen();
        }
    }
}