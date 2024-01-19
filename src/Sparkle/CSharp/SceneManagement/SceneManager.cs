using Sparkle.CSharp.Entities;

namespace Sparkle.CSharp.SceneManagement; 

public static class SceneManager {
    
    public static Scene? ActiveScene { get; private set; }
    
    public static Cam3D? MainCam3D { get; private set; }
    public static Cam2D? MainCam2D { get; private set; }

    /// <summary>
    /// Initializes the current scene and sets the main camera if available.
    /// </summary>
    internal static void Init() {
        ActiveScene?.Init();
        MainCam3D = (Cam3D) ActiveScene?.GetEntitiesWithTag("camera3D").FirstOrDefault()!;
        MainCam2D = (Cam2D) ActiveScene?.GetEntitiesWithTag("camera2D").FirstOrDefault()!;
    }
    
    /// <summary>
    /// Is invoked during each tick and is used for updating dynamic elements and game logic.
    /// </summary>
    internal static void Update() {
        ActiveScene?.Update();
    }
    
    /// <summary>
    /// Called after the Update method on each tick to further update dynamic elements and game logic.
    /// </summary>
    internal static void AfterUpdate() {
        ActiveScene?.AfterUpdate();
    }
    
    /// <summary>
    /// Is invoked at a fixed rate of every <see cref="GameSettings.FixedTimeStep"/> frames following the <see cref="AfterUpdate"/> method.
    /// It is used for handling physics and other fixed-time operations.
    /// </summary>
    internal static void FixedUpdate() {
        ActiveScene?.FixedUpdate();
    }
    
    /// <summary>
    /// Is called every tick, used for rendering stuff.
    /// </summary>
    internal static void Draw() {
        ActiveScene?.Draw();
    }
    
    /// <summary>
    /// Sets the default scene without disposing of the previous scene or initializing the new scene.
    /// </summary>
    /// <param name="scene">The scene to be set as the default scene.</param>
    internal static void SetDefaultScene(Scene? scene) {
        ActiveScene = scene;
    }
    
    /// <summary>
    /// Sets the active scene, disposes of the previous scene, and initializes the new scene.
    /// </summary>
    /// <param name="scene">The scene to be set as the active scene.</param>
    public static void SetScene(Scene? scene) {
        ActiveScene?.Dispose();
        ActiveScene = scene;
        ActiveScene?.Init();
        MainCam3D = (Cam3D) ActiveScene?.GetEntitiesWithTag("camera3D").FirstOrDefault()!;
        MainCam2D = (Cam2D) ActiveScene?.GetEntitiesWithTag("camera2D").FirstOrDefault()!;
    }
}