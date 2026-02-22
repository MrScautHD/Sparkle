using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Keyboards;
using Bliss.CSharp.Logging;
using Sparkle.CSharp;
using Sparkle.CSharp.GUI.Loading;
using Sparkle.CSharp.Overlays;
using Sparkle.CSharp.Registries;
using Sparkle.CSharp.Scenes;
using Sparkle.CSharp.Utils.Async;
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
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(new TestScene2D("TEST"), new ProgressBarLoadingGui("Loading"));
            
            asyncOperation.Completed += (success) => {
                if (success) {
                    Logger.Warn("LOADED");
                }
            };
        }
        
        if (Input.IsKeyPressed(KeyboardKey.B)) {
            SceneManager.LoadSceneAsync(new TestScene3D("TEST"), new ProgressBarLoadingGui("Loading"));
        }
    }
}