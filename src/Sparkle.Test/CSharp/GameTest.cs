using Sparkle.CSharp;
using Sparkle.CSharp.Overlays;

namespace Sparkle.Test.CSharp;

public class GameTest : Game {
    
    public GameTest(GameSettings settings) : base(settings) { }

    protected override void Init() {
        base.Init();

        TestOverlay overlay = new TestOverlay("TEST", true);
        OverlayManager.Add(overlay);
    }
}