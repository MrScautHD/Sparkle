using Sparkle.CSharp;
using Sparkle.CSharp.Overlays;
using Sparkle.CSharp.Registries;

namespace Sparkle.Test.CSharp;

public class TestGame : Game {
    
    public TestGame(GameSettings settings) : base(settings) { }

    protected override void OnRun() {
        base.OnRun();
        RegistryManager.AddRegistry(new ContentRegistry());
    }

    protected override void Init() {
        base.Init();

        TestOverlay overlay = new TestOverlay("TEST", true);
        OverlayManager.AddOverlay(overlay);
    }
}