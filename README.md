<p align="center" style="margin-bottom: 0px !important;">
  <img width="512" src="https://cdn.discordapp.com/attachments/1036960672715644939/1131937257578836088/imaged.png" alt="Logo" align="center">
</p>

<h1 align="center" style="margin-top: 0px;">Welcome to Sparkle 🎉</h1>
<h4 align="center">A fast, Cross-platform .NET 8 C# 12 game engine.</h4>

![grafik](https://user-images.githubusercontent.com/65916181/220327780-328a50de-def5-485a-b769-1f98b5c292ad.png)

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/Q5Q6K0XC0)

[<img src="https://user-images.githubusercontent.com/65916181/229357115-d601e227-e80a-459d-974e-92905e192b08.png" width="125" height="40">](https://discord.gg/7XKw6YQa76)

🪙 Installation - [Nuget](https://www.nuget.org/packages/Sparkle)
========================
<!-- Make sure to update this as new versions come out. Doing this makes it easier for people to copy paste, us devs are lazy -->
```
dotnet add package Sparkle --version 2.2.0
```

⭐ Getting Started
===========
We trust you'll relish your time with Sparkle! To kick things off, head over to our [Wiki](https://github.com/MrScautHD/Sparkle/wiki/Getting-Started) for a seamless start.

📚 Libraries - [Nuget](https://www.nuget.org/packages)
==============================================
- [RayLib](https://www.raylib.com)
- [OpenTK](https://github.com/opentk/opentk)
- [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json)
- [JoltPhysicsSharp](https://www.nuget.org/packages/JoltPhysicsSharp)

🌋 Graphic Engine
==================
- [OpenGl](https://www.opengl.org/)
- [Angle](https://github.com/google/angle) `(Vulkan, Direct3D, Metal...)`

💻 Contributors
==================
<a href="https://github.com/mrscauthd/sparkle/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=mrscauthd/sparkle&max=500&columns=20&anon=1" />
</a>


## 🖥️ Basic Example
```csharp
public class GameTest : Game {

    public Texture2D Texture;
    
    public GameTest(GameSettings settings) : base(settings) {
        
        // Create your own config file!
        Config config = new ConfigBuilder("config", "test")
            .Add("Hello", "Hello World!")
            .Add("Bye", 1000)
            .Build();

        Logger.Info(config.GetValue<string>("Hello"));
    }

    protected override void Init() {
        base.Init();
        
        // Open a url.
        this.OpenUrl("https://www.youtube.com/");
    }

    protected override void Load() {
        base.Load();
        
        // Load resources.
        this.Texture = this.Content.Load(new TextureContent("icon.png"));
    }

    protected override void Draw() {
        base.Draw();
        
        // Draw circle if "A" down.
        if (Input.IsKeyDown(KeyboardKey.A)) {
            ShapeHelper.DrawCircle(new Vector2(50, 50), 20, Color.Blue);
        }

        // Draw texture if "B" down.
        if (Input.IsKeyDown(KeyboardKey.B)) {
            TextureHelper.Draw(this.Texture, Vector3.Zero, Color.White);
        }
    }
}
```

💸 Sponsors
============
Please consider [SPONSOR](https://github.com/sponsors/MrScautHD) me to further help development and to allow faster issue triaging and new features to be implemented.
