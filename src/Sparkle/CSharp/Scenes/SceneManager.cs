using Bliss.CSharp.Colors;
using Bliss.CSharp.Effects;
using Bliss.CSharp.Images;
using Bliss.CSharp.Logging;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
using Bliss.CSharp.Windowing;
using Sparkle.CSharp.Content;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.GUI;
using Sparkle.CSharp.GUI.Loading;
using Sparkle.CSharp.Physics;
using Veldrid;

namespace Sparkle.CSharp.Scenes;

public static class SceneManager {
    
    /// <summary>
    /// The graphics device used for rendering.
    /// </summary>
    public static GraphicsDevice GraphicsDevice { get; private set; }
    
    /// <summary>
    /// Gets the currently active scene.
    /// </summary>
    public static Scene? ActiveScene { get; private set; }
    
    /// <summary>
    /// Indicates whether a scene is currently being loaded.
    /// </summary>
    public static bool IsLoading { get; private set; }

    /// <summary>
    /// An object used to synchronize access to critical sections of code
    /// during scene loading operations in the SceneManager.
    /// </summary>
    private static readonly object LoadLock = new();
    
    /// <summary>
    /// The render target used for filter effects.
    /// </summary>
    public static RenderTexture2D FilterTarget { get; private set; }

    /// <summary>
    /// The final texture result obtained after applying filter effects to a scene.
    /// </summary>
    public static Texture2D FilterResult { get; private set; }
    
    /// <summary>
    /// The render target used for post-processing effects.
    /// </summary>
    public static RenderTexture2D PostProcessingTarget { get; private set; }

    /// <summary>
    /// The final texture result obtained after applying post-processing effects to a scene.
    /// </summary>
    public static Texture2D PostProcessingResult { get; private set; }
    
    /// <summary>
    /// The post-processing effect applied to the rendered scene.
    /// </summary>
    public static Effect? PostEffect;
    
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
    /// Initializes the SceneManager with the specified parameters.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used for rendering.</param>
    /// <param name="window">The application window.</param>
    /// <param name="defaultScene">The default scene to load, if any.</param>
    /// <param name="sampleCount">The sample count for anti-aliasing.</param>
    internal static void Init(GraphicsDevice graphicsDevice, IWindow window, Scene? defaultScene = null, TextureSampleCount sampleCount = TextureSampleCount.Count1) {
        GraphicsDevice = graphicsDevice;
        ActiveScene = defaultScene;
        
        // Create a filter render target.
        FilterTarget = new RenderTexture2D(graphicsDevice, (uint) window.GetWidth(), (uint) window.GetHeight(), sampleCount: sampleCount);
        FilterResult = new Texture2D(graphicsDevice, new Image(window.GetWidth(), window.GetHeight()), false);
        
        // Create a post-processing render target.
        PostProcessingTarget = new RenderTexture2D(graphicsDevice, (uint) window.GetWidth(), (uint) window.GetHeight());
        PostProcessingResult = new Texture2D(graphicsDevice, new Image(window.GetWidth(), window.GetHeight()), false);
    }
    
    /// <summary>
    /// Loads the active scene.
    /// </summary>
    internal static void OnLoad(ContentManager content) {
        Logger.Info("Load active scene content...");
        ActiveScene?.Load(content);
        Logger.Info($"Scene {ActiveScene?.Name} content loaded successfully.");
    }
    
    /// <summary>
    /// Initializes the active scene.
    /// </summary>
    internal static void OnInit() {
        Logger.Info("Initialize active scene...");
        ActiveScene?.Init();
        ActiveCam2D = (Camera2D) ActiveScene?.GetEntitiesWithTag("camera2D").FirstOrDefault()!;
        ActiveCam3D = (Camera3D) ActiveScene?.GetEntitiesWithTag("camera3D").FirstOrDefault()!;
        Logger.Info($"Scene {ActiveScene?.Name} initialized successfully.");
    }
    
