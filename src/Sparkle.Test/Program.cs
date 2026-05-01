using System.Diagnostics;
using Bliss.CSharp.Logging;
using Sparkle.CSharp;
using Sparkle.CSharp.GUI.Loading;
using Sparkle.Test.CSharp;
using Sparkle.Test.CSharp.Dim3D;
using Veldrid;

GameSettings settings = new GameSettings() {
    Title = "Sparkle - [Test]",
    Backend = GraphicsBackend.Direct3D11,
    VSync = false // For some reason on 240+ Hz monitors on Windows, it starts stuttering (Solutions: 1. Move to Linux, 2. Set the Hz in your windows settings down for the monitor, 3. Just don't use vsync).
};

TestGame game = new TestGame(settings);

try {
    game.Run(new TestScene3D(), new LogoLoadingGui("Loading", "content/sparkle/images/logo.png"));
}
catch (Exception ex) {
    StackFrame? frame = new StackTrace(ex, true).GetFrame(0);
    string sourceFile = frame?.GetFileName() ?? "Unknown";
    string member = frame?.GetMethod()?.Name ?? "Unknown";
    int line = frame?.GetFileLineNumber() ?? 0;
    
    Logger.Error(ex.ToString(), sourceFile, member, line);
    Environment.Exit(1);
}
finally {
    try {
        game.Dispose();
    }
    catch {
        // Suppress Dispose() exceptions so the original isn't masked.
    }
}