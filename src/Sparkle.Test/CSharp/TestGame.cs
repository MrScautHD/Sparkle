using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Keyboards;
using Sparkle.CSharp;
using Sparkle.CSharp.Loading;
using Sparkle.CSharp.Overlays;
using Sparkle.CSharp.Registries;
using Sparkle.CSharp.Scenes;
using Sparkle.Test.CSharp.Dim2D;
using Sparkle.Test.CSharp.Dim3D;

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
            SceneManager.SetScene(new TestScene2D("TEST"), LoadingScreen.Loading);
        }
        
        if (Input.IsKeyPressed(KeyboardKey.B)) {
            SceneManager.SetScene(new TestScene3D("TEST"), LoadingScreen.Loading);
        }
    }
}