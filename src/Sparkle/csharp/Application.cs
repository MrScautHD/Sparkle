using System.Reflection;
using Silk.NET.Input;
using Silk.NET.Windowing;

namespace Sparkle.csharp; 

public class Application : IDisposable {
    
    public static Application Instance;
    public static readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version!;
    
    private ApplicationSettings _settings;
    public IWindow IWindow;

    private readonly double _delay = 1.0 / 60.0;
    private double _timer;

    public Application(ApplicationSettings settings) {
        Instance = this;
        this._settings = settings;
    }
    
    public void Run() {
        Logger.Info("Hello World! Sparkle start...");
        Logger.Info("\tCPU: " + SystemInfo.Cpu);
        Logger.Info("\tMEMORY: " + SystemInfo.Memory);
        Logger.Info("\tTHREADS: " + SystemInfo.Threads);
        Logger.Info("\tOS: " + SystemInfo.Os);
        
        Logger.Debug("Creating Window...");
        this.CreateWindow();

        this.IWindow.Load += Init;
        this.IWindow.Update += Update;
        this.IWindow.Render += Draw;
        
        Logger.Debug("Run Window!");
        this.IWindow.Run();
    }

    protected virtual void Init() {
        Logger.Info("Starting Initializing!");
        
        Logger.Debug("Initializing Input...");
        this.CreateInput();
        
        Logger.Debug("Initializing Time...");
        Time.Init();
    }

    protected virtual void Draw(double dt) {
       // Logger.Error("Draw");
    }

    protected virtual void Update(double dt) {
        /*while (this._timer >= this._delay) {
            this.FixedUpdate();
            this._timer -= this._delay;
        }*/
        
       // Logger.Error("Update");
    }

    protected virtual void FixedUpdate() {
       // Logger.Error("FixedUpdate");
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

    private void CreateInput() {
        IInputContext input = this.IWindow.CreateInput();
        
        for (int i = 0; i < input.Keyboards.Count; i++) {
            input.Keyboards[i].KeyDown += KeyDown;
        }
    }

    protected virtual void KeyDown(IKeyboard keyboard, Key key, int keyCode) {
        if (key == Key.Escape) {
            this.Close();
        }
    }

    public void Close() {
        Logger.Warn("Application shuts down!");
        this.IWindow.Close();
    }

    public void Dispose() {
        this.IWindow.Dispose();
        //_vk.Dispose();
    }
}