    /// <summary>
    /// Updates the active scene logic.
    /// </summary>
    /// <param name="delta">The time elapsed since the last update.</param>
    internal static void OnUpdate(double delta) {
        lock (LoadLock) {
            if (GuiManager.ActiveGui is LoadingGui || IsLoading) {
                return;
            }
        
            ActiveScene?.Update(delta);
        }
    }

    /// <summary>
    /// Performs post-update logic for the active scene.
    /// </summary>
    /// <param name="delta">The time elapsed since the last update.</param>
    internal static void OnAfterUpdate(double delta) {
        lock (LoadLock) {
            if (GuiManager.ActiveGui is LoadingGui || IsLoading) {
                return;
            }
        
            ActiveScene?.AfterUpdate(delta);
        }
    }
    
    /// <summary>
    /// Handles fixed time-step updates for the active scene.
    /// </summary>
    /// <param name="fixedStep">The fixed time-step duration.</param>
    internal static void OnFixedUpdate(double fixedStep) {
        lock (LoadLock) {
            if (GuiManager.ActiveGui is LoadingGui || IsLoading) {
                return;
            }
        
            ActiveScene?.FixedUpdate(fixedStep);
        }
    }

    /// <summary>
    /// Draws the active scene.
    /// </summary>
    /// <param name="context">The graphics context used for drawing.</param>
    /// <param name="framebuffer">The framebuffer to render into.</param>
    internal static void OnDraw(GraphicsContext context, Framebuffer framebuffer) {
        lock (LoadLock) {
            if (GuiManager.ActiveGui is LoadingGui || IsLoading) {
                return;
            }
            
            context.CommandList.SetFramebuffer(FilterTarget.Framebuffer);
            context.CommandList.ClearColorTarget(0, Color.DarkGray.ToRgbaFloat());
            context.CommandList.ClearDepthStencil(1.0F);
            
            switch (ActiveScene?.SceneType) {
                case SceneType.Scene2D:
                    ActiveCam2D?.Begin();
                    ActiveScene.Draw(context, FilterTarget.Framebuffer);
                    ActiveCam2D?.End();
                    break;
                
                case SceneType.Scene3D:
                    ActiveCam3D?.Begin();
                    ActiveScene.Draw(context, FilterTarget.Framebuffer);
                    ActiveCam3D?.End();
                    break;
            }
            
            // Apply MSAA. 
            if (FilterTarget.SampleCount != TextureSampleCount.Count1) {
                context.CommandList.ResolveTexture(FilterTarget.ColorTexture, FilterResult.DeviceTexture);
            }
            else {
                context.CommandList.CopyTexture(FilterTarget.ColorTexture, FilterResult.DeviceTexture);
            }
            
            if (PostEffect != null) {
                
                // Draw the filter effect into the post-processing framebuffer.
                context.CommandList.SetFramebuffer(PostProcessingTarget.Framebuffer);
                context.FullScreenRenderer.Draw(context.CommandList, FilterResult, PostProcessingTarget.Framebuffer.OutputDescription, ActiveScene?.FilterEffect);
                
                // Draw the post-processing effect into the final framebuffer.
                context.CommandList.SetFramebuffer(framebuffer);
                context.CommandList.CopyTexture(PostProcessingTarget.ColorTexture, PostProcessingResult.DeviceTexture);
                context.FullScreenRenderer.Draw(context.CommandList, PostProcessingResult, framebuffer.OutputDescription, PostEffect);
            }
            else {
                
                // Draw the filter effect into the final framebuffer.
                context.CommandList.SetFramebuffer(framebuffer);
                context.FullScreenRenderer.Draw(context.CommandList, FilterResult, framebuffer.OutputDescription, ActiveScene?.FilterEffect);
            }
        }
    }

