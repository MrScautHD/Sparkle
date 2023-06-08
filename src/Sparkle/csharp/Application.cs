using System.Reflection;
using Silk.NET.Input;
using Silk.NET.Windowing;

namespace Sparkle.csharp; 

public class Application : IDisposable {
    
    public static Application Instance;
    public static readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version!;
    
    private ApplicationSettings _settings;
    public IWindow IWindow;
    public IInputContext InputContext;

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

        this.IWindow.Load += Load;
        this.IWindow.Update += Tick;
        this.IWindow.Render += Draw;
        
        Logger.Debug("Run Window!");
        this.IWindow.Run();
    }
    
    private void Load() {
        Logger.Info("Starting Initializing!");
        
        Logger.Debug("Initializing Input...");
        this.InputContext = this.IWindow.CreateInput();
        Input.Init();

        Logger.Debug("Initializing Time...");
        Time.Init();
        
        this.Init();
    }
    
    private void Tick(double dt) {
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

        this.IWindow = Window.Create(options);
    }

    public void Close() {
        Logger.Warn("Application shuts down!");
        this.IWindow.Close();
    }

    public void Dispose() {
        this.IWindow.Dispose();
    }
}