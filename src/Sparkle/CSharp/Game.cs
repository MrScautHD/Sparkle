using Bliss.CSharp;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Graphics.Rendering.Renderers;
using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Primitives;
using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Sprites;
using Bliss.CSharp.Images;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Contexts;
using Bliss.CSharp.Logging;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
using Bliss.CSharp.Windowing;
using MiniAudioEx.Core.StandardAPI;
using Sparkle.CSharp.Content;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.GUI;
using Sparkle.CSharp.GUI.Loading;
using Sparkle.CSharp.Logging;
using Sparkle.CSharp.Overlays;
using Sparkle.CSharp.Registries;
using Sparkle.CSharp.Scenes;
using Veldrid;
using JLogger = Jitter2.Logger;

namespace Sparkle.CSharp;

public class Game : Disposable {
    
    /// <summary>
    /// The version of the game engine (Sparkle).
    /// </summary>
    public static readonly Version Version = new Version(5, 0, 0);

    /// <summary>
    /// The singleton instance of the game.
    /// </summary>
    public static Game? Instance { get; private set; }
    
    /// <summary>
    /// The settings for the game.
    /// </summary>
    public GameSettings Settings { get; private set; }
    
    /// <summary>
    /// The main window of the game.
    /// </summary>
    public IWindow MainWindow { get; private set; }
    
    /// <summary>
    /// The graphics device for rendering.
    /// </summary>
    public GraphicsDevice GraphicsDevice { get; private set; }
    
    /// <summary>
    /// The command list used for rendering commands.
    /// </summary>
    public CommandList CommandList { get; private set; }

    /// <summary>
    /// Represents the fullscreen render pass used during the rendering process.
    /// </summary>
    public FullScreenRenderer FullScreenRenderPass { get; private set; }

    /// <summary>
    /// The default (global) sprite batch used for rendering 2D sprites.
    /// </summary>
    public SpriteBatch GlobalSpriteBatch { get; private set; }

    /// <summary>
    /// The default (global) primitive batch used for rendering 2D geometric primitives.
    /// </summary>
    public PrimitiveBatch GlobalPrimitiveBatch { get; private set; }

    /// <summary>
    /// The default (global) immediate renderer used for rendering (Vertices...) in immediate mode.
    /// </summary>
    public ImmediateRenderer GlobalImmediateRenderer { get; private set; }

    /// <summary>
    /// An instance encapsulating core graphics rendering components associated with the game.
    /// </summary>
    public GraphicsContext GraphicsContext { get; private set; }
    
    /// <summary>
    /// The content manager used to load game assets.
    /// </summary>
    public ContentManager Content { get; private set; }
    
    /// <summary>
    /// Flag to indicate if the game should close.
    /// </summary>
    public bool ShouldClose;
    
    /// <summary>
    /// The render target.
    /// </summary>
    private RenderTexture2D _renderTarget;
    
    /// <summary>
    /// The render result.
    /// </summary>
    private Texture2D _renderResult;
    
    /// <summary>
    /// The log file writer used for logging messages to a file.
    /// </summary>
    private LogFileWriter _logFileWriter;
    
    /// <summary>
    /// The logger for jitter.
    /// </summary>
    private LogJitter _logJitter;
    
    /// <summary>
    /// The fixed frame rate for the game.
    /// </summary>
    private double _fixedFrameRate;
    
    /// <summary>
    /// The time step for fixed updates.
    /// </summary>
    private readonly double _fixedUpdateTimeStep;
    
    /// <summary>
    /// The timer for tracking the fixed update time.
    /// </summary>
    private double _fixedUpdateTimer;
    
    /// <summary>
    /// Initializes the <see cref="Game"/> with specified settings.
    /// </summary>
    /// <param name="settings">The game settings.</param>
    public Game(GameSettings settings) {
        Instance = this;
        this.Settings = settings;
        this._fixedUpdateTimeStep = settings.FixedTimeStep;
    }
    
