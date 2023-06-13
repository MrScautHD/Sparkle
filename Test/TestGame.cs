using Raylib_cs;
using Sparkle.csharp;
using Sparkle.csharp.file.config;

namespace Test; 

public class TestGame : Application {

    public Texture2D Icon;
    
    public TestGame(ApplicationSettings settings) : base(settings) {
        Logger.CreateLogFile("logs", "log");
    }

    protected override void Init() {
        base.Init();

        this.Icon = this.Content.Load<Texture2D>("icon.png");

        Config builder = new ConfigBuilder("config", "test")
            .Add("test", true)
            .Add("lol", 1000)
            .Build();
    }

    protected override void Update() {
        base.Update();
    }

    protected override void Draw() {
        base.Draw();
        
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.SKYBLUE);

        Raylib.DrawText("Hello, world!", 12, 12, 20, Color.BLACK);
        
        Raylib.DrawTexture(this.Icon, 30, 30, Color.WHITE);

        Raylib.EndDrawing();
    }
}