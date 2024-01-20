using Raylib_cs;
using Sparkle.CSharp;
using Test;

GameSettings settings = new GameSettings() {
    Title = "Test - [Sparkle]",
    WindowFlags = ConfigFlags.Msaa4xHint | ConfigFlags.ResizableWindow
};

using TestGame game = new TestGame(settings);
game.Run(new Test3DScene("test"));