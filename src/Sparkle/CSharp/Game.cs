using Bliss.CSharp;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Images;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Contexts;
using Bliss.CSharp.Logging;
using Bliss.CSharp.Transformations;
using Bliss.CSharp.Windowing;
using MiniAudioEx;
using Sparkle.CSharp.Logging;
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
    /// The content manager used to load game assets.
    /// </summary>
    //public ContentManager Content { get; private set; }
    
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
    /// Initializes the game with specified settings.
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
    public void Run(/*Scene? scene*/) {
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
        
        Logger.Info("Initialize time...");
        Time.Init();
        
        Logger.Info($"Set target FPS to: {this.Settings.TargetFps}");
        this.SetTargetFps(this.Settings.TargetFps);
        
        Logger.Info("Initialize commandlist...");
        this.CommandList = graphicsDevice.ResourceFactory.CreateCommandList();
        
        Logger.Info("Initialize global resources...");
        GlobalResource.Init(graphicsDevice);
        
        Logger.Info("Initialize input...");
        if (this.MainWindow is Sdl3Window) {
            Input.Init(new Sdl3InputContext(this.MainWindow));
        }
        else {
            Logger.Fatal("This type of window is not supported by the InputContext!");
        }
        
        Logger.Info("Initialize audio device...");
        AudioContext.Initialize(44100, 2);
        
        //Logger.Info("Initialize content manager...");
        //this.Content = new ContentManager();
        
        this.OnRun();
        
        Logger.Info("Load content...");
        this.Load();

        //Logger.Info("Set default scene...");
        //SceneManager.SetDefaultScene(scene);
        
        this.Init();

        Logger.Info("Start main loops...");
        while (!this.ShouldClose && this.MainWindow.Exists) {
            if (this.GetTargetFps() != 0 && Time.Timer.Elapsed.TotalSeconds <= this._fixedFrameRate) {
                continue;
            }
            Time.Update();
            
            this.MainWindow.PumpEvents();
            Input.Begin();
            
            AudioContext.Update();
            this.Update();
            this.AfterUpdate();

            this._fixedUpdateTimer += Time.Delta;
            while (this._fixedUpdateTimer >= this._fixedUpdateTimeStep) {
                this.FixedUpdate();
                this._fixedUpdateTimer -= this._fixedUpdateTimeStep;
            }
            
            this.Draw(graphicsDevice, this.CommandList);
            Input.End();
        }
        
        Logger.Warn("Application shuts down!");
        this.OnClose();
    }
    
    /// <summary>
    /// Virtual method for additional setup when the game starts.
    /// </summary>
    protected virtual void OnRun() {
    }

    /// <summary>
    /// Loads the required game content and resources.
    /// </summary>
    protected virtual void Load() {
    }
    
    /// <summary>
    /// Initializes global game resources.
    /// </summary>
    protected virtual void Init() {
    }

    /// <summary>
    /// Updates the game state, including scene and UI management.
    /// </summary>
    protected virtual void Update() {
    }
    
    /// <summary>
    /// Final update after regular updates are completed.
    /// </summary>
    protected virtual void AfterUpdate() {
    }
    
    /// <summary>
    /// Performs fixed update actions, usually for physics or time-based events.
    /// </summary>
    protected virtual void FixedUpdate() {
    }
    
    /// <summary>
    /// Renders the game scene to the screen.
    /// </summary>
    protected virtual void Draw(GraphicsDevice graphicsDevice, CommandList commandList) { // TODO: Do a Framebuffer for MSAA (Here) and one for Post-Processing (Scene)
        commandList.Begin();
        commandList.SetFramebuffer(graphicsDevice.SwapchainFramebuffer);
        commandList.ClearColorTarget(0, Color.DarkGray.ToRgbaFloat());
        
        commandList.End();
        graphicsDevice.SubmitCommands(commandList);
        graphicsDevice.SwapBuffers();
    }

    /// <summary>
    /// Handles window resizing events.
    /// </summary>
    protected virtual void OnResize(Rectangle rectangle) {
        this.GraphicsDevice.MainSwapchain.Resize((uint) rectangle.Width, (uint) rectangle.Height);
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
            AudioContext.Deinitialize();
            this.CommandList.Dispose();
            this.GraphicsDevice.Dispose();
            this.MainWindow.Dispose();
            Logger.Message -= this._logFileWriter.WriteFileMsg;
        }
    }
}