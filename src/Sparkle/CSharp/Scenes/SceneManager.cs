using Bliss.CSharp.Colors;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
using Bliss.CSharp.Windowing;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.Physics;
using Veldrid;

namespace Sparkle.CSharp.Scenes;

public static class SceneManager {
    
    /// <summary>
    /// Gets the currently active scene.
    /// </summary>
    public static Scene? ActiveScene { get; private set; }

    /// <summary>
    /// The active 2D camera in the scene.
    /// </summary>
    public static Camera2D? ActiveCam2D;
    
    /// <summary>
    /// The active 3D camera in the scene.
    /// </summary>
    public static Camera3D? ActiveCam3D;

    /// <summary>
    /// Gets the simulation associated with the active scene.
    /// </summary>
    public static Simulation? Simulation => ActiveScene?.Simulation;
    
    /// <summary>
    /// The render texture used for rendering before post-processing.
    /// </summary>
    private static RenderTexture2D _filterRenderTexture;
    
    /// <summary>
    /// The sample count used for anti-aliasing.
    /// </summary>
    private static TextureSampleCount _sampleCount;
    
    /// <summary>
    /// Initializes the SceneManager with the specified parameters.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used for rendering.</param>
    /// <param name="window">The application window.</param>
    /// <param name="defaultScene">The default scene to load, if any.</param>
    /// <param name="sampleCount">The sample count for anti-aliasing.</param>
    internal static void Init(GraphicsDevice graphicsDevice, IWindow window, Scene? defaultScene = null, TextureSampleCount sampleCount = TextureSampleCount.Count1) {
        ActiveScene = defaultScene;
        _sampleCount = sampleCount;
        _filterRenderTexture = new RenderTexture2D(graphicsDevice, (uint) window.GetWidth(), (uint) window.GetHeight(), sampleCount);
    }
    
    /// <summary>
    /// Initializes the active scene.
    /// </summary>
    internal static void OnInit() {
        ActiveScene?.Init();
        ActiveCam2D = (Camera2D) ActiveScene?.GetEntitiesWithTag("camera2D").FirstOrDefault()!;
        ActiveCam3D = (Camera3D) ActiveScene?.GetEntitiesWithTag("camera3D").FirstOrDefault()!;
    }

    /// <summary>
    /// Updates the active scene logic.
    /// </summary>
    /// <param name="delta">The time elapsed since the last update.</param>
    internal static void OnUpdate(double delta) {
        ActiveScene?.Update(delta);
    }

    /// <summary>
    /// Performs post-update logic for the active scene.
    /// </summary>
    /// <param name="delta">The time elapsed since the last update.</param>
    internal static void OnAfterUpdate(double delta) {
        ActiveScene?.AfterUpdate(delta);
    }
    
    /// <summary>
    /// Handles fixed time-step updates for the active scene.
    /// </summary>
    /// <param name="fixedStep">The fixed time-step duration.</param>
    internal static void OnFixedUpdate(double fixedStep) {
        ActiveScene?.FixedUpdate(fixedStep);
    }

    /// <summary>
    /// Draws the active scene.
    /// </summary>
    /// <param name="context">The graphics context used for drawing.</param>
    /// <param name="framebuffer">The framebuffer to render into.</param>
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
        context.FullScreenRenderPass.Draw(context.CommandList, _filterRenderTexture, framebuffer.OutputDescription, ActiveScene?.FilterEffect);
    }

    /// <summary>
    /// Handles window resize events and updates rendering buffers accordingly.
    /// </summary>
    /// <param name="rectangle">The new window size.</param>
    internal static void OnResize(Rectangle rectangle) {
        ActiveScene?.Resize(rectangle);
        _filterRenderTexture.Resize((uint) rectangle.Width, (uint) rectangle.Height);
    }

    /// <summary>
    /// Sets a new active scene and initializes it.
    /// </summary>
    /// <param name="scene">The scene to set as active.</param>
    public static void SetScene(Scene? scene) {
        ActiveScene?.Dispose();
        ActiveScene = scene;
        ActiveScene?.Init();
        ActiveCam2D = (Camera2D) ActiveScene?.GetEntitiesWithTag("camera2D").FirstOrDefault()!;
        ActiveCam3D = (Camera3D) ActiveScene?.GetEntitiesWithTag("camera3D").FirstOrDefault()!;
    }

    /// <summary>
    /// Cleans up resources associated with the scene manager.
    /// </summary>
    internal static void Destroy() {
        ActiveScene?.Dispose();
        _filterRenderTexture.Dispose();
    }
}