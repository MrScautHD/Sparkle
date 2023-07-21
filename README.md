<p align="center" style="margin-bottom: 0px !important;">
  <img width="512" src="https://cdn.discordapp.com/attachments/1036960672715644939/1131937257578836088/imaged.png" alt="Logo" align="center">
</p>

<h1 align="center" style="margin-top: 0px;">Welcome to Sparkle 🎉</h1>
<h4 align="center">A fast, Cross-platform .NET 7 C# 11 game engine.</h4>

![grafik](https://user-images.githubusercontent.com/65916181/220327780-328a50de-def5-485a-b769-1f98b5c292ad.png)

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/Q5Q6K0XC0)

[<img src="https://user-images.githubusercontent.com/65916181/229357115-d601e227-e80a-459d-974e-92905e192b08.png" width="125" height="40">](https://discord.gg/7XKw6YQa76)

🪙 Installation - [Nuget](https://www.nuget.org/packages/Sparkle)
========================
```
dotnet add package Sparkle --version [VERSION]
```

📚 Libraries (https://www.nuget.org/packages)
==============================================
- [RayLib](https://www.raylib.com/)
- [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json)
- [BepuPhysics](https://www.nuget.org/packages/BepuPhysics)

🌋 Graphic Engine
==================
- [OpenGl](https://www.opengl.org/)

💡 Features
==================
`Audio`
`Texture`
`Model`
`Font`
`3D Camera`
`Material`
`Overlay`
`GUI`
`Scene`
`Entity`
`Directed/Pointed Light`
`Content Manager`
`Config`
`Physic`

## 🖥️ Example
```csharp
public class GameTest : Game {
    
    public GameTest(GameSettings settings, Scene scene) : base(settings, scene) {
        
        // Create your own config file!
        Config config = new ConfigBuilder("config", "test")
            .Add("Hello", "Hello World!")
            .Add("Bye", 1000)
            .Build();

        Logger.Info(config.GetValue<string>("Hello"));
    }

    protected override void Init() {
        base.Init();
        
        // Simple logger.
        Logger.Debug("Debug text!");
        Logger.Info("Info text!");
        Logger.Warn("Warn text!");
        Logger.Error("Error text!");

        // Simple time.
        double deltaTime = Time.DeltaTime;
        double totalTime = Time.TotalTime;
        
        // Stop the time!
        Time.WaitTime(10);

        // Load resources.
        Texture2D texture = this.Content.Load<Texture2D>("icon.png");
        
        // Create your own Scene.
        SceneManager.SetScene(new TestScene("earth"));
        
        // Open a url.
        this.OpenURL("https://www.youtube.com/");
        
        // Take a screenshot
        this.Window.TakeScreenshot("path");
    }

    // Just runs when !Game.Headless [Support for servers!]
    protected override void Draw() {
        base.Draw();
        
        // Draw circle if "A" down and way more simple options to draw!
        if (Input.IsKeyDown(KeyboardKey.KEY_A)) {
            this.Graphics.ShapeRenderer.DrawCircle(new Vector2(50, 50), 20, Color.BLUE);
        }
    }
}
```

💸 Sponsors
============
Please consider [SPONSOR](https://github.com/sponsors/MrScautHD) me to further help development and to allow faster issue triaging and new features to be implemented.