    /// <summary>
    /// Runs the game with the specified scene and loading interface.
    /// </summary>
    /// <param name="scene">The initial scene to load. Can be null if no scene is to be loaded initially.</param>
    /// <param name="loadingGui">The graphical interface displayed while loading resources.</param>
    public void Run(Scene? scene, LoadingGui? loadingGui = null) {
        if (this.Settings.LogDirectory != string.Empty) {
            this._logFileWriter = new LogFileWriter(this.Settings.LogDirectory);
            Logger.Message += this._logFileWriter.WriteFileMsg;
        }
        
        // Setup jitter logger.
        this._logJitter = new LogJitter();
        JLogger.Listener += this._logJitter.Log;
        
        Logger.Info($"Hello World! Sparkle [{Version}] start...");
        Logger.Info($"\t> CPU: {SystemInfo.Cpu}");
        Logger.Info($"\t> MEMORY: Total: {SystemInfo.MemoryInfo.Total} MB, Available: {SystemInfo.MemoryInfo.Available} MB");
        Logger.Info($"\t> THREADS: {SystemInfo.Threads}");
        Logger.Info($"\t> OS: {SystemInfo.Os}");
        
        Logger.Info("Initialize window and graphics device...");
        GraphicsDeviceOptions options = new GraphicsDeviceOptions() {
            Debug = false,
            HasMainSwapchain = true,
            SwapchainDepthFormat = PixelFormat.D32FloatS8UInt,
            SyncToVerticalBlank = this.Settings.VSync,
            ResourceBindingModel = ResourceBindingModel.Improved,
            PreferDepthRangeZeroToOne = true,
            PreferStandardClipSpaceYDirection = true,
            SwapchainSrgbFormat = false
        };
        
        this.MainWindow = Window.CreateWindow(WindowType.Sdl3, this.Settings.Size.Width, this.Settings.Size.Height, this.Settings.Title, this.Settings.WindowFlags, options, this.Settings.Backend, out GraphicsDevice graphicsDevice);
        this.MainWindow.SetMinimumSize(this.Settings.MinSize.Width, this.Settings.MinSize.Height);
        this.MainWindow.Resized += () => this.OnResize(new Rectangle(this.MainWindow.GetX(), this.MainWindow.GetY(), this.MainWindow.GetWidth(), this.MainWindow.GetHeight()));
        this.GraphicsDevice = graphicsDevice;
        
        Logger.Info("\t> Window Info:");
        Logger.Info($"\t \t> Window type: {WindowType.Sdl3}");
        Logger.Info($"\t \t> Window Size: {this.MainWindow.GetWidth()} x {this.MainWindow.GetHeight()}");
        Logger.Info("\t> Device Info:");
        Logger.Info($"\t \t> Vendor: {this.GraphicsDevice.VendorName}");
        Logger.Info($"\t \t> Renderer: {this.GraphicsDevice.DeviceName}");
        Logger.Info($"\t \t> Backend type: {this.GraphicsDevice.BackendType}, Version: {this.GraphicsDevice.ApiVersion}");
        
        Logger.Info("Loading window icon...");
        this.MainWindow.SetIcon(this.Settings.IconPath != string.Empty ? new Image(this.Settings.IconPath) : new Image("content/sparkle/images/icon.png"));
        
        Logger.Info("Initialize input...");
        if (this.MainWindow is Sdl3Window) {
            Input.Init(new Sdl3InputContext(this.MainWindow));
        }
        else {
            Logger.Fatal("This type of window is not supported by the InputContext!");
        }
        
        Logger.Info("Initialize command list...");
        this.CommandList = graphicsDevice.ResourceFactory.CreateCommandList();
        
        Logger.Info("Initialize time...");
        Time.Init();
        
        Logger.Info($"Set target FPS to: {this.Settings.TargetFps}");
        this.SetTargetFps(this.Settings.TargetFps);
        
        Logger.Info("Initialize audio device...");
        AudioContext.Initialize(44100, 2);
        
        Logger.Info("Initialize global resources...");
        GlobalResource.Init(graphicsDevice);
        
        Logger.Info("Initialize global graphics assets manager...");
        GlobalGraphicsAssets.Init(graphicsDevice, this.MainWindow);
        
        Logger.Info("Initialize full screen renderer...");
        this.FullScreenRenderPass = new FullScreenRenderer(graphicsDevice);
        
        Logger.Info("Initialize render target texture...");
        this._renderTarget = new RenderTexture2D(graphicsDevice, (uint) this.MainWindow.GetWidth(), (uint) this.MainWindow.GetHeight(), sampleCount: this.Settings.SampleCount);
        this._renderResult = new Texture2D(graphicsDevice, new Image(this.MainWindow.GetWidth(), this.MainWindow.GetHeight()), false);
        
        Logger.Info("Initialize global sprite batch...");
        this.GlobalSpriteBatch = new SpriteBatch(graphicsDevice, this.MainWindow);
        
        Logger.Info("Initialize global primitive batch...");
        this.GlobalPrimitiveBatch = new PrimitiveBatch(graphicsDevice, this.MainWindow);
        
        Logger.Info("Initialize global immediate renderer...");
        this.GlobalImmediateRenderer = new ImmediateRenderer(graphicsDevice);
        
        Logger.Info("Initialize graphics context...");
        this.GraphicsContext = new GraphicsContext(graphicsDevice, this.CommandList, this.FullScreenRenderPass, this.GlobalSpriteBatch, this.GlobalPrimitiveBatch, this.GlobalImmediateRenderer);
        
        Logger.Info("Initialize content manager...");
        this.Content = new ContentManager(graphicsDevice);
        
        Logger.Info("Initialize overlay manager...");
        OverlayManager.Init();
        
        Logger.Info("Initialize GUI manager...");
        GuiManager.Init();
        
        Logger.Info("Initialize registry manager...");
        RegistryManager.Init();
        
        Logger.Info("Initialize scene manager...");
        SceneManager.Init(graphicsDevice, this.MainWindow, scene, this.Settings.SampleCount);
        
        this.OnRun();
        
        bool isLoaded = false;
        
        if (loadingGui == null) {
            Logger.Info("Load global graphics assets...");
            GlobalGraphicsAssets.Load(this.Content);
            
            Logger.Info("Load content...");
            this.Load(this.Content);
            
            Logger.Info("Initialize game...");
            this.Init();
        }
        else {
            GuiManager.SetLoadingGui(loadingGui);
            
            Task.Run(() => {
                DateTime startTime = DateTime.Now;
                loadingGui.Progress = 0.0F;
                
                Logger.Info("Load global graphics assets...");
                GlobalGraphicsAssets.Load(this.Content);
                loadingGui.Progress = 0.3F;
                
                Logger.Info("Load content...");
                this.Load(this.Content);
                loadingGui.Progress = 0.7F;
                
                Logger.Info("Initialize game...");
                this.Init();
                loadingGui.Progress = 0.9F;
                
                float elapsed = (float) (DateTime.Now - startTime).TotalSeconds;
                float remaining = Math.Max(0, loadingGui.MinTime - elapsed);
                
                if (remaining > 0.0F) {
                    Thread.Sleep((int) (remaining * 1000.0F));
                }
                
                loadingGui.Progress = 1.0F;
                
                GuiManager.SetLoadingGui(null);
                isLoaded = true;
            });
        }
        
        Logger.Info("Start game loop...");
        while (!this.ShouldClose && this.MainWindow.Exists) {
            if (this.GetTargetFps() != 0 && Time.DeltaTimer.Elapsed.TotalSeconds <= this._fixedFrameRate) {
                continue;
            }
            Time.Update();
            
            this.MainWindow.PumpEvents();
            Input.Begin();
            
            AudioContext.Update();
            
            if (isLoaded) {
                this.Update(Time.Delta);
                this.AfterUpdate(Time.Delta);
                
                this._fixedUpdateTimer += Time.Delta;
                while (this._fixedUpdateTimer >= this._fixedUpdateTimeStep) {
                    this.FixedUpdate(this._fixedUpdateTimeStep);
                    this._fixedUpdateTimer -= this._fixedUpdateTimeStep;
                }
            }
            else {
                GuiManager.OnUpdate(Time.Delta);
                GuiManager.OnAfterUpdate(Time.Delta);
                
                this._fixedUpdateTimer += Time.Delta;
                while (this._fixedUpdateTimer >= this._fixedUpdateTimeStep) {
                    GuiManager.OnFixedUpdate(this._fixedUpdateTimeStep);
                    this._fixedUpdateTimer -= this._fixedUpdateTimeStep;
                }
            }
            
            // Draw.
            this.CommandList.Begin();
            this.CommandList.SetFramebuffer(this._renderTarget.Framebuffer);
            this.CommandList.ClearColorTarget(0, Color.DarkGray.ToRgbaFloat());
            this.CommandList.ClearDepthStencil(1.0F);
            
            if (isLoaded) {
                this.Draw(this.GraphicsContext, this._renderTarget.Framebuffer);
            }
            else {
                GuiManager.OnDraw(this.GraphicsContext, this._renderTarget.Framebuffer);
            }
            
            // Apply MSAA.
            if (this._renderTarget.SampleCount != TextureSampleCount.Count1) {
                this.CommandList.ResolveTexture(this._renderTarget.ColorTexture, this._renderResult.DeviceTexture);
            }
            else {
                this.CommandList.CopyTexture(this._renderTarget.ColorTexture, this._renderResult.DeviceTexture);
            }
            
            // Draw render target texture.
            this.CommandList.SetFramebuffer(graphicsDevice.SwapchainFramebuffer);
            this.CommandList.ClearColorTarget(0, Color.DarkGray.ToRgbaFloat());
            
            this.FullScreenRenderPass.Draw(this.CommandList, this._renderResult, graphicsDevice.SwapchainFramebuffer.OutputDescription);
            
            this.CommandList.End();
            graphicsDevice.WaitForIdle();
            graphicsDevice.SubmitCommands(this.CommandList);
            graphicsDevice.SwapBuffers();
            
            Input.End();
        }
        
        Logger.Warn("Application shuts down!");
        this.OnClose();
    }

