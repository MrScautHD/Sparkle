using Sparkle.CSharp;
using Sparkle.Test.CSharp;

GameSettings settings = new GameSettings() {
    Title = "Sparkle - [Test]"
};

using GameTest game = new GameTest(settings);
game.Run(null);