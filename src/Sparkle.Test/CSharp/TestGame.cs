using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Keyboards;
using Sparkle.CSharp;
using Sparkle.CSharp.GUI;
using Sparkle.CSharp.Overlays;
using Sparkle.CSharp.Registries;
using Sparkle.Test.CSharp.Menus;

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

    protected override void Update(double delta) {
        base.Update(delta);
        
        if (Input.IsKeyPressed(KeyboardKey.V)) {
            if (GuiManager.ActiveGui == null) {
                GuiManager.SetGui(new SceneSwitcherMenu("Scene-Switcher"));
            }
            else {
                GuiManager.SetGui(null);
            }
        }
    }
}