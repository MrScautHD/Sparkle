using System.Drawing;
using Sparkle.csharp;
using Test;

ApplicationSettings settings = new ApplicationSettings() {
    Title = "Test",
    Size = new Size(1280, 720),
    TargetFps = 0,
    Headless = false
};

using TestGame application = new TestGame(settings);
application.Run();