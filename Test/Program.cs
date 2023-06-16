using System.Drawing;
using Sparkle.csharp;
using Test;

GameSettings settings = new GameSettings() {
    Title = "Test",
    Size = new Size(1280, 720),
    TargetFps = 0,
    Headless = false
};

using TestGame application = new TestGame(settings, null);
application.Run();