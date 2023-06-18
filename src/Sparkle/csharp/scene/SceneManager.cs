using Sparkle.csharp.entity;

namespace Sparkle.csharp.scene; 

public static class SceneManager {
    
    public static Scene? ActiveScene { get; private set; }
    public static Camera MainCamera => (Camera) ActiveScene!.GetEntitiesWithTag("camera")[0];
    
    internal static void SetDefaultScene(Scene scene) {
        ActiveScene = scene;
    }

    internal static void Init() {
        ActiveScene?.Init();
    }
    
    internal static void Update() {
        ActiveScene?.Update();
    }
    
    internal static void FixedUpdate() {
        ActiveScene?.FixedUpdate();
    }
    
    internal static void Draw() {
        ActiveScene?.Draw();
    }
    
    public static void SetScene(Scene scene) {
        ActiveScene?.Dispose();
        ActiveScene = scene;
        ActiveScene.Init();
    }
}