namespace Sparkle.csharp.scene; 

public static class SceneManager {
    
    public static Scene? ActiveScene { get; private set; }

    public static void SetScene(Scene scene) {
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
}