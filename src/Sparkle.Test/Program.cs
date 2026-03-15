using Bliss.CSharp.Logging;
using Sparkle.CSharp;
using Sparkle.CSharp.GUI.Loading;
using Sparkle.Test.CSharp;
using Sparkle.Test.CSharp.Dim3D;

try {
    GameSettings settings = new GameSettings() {
        Title = "Sparkle - [Test]"
    };
    
    using TestGame testGame = new TestGame(settings);
    testGame.Run(new PlayerMovementScene(), new LogoLoadingGui("Loading", "content/sparkle/images/logo.png"));
}
catch (Exception ex) {
    Logger.Error(ex.ToString());
    Environment.ExitCode = 1;
}