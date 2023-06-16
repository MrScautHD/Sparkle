using System.Numerics;
using Raylib_cs;
using Sparkle.csharp;
using Sparkle.csharp.file.config;
using Sparkle.csharp.scene;

namespace Test; 

public class TestGame : Game {

    public Texture2D Icon;
    public Texture2D Icon2;
    
    public TestGame(GameSettings settings, Scene scene) : base(settings, scene) {
        Logger.CreateLogFile("logs", "log");
    }

    protected override void Init() {
        base.Init();

        this.Icon = this.Content.Load<Texture2D>("icon.png");
        this.Icon2 = this.Content.Load<Texture2D>("icon.png");

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
        
        Raylib.DrawTextureEx(this.Icon, new Vector2(this.Window.GetScreenSize().Width / 2 - this.Icon.width / 2 * 5, this.Window.GetScreenSize().Height / 2 - this.Icon.height / 2 * 5), 0, 5, Color.WHITE);

        Raylib.EndDrawing();
    }

    public override void Dispose() {
        base.Dispose();
    }
}