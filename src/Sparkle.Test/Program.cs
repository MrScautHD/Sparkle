using Sparkle.CSharp;
using Sparkle.Test.CSharp;
using Sparkle.Test.CSharp.Dim3D;

GameSettings settings = new GameSettings() {
    Title = "Sparkle - [Test]"
};

using TestGame testGame = new TestGame(settings);
testGame.Run(new TestScene3D("Test - [3D]"));