using Raylib_cs;
using Sparkle.csharp;
using Sparkle.csharp.file.config;
using Sparkle.csharp.scene;

namespace Test; 

public class TestGame : Game {

    public Texture2D Icon;
    public Texture2D Icon2;

    public Texture2D Screenshot;
    
    public TestGame(GameSettings settings, Scene scene) : base(settings, scene) {
        Logger.CreateLogFile("logs", "log");
    }

    protected override void Init() {
        base.Init();
        
        //this.OpenURL("https://www.youtube.com/");
        
        this.Icon = this.Content.Load<Texture2D>("icon.png");
        this.Icon2 = this.Content.Load<Texture2D>("icon.png");

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