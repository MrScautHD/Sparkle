﻿using Raylib_cs;
using Sparkle.csharp;
using Test;

GameSettings settings = new GameSettings() {
    Title = "Test - [Sparkle]",
    WindowFlags = ConfigFlags.FLAG_MSAA_4X_HINT | ConfigFlags.FLAG_WINDOW_RESIZABLE
};

using TestGame game = new TestGame(settings);
game.Run(new Test3DScene("test"));