using Bliss.CSharp.Windowing;
using Sparkle.CSharp;
using Sparkle.Test.CSharp;
using Sparkle.Test.CSharp.Dim2D;
using Sparkle.Test.CSharp.Dim3D;

GameSettings settings = new GameSettings() {
    Title = "Sparkle - [Test]",
    Backend = Window.GetPlatformDefaultBackend()
};

using TestGame testGame = new TestGame(settings);
//testGame.Run(new TestScene2D("Test - [2D]"));
testGame.Run(new TestScene3D("Test - [3D]"));