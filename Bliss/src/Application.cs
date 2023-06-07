using System.Reflection;
using Silk.NET.Vulkan;
using Silk.NET.Windowing;

namespace Bliss; 

public class Application : IDisposable {
    
    public static Application Instance;
    public static readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version!;

    private ApplicationSettings _settings;
    public bool Close;

    private IWindow _window;
    private Vk _vk;
    
    private PhysicalDevice _physicalDevice;
    private Device _device;
    
    private readonly double _delay = 1.0 / 60.0;
    private double _timer;
    
    public Application() {
        Instance = this;
    }
    
    public void Run() {
        this._window.Run();
        
        this.Init();

        while (!this.Close) {

            while (this._timer >= this._delay) {
                this.FixedUpdate();
                this._timer -= this._delay;
            }
            
            this.Update();
            this.Draw();
        }
    }

    public virtual void Init() {
        Time.Init();
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
            throw new Exception("Windowing platform doesn't support Vulkan.");
        }
    }

    public void Dispose() {
        _window.Dispose();
        _vk.Dispose();
    }
}