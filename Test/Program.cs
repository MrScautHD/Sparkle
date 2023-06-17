using System.Drawing;
using Raylib_cs;
using Sparkle.csharp;
using Test;

GameSettings settings = new GameSettings() {
    Title = "Test - [Sparkle]",
    Size = new Size(1280, 720),
    Headless = false,
    WindowStates = new[] {
        ConfigFlags.FLAG_WINDOW_RESIZABLE
    }
};

using TestGame application = new TestGame(settings, new TestScene("earth"));
application.Run();