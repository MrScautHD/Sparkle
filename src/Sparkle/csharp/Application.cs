using System.Reflection;
using Silk.NET.Vulkan;
using Silk.NET.Windowing;

namespace Sparkle.csharp; 

public class Application : IDisposable {
    
    public static Application Instance;
    public static readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version!;

    private ApplicationSettings _settings;
    private bool _close;

    private IWindow _window;
    private Vk _vk;
    
    private PhysicalDevice _physicalDevice;
    private Device _device;
    
    private readonly double _delay = 1.0 / 60.0;
    private double _timer;
    
    public Application(ApplicationSettings settings) {
        Instance = this;
        this._settings = settings;
    }
    
    public void Run() {
        Logger.Debug("Hello World! Sparkle start...");
        Logger.Info("\tCPU: " + SystemInfo.Cpu);
        Logger.Info("\tMEMORY: " + SystemInfo.Memory);
        Logger.Info("\tTHREADS: " + SystemInfo.Threads);
        Logger.Info("\tOS: " + SystemInfo.Os);
        
        Logger.Info("Start Initializing!");
        this._window.Run();
        this.Init();
        
        while (!this._close) {

            while (this._timer >= this._delay) {
                this.FixedUpdate();
                this._timer -= this._delay;
            }
            
            this.Update();
            this.Draw();
        }
    }

    public virtual void Init() {
        Logger.Debug("Initializing Time...");
        Time.Init();
        
        Logger.Debug("Initializing Window...");
        this.CreateWindow();
    }

    public virtual void Draw() {
        
    }

    public virtual void Update() {
        Time.Update();
    }

    public virtual void FixedUpdate() {
        
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

        this._window = Window.Create(options);
        this._window.Initialize();

        if (this._window.VkSurface == null) {
            Logger.Fatal("Windowing platform doesn't support Vulkan.");
        }
    }

    public void Close() {
        Logger.Warn("Application shuts down!");
        this._close = true;
    }

    public void Dispose() {
        _window.Dispose();
        _vk.Dispose();
    }
}