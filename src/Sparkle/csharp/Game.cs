using System.Reflection;
using Raylib_cs;
using Sparkle.csharp.content;
using Sparkle.csharp.scene;
using Sparkle.csharp.window;

namespace Sparkle.csharp; 

public class Game : IDisposable {
    
    public static Game Instance;
    public static readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version!;

    private readonly GameSettings _settings;
    
    private readonly double _delay = 1.0 / 60.0;
    private double _timer;

    public Window Window { get; private set; }
    
    public ContentManager Content { get; private set; }
    
    public bool Headless { get; private set; }

    public Game(GameSettings settings, Scene scene) {
        Instance = this;
        this._settings = settings;
        this.Headless = settings.Headless;
        
        SceneManager.SetScene(scene);
    }

    public void Run() {
        Logger.Info($"Hello World! Sparkle [{Version}] Start...");
        Logger.Info("\tCPU: " + SystemInfo.Cpu);
        Logger.Info("\tVIRTUAL MEMORY: " + SystemInfo.VirtualMemorySize);
        Logger.Info("\tTHREADS: " + SystemInfo.Threads);
        Logger.Info("\tOS: " + SystemInfo.Os);
        
        Logger.Debug("Initialize RayLib Logger...");
        Logger.SetupRayLibLog();

        if (!this.Headless) {
            Logger.Debug("Initialize Window...");
            this.Window = new Window(this._settings.Size, this._settings.Title);
            
            Logger.Debug("Initialize Content Manager...");
            this.Content = new ContentManager(this._settings.ContentDirectory);
            
            Logger.Debug("Initialize Settings...");
            this.Window.SetIcon(this.Content.Load<Image>(this._settings.IconPath));
            this.Window.SetStates(this._settings.WindowStates);
            this.SetTargetFps(this._settings.TargetFps);
        }

        this.Init();

        Logger.Debug("Run Ticks...");
        while (!Raylib.WindowShouldClose()) { // TODO Make it work with Headless to!
            this.Update();
            
            this._timer += Time.DeltaTime;
            while (this._timer >= this._delay) {
                this.FixedUpdate();
                this._timer -= this._delay;
            }

            if (!this.Headless) {
                Raylib.BeginDrawing();
                this.Draw();
                Raylib.EndDrawing();
            }
        }
        
        this.OnClose();
    }
    
    protected virtual void Init() {
        SceneManager.Init();
    }

    protected virtual void Update() {
        SceneManager.Update();
    }

    protected virtual void FixedUpdate() {
        SceneManager.FixedUpdate();
    }
    
    protected virtual void Draw() {
        SceneManager.Draw();
    }

    public void Close() {
        this.Window.Close();
    }

    public void OnClose() {
        Logger.Warn("Application shuts down!");
    }

    public int GetFps() {
        return Raylib.GetFPS();
    }

    public void SetTargetFps(int fps) {
        if (fps != 0) {
            Raylib.SetTargetFPS(fps);
        }
    }

    public virtual void Dispose() {
        if (!this.Headless) {
            this.Content.Dispose();
        }
        
        SceneManager.ActiveScene?.Dispose();
    }
}