using Silk.NET.Input;
using Silk.NET.Windowing;
using Sparkle.csharp;
using Sparkle.csharp.file.config;

namespace Test; 

public class TestGame : Application {
    
    public TestGame(ApplicationSettings settings) : base(settings) {
        Logger.CreateLogFile("logs", "log");
    }

    protected override void Init() {
        base.Init();
        
        Input.KeyIsDown += this.OnKeyDown;
        
        Config builder = new ConfigBuilder("config", "test")
            .Add("test", true)
            .Add("lol", 1000)
            .Build();
    }

    protected override void Update(double dt) {
        base.Update(dt);
    }

    protected virtual void OnKeyDown(IKeyboard keyboard, Key key, int keyCode) {
        if (key == Key.F11) {
            if (this.Win.WindowState != WindowState.Fullscreen) {
                this.Win.WindowState = WindowState.Fullscreen;
            }
            else {
                this.Win.WindowState = WindowState.Normal;
            }
        }
    }
}