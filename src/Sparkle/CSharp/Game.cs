using Bliss.CSharp;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Geometry;
using Bliss.CSharp.Graphics.Rendering.Batches.Primitives;
using Bliss.CSharp.Graphics.Rendering.Batches.Sprites;
using Bliss.CSharp.Graphics.Rendering.Passes;
using Bliss.CSharp.Graphics.Rendering.Renderers;
using Bliss.CSharp.Images;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Contexts;
using Bliss.CSharp.Logging;
using Bliss.CSharp.Materials;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
using Bliss.CSharp.Windowing;
using MiniAudioEx;
using Sparkle.CSharp.Content;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.Logging;
using Sparkle.CSharp.Overlays;
using Sparkle.CSharp.Registries;
using Sparkle.CSharp.Scenes;
using Veldrid;

namespace Sparkle.CSharp;

public class Game : Disposable {
    
    /// <summary>
    /// The version of the game engine (Sparkle).
    /// </summary>
    public static readonly Version Version = new Version(5, 0, 0);

    /// <summary>
    /// The singleton instance of the game.
    /// </summary>
    public static Game Instance { get; private set; }
    
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
    /// The render pass used for draw the Multi-Sampled RenderTexture. 
    /// </summary>
    public FullScreenRenderPass MsaaRenderPass { get; private set; }

