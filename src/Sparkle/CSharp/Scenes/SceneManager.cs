using System.Numerics;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Textures;
using Raylib_CSharp.Transformations;
using Raylib_CSharp.Windowing;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Physics;

namespace Sparkle.CSharp.Scenes;

public static class SceneManager {
    
    public static Scene? ActiveScene { get; private set; }
    public static RenderTexture2D FilterTexture { get; private set; }
    
    public static Cam3D? ActiveCam3D;
    public static Cam2D? ActiveCam2D;
    
    public static Simulation? Simulation => ActiveScene?.Simulation;
    
    /// <summary>
    /// Initializes the current scene and sets the main camera if available.
    /// </summary>
    internal static void Init() {
        ActiveScene?.Init();
        FilterTexture = RenderTexture2D.Load(Window.GetRenderWidth(), Window.GetRenderHeight());
        ActiveCam3D = (Cam3D) ActiveScene?.GetEntitiesWithTag("camera3D").FirstOrDefault()!;
        ActiveCam2D = (Cam2D) ActiveScene?.GetEntitiesWithTag("camera2D").FirstOrDefault()!;
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
        if (Window.IsResized()) {
            FilterTexture.Unload();
            FilterTexture = RenderTexture2D.Load(Window.GetRenderWidth(), Window.GetRenderHeight());
        }
        
        Graphics.BeginTextureMode(FilterTexture);
        Graphics.ClearBackground(Color.SkyBlue);
        
        switch (ActiveScene?.Type) {
            
            case SceneType.Scene3D:
                ActiveCam3D?.BeginMode3D();
                ActiveScene.Draw();
                ActiveCam3D?.EndMode3D();
                break;
            
            case SceneType.Scene2D:
                ActiveCam2D?.BeginMode2D();
                ActiveScene.Draw();
                ActiveCam2D?.EndMode2D();
                break;
        }
        
        Graphics.EndTextureMode();

        if (ActiveScene?.FilterEffect != null) {
            ActiveScene.FilterEffect.Apply();
            
            Graphics.BeginShaderMode(ActiveScene.FilterEffect.Shader);
            Graphics.DrawTextureRec(FilterTexture.Texture, new Rectangle(0, 0, FilterTexture.Texture.Width, -FilterTexture.Texture.Height), Vector2.Zero, Color.White);
            Graphics.EndShaderMode();
        }
        else {
            Graphics.DrawTextureRec(FilterTexture.Texture, new Rectangle(0, 0, FilterTexture.Texture.Width, -FilterTexture.Texture.Height), Vector2.Zero, Color.White);
        }
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
        ActiveCam3D = (Cam3D) ActiveScene?.GetEntitiesWithTag("camera3D").FirstOrDefault()!;
        ActiveCam2D = (Cam2D) ActiveScene?.GetEntitiesWithTag("camera2D").FirstOrDefault()!;
    }

    /// <summary>
    /// Performs cleanup operations.
    /// </summary>
    public static void Destroy() {
        ActiveScene?.Dispose();
        FilterTexture.Unload();
    }
}