    /// <summary>
    /// Virtual method for additional setup when the game starts.
    /// </summary>
    protected virtual void OnRun() { }

    /// <summary>
    /// Loads the content for the game using the specified content manager and content key.
    /// </summary>
    /// <param name="content">The content manager responsible for managing game content.</param>
    protected virtual void Load(ContentManager content) {
        RegistryManager.OnLoad(content);
        SceneManager.OnLoad(content);
    }
    
    /// <summary>
    /// Initializes global game resources.
    /// </summary>
    protected virtual void Init() {
        RegistryManager.OnInit();
        SceneManager.OnInit();
    }

    /// <summary>
    /// Updates the game state, including scene and UI management.
    /// </summary>
    /// <param name="delta">The time delta since the last update.</param>
    protected virtual void Update(double delta) {
        SceneManager.OnUpdate(delta);
        OverlayManager.OnUpdate(delta);
        GuiManager.OnUpdate(delta);
    }
    
    /// <summary>
    /// Final update after regular updates are completed.
    /// </summary>
    /// <param name="delta">The time delta since the last update.</param>
    protected virtual void AfterUpdate(double delta) {
        SceneManager.OnAfterUpdate(delta);
        OverlayManager.OnAfterUpdate(delta);
        GuiManager.OnAfterUpdate(delta);
    }
    
