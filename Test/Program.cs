using System.Drawing;
using Sparkle.csharp;
using Test;
using Raylib_cs;

ApplicationSettings settings = new ApplicationSettings() {
    Title = "Test",
    Size = new Size(1280, 720),
    TargetFps = 0,
    Headless = false,
    Icon = Raylib.LoadImage("./src/logo.png"),
};

using TestGame application = new TestGame(settings);
application.Run();