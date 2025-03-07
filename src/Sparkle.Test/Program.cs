using Sparkle.CSharp;
using Sparkle.Test.CSharp;
using Sparkle.Test.CSharp.Dim3;

GameSettings settings = new GameSettings() {
    Title = "Sparkle - [Test]"
};

using TestGame game = new TestGame(settings);
game.Run(new Test3DScene("3D"));

//using TestGame game = new TestGame(settings);
//game.Run(new Test2DScene("2D"));