    /// <summary>
    /// Executes fixed update logic with the specified time step.
    /// </summary>
    /// <param name="fixedStep">The fixed time step in seconds.</param>
    protected virtual void FixedUpdate(double fixedStep) {
        SceneManager.OnFixedUpdate(fixedStep);
        OverlayManager.OnFixedUpdate(fixedStep);
        GuiManager.OnFixedUpdate(fixedStep);
    }
    
    /// <summary>
    /// Renders the game scene to the screen.
    /// </summary>
    protected virtual void Draw(GraphicsContext context, Framebuffer framebuffer) {
        SceneManager.OnDraw(context, framebuffer);
        OverlayManager.OnDraw(context, framebuffer);
        GuiManager.OnDraw(context, framebuffer);
    }
    
    /// <summary>
    /// Handles window resizing events.
    /// </summary>
    protected virtual void OnResize(Rectangle rectangle) {
        
        // Resize main swapchain.
        this.GraphicsDevice.MainSwapchain.Resize((uint) rectangle.Width, (uint) rectangle.Height);
        
        // Resize render target.
        this._renderTarget.Resize((uint) rectangle.Width, (uint) rectangle.Height);
        this._renderResult.Dispose();
        this._renderResult = new Texture2D(this.GraphicsDevice, new Image(rectangle.Width, rectangle.Height), false);
        
        // Resize scene manager.
        SceneManager.OnResize(rectangle);

        // Resize overlay manager.
        OverlayManager.OnResize(rectangle);
        
        // Resize gui manager.
        GuiManager.OnResize(rectangle);
    }
    
    /// <summary>
    /// Handles the logic to be executed when the application shuts down.
    /// This method can be overridden by derived classes to include custom shutdown behavior.
    /// </summary>
    protected virtual void OnClose() { }
    
    /// <summary>
    /// Gets the target frames per second.
    /// </summary>
    public int GetTargetFps() {
        return (int) (1.0F / this._fixedFrameRate);
    }
    
    /// <summary>
    /// Sets the target frames per second.
    /// </summary>
    public void SetTargetFps(int fps) {
        this._fixedFrameRate = 1.0F / fps;
    }
    
    /// <summary>
    /// Retrieves the texture sample count currently used by the game's MSAA render target texture.
    /// </summary>
    /// <returns>The sample count of the MSAA render target texture.</returns>
    public TextureSampleCount? GetSampleCount() {
        return this._renderTarget.SampleCount;
    }
    
    /// <summary>
    /// Sets the sample count for multi-sample anti-aliasing (MSAA).
    /// </summary>
    /// <param name="sampleCount">The texture sample count to apply, defining the level of anti-aliasing.</param>
    public void SetSampleCount(TextureSampleCount sampleCount) {
        this._renderTarget.SampleCount = sampleCount;
        SceneManager.FilterTarget.SampleCount = sampleCount;
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            SceneManager.Destroy();
            OverlayManager.Destroy();
            GuiManager.Destroy();
            RegistryManager.Destroy();
            
            this.Content.Dispose();
            
            this._renderTarget.Dispose();
            this._renderResult.Dispose();
            this.FullScreenRenderPass.Dispose();
            
            this.GlobalImmediateRenderer.Dispose();
            this.GlobalPrimitiveBatch.Dispose();
            this.GlobalSpriteBatch.Dispose();
            
            this.CommandList.Dispose();
            
            GlobalGraphicsAssets.Destroy();
            GlobalResource.Destroy();
            
            AudioContext.Deinitialize();
            
            this.GraphicsDevice.Dispose();
            this.MainWindow.Dispose();

            JLogger.Listener -= this._logJitter.Log;
            Logger.Message -= this._logFileWriter.WriteFileMsg;
        }
    }
}