using System.Reflection;
using Silk.NET.Input;
using Silk.NET.Windowing;

namespace Sparkle.csharp; 

public class Application : IDisposable {
    
    public static Application Instance;
    public static readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version!;

    private readonly ApplicationSettings _settings;
    public IWindow Win { get; private set; }
    public IInputContext InputContext { get; private set; }

    private readonly double _delay = 1.0 / 60.0;
    private double _timer;

    public Application(ApplicationSettings settings) {
        Instance = this;
        this._settings = settings;
    }
    
    public void Run() {
        Logger.Info($"Hello World! Sparkle [{Version}] start...");
        Logger.Info("\tCPU: " + SystemInfo.Cpu);
        Logger.Info("\tMEMORY: " + SystemInfo.Memory);
        Logger.Info("\tTHREADS: " + SystemInfo.Threads);
        Logger.Info("\tOS: " + SystemInfo.Os);
        
        Logger.Debug("Creating Window...");
        this.CreateWindow();

        this.Win.Load += OnInit;
        this.Win.Update += OnUpdate;
        this.Win.Render += Draw;
        
        Logger.Debug("Run Window!");
        this.Win.Run();
    }
    
    private void OnInit() {
        if (this.Win.VkSurface == null) {
            throw new Exception("Windowing platform is not compatible with Vulkan.");
        }
        
        Logger.Info("Starting Initializing!");

        Logger.Debug("Initializing Input...");
        this.InputContext = this.Win.CreateInput();
        Input.Init();

        Logger.Debug("Initializing Time...");
        Time.Init();
        
        this.Init();
    }
    
    private void OnUpdate(double dt) {
        this.Update(dt);
        
        this._timer += dt;
        while (this._timer >= this._delay) {
            this.FixedUpdate();
            this._timer -= this._delay;
        }

        Input.Update();
    }

    protected virtual void Init() {

    }

    protected virtual void Draw(double dt) {
        
    }

    protected virtual void Update(double dt) {
        
    }

    protected virtual void FixedUpdate() {
        
    }

    private void CreateWindow() {
        WindowOptions options = WindowOptions.DefaultVulkan with {
            Title = this._settings.Title,
            Size = this._settings.Size,
            WindowState = this._settings.WindowState,
            WindowBorder = this._settings.WindowBorder,
            UpdatesPerSecond = this._settings.TargetFps,
            VSync = this._settings.VSync,
            IsVisible = this._settings.IsVisible
        };

        this.Win = Window.Create(options);
    }

    public void Close() {
        Logger.Warn("Application shuts down!");
        this.Win.Close();
    }

    public void Dispose() {
        this.Win.Dispose();
        this.InputContext.Dispose();
    }
}