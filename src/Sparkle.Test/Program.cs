using Raylib_cs;
using Sparkle.CSharp;
using Sparkle.Test.CSharp;

GameSettings settings = new GameSettings() {
    Title = "Sparkle - [Test]",
    WindowFlags = ConfigFlags.Msaa4xHint | ConfigFlags.ResizableWindow,
};

using TestGame game = new TestGame(settings);
game.Run(new Test3DScene("test"));