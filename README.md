<p align="center" style="margin-bottom: 0px !important;">
  <img width="512" src="https://cdn.discordapp.com/attachments/1036960672715644939/1131937257578836088/imaged.png" alt="Logo" align="center">
</p>

<h1 align="center" style="margin-top: 0px;">Welcome to Sparkle 🎉</h1>
<h4 align="center">A fast, Cross-platform .NET 8 C# 12 game engine.</h4>

![grafik](https://user-images.githubusercontent.com/65916181/220327780-328a50de-def5-485a-b769-1f98b5c292ad.png)

[<img src="https://cdn.discordapp.com/attachments/1023302120755187753/1216795862072688740/image-1.png?ex=6601b074&is=65ef3b74&hm=8c2c0e9bdafae052b6118485b7d4bca0438ebd10f73d30980d6d064fa4f75fd2&" width="186" height="60">](https://discord.gg/7XKw6YQa76)

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
- [Jitter2](https://www.nuget.org/packages/Jitter2)

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