    /// <summary>
    /// The MSAA RenderTexture used for handling Anti-Aliasing (MSAA).
    /// </summary>
    public RenderTexture2D MsaaRenderTexture { get; private set; }

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
    /// The log file writer used for logging messages to a file.
    /// </summary>
    private LogFileWriter _logFileWriter;
    
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
    /// Starts the game loop, initializing all necessary components and running the game.
    /// </summary>
    /// <param name="scene">The scene to load initially.</param>
    public void Run(Scene? scene) {
        if (this.Settings.LogDirectory != string.Empty) {
            this._logFileWriter = new LogFileWriter(this.Settings.LogDirectory);
            Logger.Message += this._logFileWriter.WriteFileMsg;
        }

        Logger.Info($"Hello World! Sparkle [{Version}] start...");
        Logger.Info($"\t> CPU: {SystemInfo.Cpu}");
        Logger.Info($"\t> MEMORY: {SystemInfo.MemorySize} GB");
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
        
        this.MainWindow = Window.CreateWindow(WindowType.Sdl3, this.Settings.Width, this.Settings.Height, this.Settings.Title, this.Settings.WindowFlags, options, this.Settings.Backend, out GraphicsDevice graphicsDevice);
        this.MainWindow.Resized += () => this.OnResize(new Rectangle(this.MainWindow.GetX(), this.MainWindow.GetY(), this.MainWindow.GetWidth(), this.MainWindow.GetHeight()));
        this.GraphicsDevice = graphicsDevice;
        
        Logger.Info("Loading window icon...");
        this.MainWindow.SetIcon(this.Settings.IconPath != string.Empty ? new Image(this.Settings.IconPath) : new Image("content/images/icon.png"));
        
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
        
        Logger.Info("Initialize MSAA render pass...");
        this.MsaaRenderPass = new FullScreenRenderPass(graphicsDevice, graphicsDevice.SwapchainFramebuffer.OutputDescription);
        
        Logger.Info("Initialize MSAA render texture...");
        this.MsaaRenderTexture = new RenderTexture2D(graphicsDevice, (uint) this.MainWindow.GetWidth(), (uint) this.MainWindow.GetHeight(), this.Settings.SampleCount);
        
        Logger.Info("Initialize global sprite batch...");
        this.GlobalSpriteBatch = new SpriteBatch(graphicsDevice, this.MainWindow, this.MsaaRenderTexture.Framebuffer.OutputDescription);
        
        Logger.Info("Initialize global primitive batch...");
        this.GlobalPrimitiveBatch = new PrimitiveBatch(graphicsDevice, this.MainWindow, this.MsaaRenderTexture.Framebuffer.OutputDescription);
        
        Logger.Info("Initialize global immediate renderer...");
        this.GlobalImmediateRenderer = new ImmediateRenderer(graphicsDevice, this.MsaaRenderTexture.Framebuffer.OutputDescription);
        
        Logger.Info("Initialize graphics context...");
        this.GraphicsContext = new GraphicsContext(graphicsDevice, this.CommandList, this.GlobalSpriteBatch, this.GlobalPrimitiveBatch, this.GlobalImmediateRenderer, this.MsaaRenderTexture.Framebuffer.OutputDescription);
        
        Logger.Info("Initialize content manager...");
        this.Content = new ContentManager(graphicsDevice);
        
        Logger.Info("Initialize overlay manager...");
        OverlayManager.Init();
        
        Logger.Info("Initialize registry manager...");
        RegistryManager.Init();
        
        Logger.Info("Initialize scene manager...");
        SceneManager.Init(scene);
        
        this.OnRun();
        
        Logger.Info("Load content...");
        this.Load(this.Content);
        
        this.Init();

        Logger.Info("Start game loop...");
        while (!this.ShouldClose && this.MainWindow.Exists) {
            if (this.GetTargetFps() != 0 && Time.Timer.Elapsed.TotalSeconds <= this._fixedFrameRate) {
                continue;
            }
            Time.Update();
            
            this.MainWindow.PumpEvents();
            Input.Begin();
            
            AudioContext.Update();
            this.Update(Time.Delta);
            this.AfterUpdate(Time.Delta);

            this._fixedUpdateTimer += Time.Delta;
            while (this._fixedUpdateTimer >= this._fixedUpdateTimeStep) {
                this.FixedUpdate(1.0F / this._fixedUpdateTimeStep);
                this._fixedUpdateTimer -= this._fixedUpdateTimeStep;
            }
            
            this.CommandList.Begin();
            this.CommandList.SetFramebuffer(this.MsaaRenderTexture.Framebuffer);
            this.CommandList.ClearColorTarget(0, Color.DarkGray.ToRgbaFloat());
        
            this.Draw(this.GraphicsContext);

            this.CommandList.End();
            graphicsDevice.SubmitCommands(this.CommandList);
            
            this.CommandList.Begin();

            if (this.MsaaRenderTexture.SampleCount != TextureSampleCount.Count1) {
                this.CommandList.ResolveTexture(this.MsaaRenderTexture.ColorTexture, this.MsaaRenderTexture.DestinationTexture);
            }
            
            this.CommandList.SetFramebuffer(graphicsDevice.SwapchainFramebuffer);
            this.CommandList.ClearColorTarget(0, Color.DarkGray.ToRgbaFloat());
            
            this.MsaaRenderPass.Draw(this.CommandList, this.MsaaRenderTexture);
            
            this.CommandList.End();
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
    /// Loads the required game content and resources.
    /// </summary>
    protected virtual void Load(ContentManager content) {
        RegistryManager.OnLoad(content);
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
    protected virtual void Update(double delta) {
        SceneManager.OnUpdate(delta);
        OverlayManager.OnUpdate(delta);
    }
    
    /// <summary>
    /// Final update after regular updates are completed.
    /// </summary>
    protected virtual void AfterUpdate(double delta) {
        SceneManager.OnAfterUpdate(delta);
        OverlayManager.OnAfterUpdate(delta);
    }
    
    /// <summary>
    /// Performs fixed update actions, usually for physics or time-based events.
    /// </summary>
    protected virtual void FixedUpdate(double timeStep) {
        SceneManager.OnFixedUpdate(timeStep);
        OverlayManager.OnFixedUpdate(timeStep);
    }
    
    /// <summary>
    /// Renders the game scene to the screen.
    /// </summary>
    protected virtual void Draw(GraphicsContext context) {
        SceneManager.OnDraw(context);
        OverlayManager.OnDraw(context);
    }

    /// <summary>
    /// Handles window resizing events.
    /// </summary>
    protected virtual void OnResize(Rectangle rectangle) {
        this.GraphicsDevice.MainSwapchain.Resize((uint) rectangle.Width, (uint) rectangle.Height);
        this.MsaaRenderTexture.Resize((uint) rectangle.Width, (uint) rectangle.Height);
        SceneManager.OnResize(rectangle);
        OverlayManager.OnResize(rectangle);
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

    protected override void Dispose(bool disposing) {
        if (disposing) {
            OverlayManager.Destroy();
            RegistryManager.Destroy();
            
            this.Content.Dispose();
            
            this.MsaaRenderTexture.Dispose();
            this.MsaaRenderPass.Dispose();
            
            this.GlobalImmediateRenderer.Dispose();
            this.GlobalPrimitiveBatch.Dispose();
            this.GlobalSpriteBatch.Dispose();
            
            this.CommandList.Dispose();
            
            AudioContext.Deinitialize();
            
            this.GraphicsDevice.Dispose();
            this.MainWindow.Dispose();

            Logger.Message -= this._logFileWriter.WriteFileMsg;
        }
    }
}