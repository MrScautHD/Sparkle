using Bliss.CSharp.Logging;
using SDL;
using Sparkle.CSharp;
using Sparkle.CSharp.GUI.Loading;
using Sparkle.Test.CSharp;
using Sparkle.Test.CSharp.Dim3D;
using Veldrid;

try {
    GameSettings settings = new GameSettings() {
        Title = "Sparkle - [Test]",
        VSync = false // For some reason on 240+ Hz monitors on Windows, it starts stuttering (Solutions: 1. Move to Linux, 2. Set the Hz in your windows settings down for the monitor, 3. Just don't use vsync).
    };
    
    using TestGame testGame = new TestGame(settings);
    testGame.Run(new PlayerMovementScene(), new LogoLoadingGui("Loading", "content/sparkle/images/logo.png"));
}
catch (Exception ex) {
    Logger.Error(ex.ToString());
    Environment.ExitCode = 1;
}