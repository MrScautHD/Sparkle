using Bliss.CSharp.Colors;
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

    public static Camera2D? ActiveCam2D;
    public static Camera3D? ActiveCam3D;

    public static Simulation? Simulation => ActiveScene?.Simulation;

    private static FullScreenRenderPass _filterRenderPass;
    private static RenderTexture2D _filterRenderTexture;
    private static TextureSampleCount _sampleCount;
    
    internal static void Init(GraphicsDevice graphicsDevice, IWindow window, Scene? defaultScene = null, TextureSampleCount sampleCount = TextureSampleCount.Count1) {
        ActiveScene = defaultScene;
        _sampleCount = sampleCount;
        _filterRenderPass = new FullScreenRenderPass(graphicsDevice);
        _filterRenderTexture = new RenderTexture2D(graphicsDevice, (uint) window.GetWidth(), (uint) window.GetHeight(), sampleCount);
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

    internal static void OnDraw(GraphicsContext context, Framebuffer framebuffer) {
        context.CommandList.SetFramebuffer(_filterRenderTexture.Framebuffer);
        context.CommandList.ClearColorTarget(0, Color.DarkGray.ToRgbaFloat());
        
        switch (ActiveScene?.Type) {
            case SceneType.Scene2D:
                ActiveCam2D?.Begin(context.CommandList);
                ActiveScene.Draw(context, _filterRenderTexture.Framebuffer);
                ActiveCam2D?.End();
                break;
            
            case SceneType.Scene3D:
                ActiveCam3D?.Begin(context.CommandList);
                ActiveScene.Draw(context, _filterRenderTexture.Framebuffer);
                ActiveCam3D?.End();
                break;
        }
        
        // Apply MSAA. 
        if (_sampleCount != TextureSampleCount.Count1) {
            context.CommandList.ResolveTexture(_filterRenderTexture.ColorTexture, _filterRenderTexture.DestinationTexture);
        }
        
        // Draw Post-Processing texture.
        context.CommandList.SetFramebuffer(framebuffer);
        _filterRenderPass.Draw(context.CommandList, _filterRenderTexture, framebuffer.OutputDescription, ActiveScene?.FilterEffect);
    }

    internal static void OnResize(Rectangle rectangle) {
        ActiveScene?.Resize(rectangle);
        _filterRenderTexture.Resize((uint) rectangle.Width, (uint) rectangle.Height);
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
        _filterRenderPass.Dispose();
        _filterRenderTexture.Dispose();
    }
}