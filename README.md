<p align="center" style="margin-bottom: 0px !important;">
  <img width="512" src="https://github.com/MrScautHD/Sparkle/assets/65916181/9f378f17-5468-4dd4-bc72-ffd396a90639" alt="Logo" align="center">
</p>

<h1 align="center" style="margin-top: 0px;">Welcome to Sparkle 🎉</h1>
<h4 align="center">A fast, Cross-platform .NET 8 C# 12 game engine.</h4>

![grafik](https://user-images.githubusercontent.com/65916181/220327780-328a50de-def5-485a-b769-1f98b5c292ad.png)

[<img src="https://github.com/MrScautHD/Sparkle/assets/65916181/87b291cd-6506-4fb5-b032-abf3170a28c4" alt="discord" width="186" height="60">](https://discord.gg/7XKw6YQa76)
[<img src="https://github.com/MrScautHD/Sparkle/assets/65916181/de09f016-db11-4554-aa56-4d1bd6c2464f" alt="sponsor" width="186" height="60">](https://github.com/sponsors/MrScautHD)

🪙 Installation - [Nuget](https://www.nuget.org/packages/Sparkle)
==================================================================
```
dotnet add package Sparkle --version 4.0.1
```

⭐ Getting Started
===================
We trust you'll relish your time with Sparkle! To kick things off, head over to our [Wiki](https://github.com/MrScautHD/Sparkle/wiki/Getting-Started) for a seamless start.

📚 Libraries - [Nuget](https://www.nuget.org/packages)
======================================================
- [`RayLib-CSharp`](https://github.com/MrScautHD/Raylib-CSharp)
- [`OpenTK.Graphics`](https://github.com/opentk/opentk)
- [`Newtonsoft.Json`](https://www.nuget.org/packages/Newtonsoft.Json)
- [`Jitter2`](https://www.nuget.org/packages/Jitter2)
- [`Box2D`](https://www.nuget.org/packages/Box2D.NetStandard/)
- [`LibNoise`](https://www.nuget.org/packages/LibNoise)

# 💻 Platforms
[<img src="https://github.com/MrScautHD/Sparkle/assets/65916181/a92bd5fa-517b-44c2-ab58-cc01b5ae5751" alt="windows" width="70" height="70" align="right">](https://www.microsoft.com/de-at/windows)
### Windows
- Using `OpenGL-4.3`

[<img src="https://github.com/MrScautHD/Sparkle/assets/65916181/f9e643a8-4d46-450c-91ac-d220394ecd42" alt="Linux" width="70" height="70" align="right">](https://www.ubuntu.com/)
### Linux
- Using `OpenGL-4.3`

[<img src="https://github.com/MrScautHD/Sparkle/assets/65916181/e37eb15f-4237-47ae-9ae7-e4455f7c3d92" alt="macOS" width="70" height="70" align="right">](https://www.apple.com/at/macos/sonoma/)
### MacOS
- Using `OpenGL-3.3`

🧑 Contributors
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
        JsonConfig config = new JsonConfigBuilder("config", "test")
            .Add("Hello", "Hello World!")
            .Add("Bye", 1000)
            .Build();

        Logger.Info(config.GetValue<string>("Hello"));
    }

    protected override void Init() {
        base.Init();
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
            Graphics.DrawTexture(this.Texture, Vector3.Zero, Color.White);
        }
    }
}
```

💸 Sponsors
============
Please consider [SPONSOR](https://github.com/sponsors/MrScautHD) me to further help development and to allow faster issue triaging and new features to be implemented.