    /// <summary>
    /// Handles window resize events and updates rendering buffers accordingly.
    /// </summary>
    /// <param name="rectangle">The new window size.</param>
    internal static void OnResize(Rectangle rectangle) {
        ActiveScene?.Resize(rectangle);
        
        // Resize filter target.
        FilterTarget.Resize((uint) rectangle.Width, (uint) rectangle.Height);
        FilterResult.Dispose();
        FilterResult = new Texture2D(GraphicsDevice, new Image(rectangle.Width, rectangle.Height), false);
        
        // Resize post-processing target.
        PostProcessingTarget.Resize((uint) rectangle.Width, (uint) rectangle.Height);
        PostProcessingResult.Dispose();
        PostProcessingResult = new Texture2D(GraphicsDevice, new Image(rectangle.Width, rectangle.Height), false);
    }

    /// <summary>
    /// Sets the specified scene as the active scene and initializes it, with an optional loading screen displayed during the loading process.
    /// </summary>
    /// <param name="scene">The scene to set as the active scene.</param>
    /// <param name="loadingGui">An optional loading GUI to display during scene loading. If null, no loading screen is shown.</param>
    public static void SetScene(Scene? scene, LoadingGui? loadingGui = null) {
        lock (LoadLock) {
            if (IsLoading) {
                Logger.Warn($"A scene is already being loaded. Ignoring request to load: {scene?.Name}");
                return;
            }
            
            IsLoading = true;
        }
        
        if (loadingGui == null) {
            Logger.Info($"Setting active scene to: {scene?.Name}");
            
            ActiveScene?.Dispose();
            ActiveScene = scene;
            
            Logger.Info("Load active scene content...");
            if (Game.Instance?.Content != null) ActiveScene?.Load(Game.Instance.Content);
            Logger.Info($"Scene {scene?.Name} content loaded successfully.");
            
            Logger.Info("Initialize active scene...");
            ActiveScene?.Init();
            ActiveCam2D = (Camera2D) ActiveScene?.GetEntitiesWithTag("camera2D").FirstOrDefault()!;
            ActiveCam3D = (Camera3D) ActiveScene?.GetEntitiesWithTag("camera3D").FirstOrDefault()!;
            Logger.Info($"Scene {scene?.Name} initialized successfully.");

            IsLoading = false;
            return;
        }
        
        GuiManager.SetGui(loadingGui);
        
        Task.Run(() => {
            DateTime startTime = DateTime.Now;
            loadingGui.Progress = 0.0F;
            
            Logger.Info($"Setting active scene to: {scene?.Name}");
            
            ActiveScene?.Dispose();
            ActiveScene = scene;
            loadingGui.Progress = 0.2F;
            
            Logger.Info("Load active scene content...");
            if (Game.Instance?.Content != null) ActiveScene?.Load(Game.Instance.Content);
            loadingGui.Progress = 0.7F;
            Logger.Info($"Scene {scene?.Name} content loaded successfully.");
            
            Logger.Info("Initialize active scene...");
            ActiveScene?.Init();
            loadingGui.Progress = 0.9F;
            
            float elapsed = (float) (DateTime.Now - startTime).TotalSeconds;
            float remaining = Math.Max(0, loadingGui.MinTime - elapsed);
            
            if (remaining > 0) {
                Thread.Sleep((int) (remaining * 1000));
            }
            
            loadingGui.Progress = 1.0F;
            
            ActiveCam2D = (Camera2D) ActiveScene?.GetEntitiesWithTag("camera2D").FirstOrDefault()!;
            ActiveCam3D = (Camera3D) ActiveScene?.GetEntitiesWithTag("camera3D").FirstOrDefault()!;
            Logger.Info($"Scene {scene?.Name} initialized successfully.");
            
            GuiManager.SetGui(null);
            IsLoading = false;
        });
    }
    
    /// <summary>
    /// Cleans up resources associated with the scene manager.
    /// </summary>
    internal static void Destroy() {
        ActiveScene?.Dispose();
        FilterTarget.Dispose();
        FilterResult.Dispose();
        PostProcessingTarget.Dispose();
        PostProcessingResult.Dispose();
    }
}