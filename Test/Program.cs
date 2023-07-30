using System.Drawing;
using Raylib_cs;
using Sparkle.csharp;
using Test;

GameSettings settings = new GameSettings() {
    Title = "Test - [Sparkle]",
    Size = new Size(1280, 720),
    WindowStates = new[] {
        ConfigFlags.FLAG_MSAA_4X_HINT,
        ConfigFlags.FLAG_WINDOW_RESIZABLE,
    }
};

using TestGame game = new TestGame(settings);
game.Run(new TestScene("test"));