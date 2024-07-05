﻿using Raylib_CSharp.Windowing;
using Sparkle.CSharp;
using Sparkle.Test.CSharp;
using Sparkle.Test.CSharp.Dim3;

GameSettings settings = new GameSettings() {
    Title = "Sparkle - [Test]",
    WindowFlags = /*ConfigFlags.Msaa4XHint |*/ ConfigFlags.ResizableWindow
};

using TestGame game = new TestGame(settings);
game.Run(new Test3DScene("3D"));

//using TestGame game = new TestGame(settings);
//game.Run(new Test2DScene("2D"));