using System.Reflection;
using Raylib_cs;
using Sparkle.csharp.audio;
using Sparkle.csharp.content;
using Sparkle.csharp.graphics;
using Sparkle.csharp.graphics.util;
using Sparkle.csharp.gui;
using Sparkle.csharp.overlay;
using Sparkle.csharp.scene;
using Sparkle.csharp.window;

namespace Sparkle.csharp; 

public class Game : IDisposable {
    
    public static Game Instance { get; private set; }
    public static readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version!;

    private readonly GameSettings _settings;
    
    private readonly double _delay = 1.0 / 60.0;
    private double _timer;
    
    public bool ShouldClose;
    
    public Image Logo { get; private set; }

    public Window Window { get; private set; }
    public Graphics Graphics { get; private set; }
    public ContentManager Content { get; private set; }
    public AudioDevice AudioDevice { get; private set; }
    
    public bool Headless { get; private set; }

    public Game(GameSettings settings) {
        Instance = this;
        this._settings = settings;
        this.Headless = settings.Headless;
    }

    public void Run(Scene? scene) {
        if (this._settings.LogDirectory != string.Empty) {
            Logger.CreateLogFile(this._settings.LogDirectory);
        }

        Logger.Info($"Hello World! Sparkle [{Version}] start...");
        Logger.Info($"\tCPU: {SystemInfo.Cpu}");
        Logger.Info($"\tMEMORY: {SystemInfo.MemorySize} GB");
        Logger.Info($"\tTHREADS: {SystemInfo.Threads}");
        Logger.Info($"\tOS: {SystemInfo.Os}");
        
        Logger.Debug("Initialize RayLib logger...");
        Logger.SetupRayLibLogger();
        
        Logger.Debug($"Setting target fps to: {this._settings.TargetFps}");
        this.SetTargetFps(this._settings.TargetFps);

        if (!this.Headless) {
            Logger.Debug("Initialize content manager...");
            this.Content = new ContentManager(this._settings.ContentDirectory);
            
            Logger.Debug("Initialize graphics...");
            this.Graphics = new Graphics();
            
            Logger.Debug("Initialize audio device...");
            this.AudioDevice = new AudioDevice();
            this.AudioDevice.Init();

            Logger.Debug("Initialize window...");
            this.Window = new Window(this._settings.Size, this._settings.Title);
            this.Window.SetConfigFlag(this._settings.ConfigFlag);
            this.Window.Init();
            
            this.Logo = this._settings.IconPath == string.Empty ? ImageHelper.Load("content/icon.png") : this.Content.Load<Image>(this._settings.IconPath);
            this.Window.SetIcon(this.Logo);
        }

        Logger.Debug("Initialize default scene...");
        SceneManager.SetDefaultScene(scene!);

        this.Init();
        
        Logger.Debug("Run ticks...");
        while (!this.ShouldClose && !this.Window.ShouldClose()) {
            this.Update();
            
            this._timer += Time.Delta;
            while (this._timer >= this._delay) {
                this.FixedUpdate();
                this._timer -= this._delay;
            }

            if (!this.Headless) {
                this.Graphics.BeginDrawing();
                this.Graphics.ClearBackground(Color.SKYBLUE);
                this.Draw();
                this.Graphics.EndDrawing();
            }
        }
        
        this.OnClose();
    }
    
    protected virtual void Init() {
        SceneManager.Init();

        foreach (Overlay overlay in Overlay.Overlays) {
            if (overlay.Enabled) {
                overlay.Init();
            }
        }
    }

    protected virtual void Update() {
        SceneManager.Update();
        GuiManager.Update();
        
        foreach (Overlay overlay in Overlay.Overlays) {
            if (overlay.Enabled) {
                overlay.Update();
            }
        }
    }

    protected virtual void FixedUpdate() {
        SceneManager.FixedUpdate();
        GuiManager.FixedUpdate();
        
        foreach (Overlay overlay in Overlay.Overlays) {
            if (overlay.Enabled) {
                overlay.FixedUpdate();
            }
        }
    }
    
    protected virtual void Draw() {
        SceneManager.Draw();
        GuiManager.Draw();
        
        foreach (Overlay overlay in Overlay.Overlays) {
            if (overlay.Enabled) {
                overlay.Draw();
            }
        }
    }
    
    protected virtual void OnClose() {
        Logger.Warn("Application shuts down!");
    }

    public int GetFps() {
        return Raylib.GetFPS();
    }

    public void SetTargetFps(int fps) {
        if (fps > 0) {
            Raylib.SetTargetFPS(fps);
        }
    }

    public void OpenURL(string url) {
        if (!this.Headless) {
            Raylib.OpenURL(url);
        }
    }

    public virtual void Dispose() {
        if (!this.Headless) {
            if (this._settings.IconPath == string.Empty) {
                ImageHelper.Unload(this.Logo);
            }
            
            this.Content.Dispose();
            this.Window.Close();
            this.AudioDevice.Close();
            GuiManager.ActiveGui?.Dispose();
        }
        
        SceneManager.ActiveScene?.Dispose();
    }
}