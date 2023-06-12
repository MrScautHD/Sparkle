using System.Reflection;
using Raylib_cs;
using Sparkle.csharp.window;

namespace Sparkle.csharp; 

public class Application : IDisposable {
    
    public static Application Instance;
    public static readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version!;

    private readonly ApplicationSettings _settings;

    public Window Window { get; private set; }
    
    public bool Headless { get; private set; }

    private readonly double _delay = 1.0 / 60.0;
    private double _timer;

    public Application(ApplicationSettings settings) {
        Instance = this;
        this._settings = settings;
        this.Headless = settings.Headless;
    }
    
    public void SetTargetFps(int fps) => Raylib.SetTargetFPS(fps);

    public int GetFps() => Raylib.GetFPS();
    
    public void Run() {
        Logger.Info($"Hello World! Sparkle [{Version}] Start...");
        Logger.Info("\tCPU: " + SystemInfo.Cpu);
        Logger.Info("\tMEMORY USE: " + SystemInfo.MemoryInUse);
        Logger.Info("\tTHREADS: " + SystemInfo.Threads);
        Logger.Info("\tOS: " + SystemInfo.Os);

        if (!this.Headless) {
            Logger.Debug("Initialize Window...");
            this.Window = new Window(this._settings.Size, this._settings.Title);
            this.Window.SetWindowStates(this._settings.WindowStates);
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
                this.Draw();
                //Raylib.PollInputEvents(); // TODO Check what it does
            }
        }
    }

    protected virtual void Init() {
        
    }

    protected virtual void Draw() {
        
    }

    protected virtual void Update() {
        
    }

    protected virtual void FixedUpdate() {
        
    }

    public void Close() {
        Logger.Warn("Application shuts down!");
        Raylib.CloseWindow();
    }

    public void Dispose() {
        
    }
}