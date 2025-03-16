using Bliss.CSharp.Graphics.Rendering.Passes;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
using Bliss.CSharp.Windowing;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.Physics;
using Veldrid;

namespace Sparkle.CSharp.Scenes;

public static class SceneManager {
    
    public static Scene? ActiveScene { get; private set; }
    public static FullScreenRenderPass FilterRenderPass { get; private set; }
    public static RenderTexture2D FilterRenderTexture { get; private set; }

    public static Camera2D? ActiveCam2D;
    public static Camera3D? ActiveCam3D;

    public static Simulation? Simulation => ActiveScene?.Simulation;

    internal static void Init(GraphicsDevice graphicsDevice, IWindow window, Scene? defaultScene = null) {
        ActiveScene = defaultScene;
        FilterRenderPass = new FullScreenRenderPass(graphicsDevice);
        FilterRenderTexture = new RenderTexture2D(graphicsDevice, (uint) window.GetWidth(), (uint) window.GetHeight());
    }

    internal static void OnInit() {
        ActiveScene?.Init();
        ActiveCam2D = (Camera2D) ActiveScene?.GetEntitiesWithTag("camera2D").FirstOrDefault()!;
        ActiveCam3D = (Camera3D) ActiveScene?.GetEntitiesWithTag("camera3D").FirstOrDefault()!;
    }

    internal static void OnUpdate(double delta) {
        ActiveScene?.Update(delta);
    }

    internal static void OnAfterUpdate(double delta) {
        ActiveScene?.AfterUpdate(delta);
    }

    internal static void OnFixedUpdate(double timeStep) {
        ActiveScene?.FixedUpdate(timeStep);
    }

    internal static void OnDraw(GraphicsContext context) {
        switch (ActiveScene?.Type) {
            case SceneType.Scene2D:
                ActiveCam2D?.Begin(context.CommandList);
                ActiveScene.Draw(context);
                ActiveCam2D?.End();
                break;
            
            case SceneType.Scene3D:
                ActiveCam3D?.Begin(context.CommandList);
                ActiveScene.Draw(context);
                ActiveCam3D?.End();
                break;
        }
    }

    internal static void OnResize(Rectangle rectangle) {
        ActiveScene?.Resize(rectangle);
    }

    public static void SetScene(Scene? scene) {
        ActiveScene?.Dispose();
        ActiveScene = scene;
        ActiveScene?.Init();
        ActiveCam2D = (Camera2D) ActiveScene?.GetEntitiesWithTag("camera2D").FirstOrDefault()!;
        ActiveCam3D = (Camera3D) ActiveScene?.GetEntitiesWithTag("camera3D").FirstOrDefault()!;
    }

    internal static void Destroy() {
        ActiveScene?.Dispose();
    }
}