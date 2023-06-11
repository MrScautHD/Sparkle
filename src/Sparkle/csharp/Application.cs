using System.Reflection;
using Raylib_cs;

namespace Sparkle.csharp; 

public class Application : IDisposable {
    
    public static Application Instance;
    public static readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version!;

    private readonly ApplicationSettings _settings;

    private readonly double _delay = 1.0 / 60.0;
    private double _timer;

    public Application(ApplicationSettings settings) {
        Instance = this;
        this._settings = settings;
    }
    
    public void Run() {
        Logger.Info($"Hello World! Sparkle [{Version}] Start...");
        Logger.Info("\tCPU: " + SystemInfo.Cpu);
        Logger.Info("\tMEMORY USE: " + SystemInfo.MemoryInUse);
        Logger.Info("\tTHREADS: " + SystemInfo.Threads);
        Logger.Info("\tOS: " + SystemInfo.Os);

        Logger.Debug("Initialize Window...");
        Raylib.InitWindow(this._settings.Size.Width, this._settings.Size.Height, this._settings.Title);
        Raylib.SetTargetFPS(this._settings.TargetFps);
        //Raylib.SetWindowIcon(Raylib.LoadImage("src/logo"));
        
        foreach (ConfigFlags state in this._settings.WindowState) {
            Raylib.SetWindowState(state);
        }
        
        this.Init();

        Logger.Debug("Run Ticks...");
        while (!Raylib.WindowShouldClose()) {
            this.Draw();
            this.Update();
            
            this._timer += Time.DeltaTime;
            while (this._timer >= this._delay) {
                this.FixedUpdate();
                this._timer -= this._delay;
            }
            
            Raylib.PollInputEvents(); // TODO Check what it does
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