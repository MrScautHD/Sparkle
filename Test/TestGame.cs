using Silk.NET.Input;
using Sparkle.csharp;
using Sparkle.csharp.file.config;

namespace Test; 

public class TestGame : Application {
    
    public TestGame(ApplicationSettings settings) : base(settings) {
        Logger.CreateLogFile("logs", "log");
    }

    protected override void Init() {
        base.Init();

        Config builder = new ConfigBuilder("config", "test")
            .Add("test", true)
            .Add("lol", 1000)
            .Build();
    }

    protected override void Update(double dt) {
        base.Update(dt);

        if (Input.IsKeyDown(Key.A)) {
            Logger.Error("A");
        }

        if (Input.IsKeyPressed(Key.T)) {
            Logger.Error("T");
        }
        
        if (Input.IsKeyReleased(Key.D)) {
            Logger.Error("D");
        }
    }
}