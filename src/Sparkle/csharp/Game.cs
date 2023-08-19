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
    
    private readonly double _delay = 1.0 / 60.0;
    private double _timer;
    
    public readonly GameSettings Settings;
    public bool ShouldClose;
    
    public Image Logo { get; private set; }

    public Window Window { get; private set; }
    public Graphics Graphics { get; private set; }
    public ContentManager Content { get; private set; }
    public AudioDevice AudioDevice { get; private set; }
    
    public bool Headless { get; private set; }

    public Game(GameSettings settings) {
        Instance = this;
        this.Settings = settings;
        this.Headless = settings.Headless;
    }

    /// <summary>
    /// Starts the <see cref="Game"/>.
    /// </summary>
    /// <param name="scene">The initial <see cref="Scene"/> to start with.</param>
    public void Run(Scene? scene) {
        if (this.Settings.LogDirectory != string.Empty) {
            Logger.CreateLogFile(this.Settings.LogDirectory);
        }

        Logger.Info($"Hello World! Sparkle [{Version}] start...");
        Logger.Info($"\tCPU: {SystemInfo.Cpu}");
        Logger.Info($"\tMEMORY: {SystemInfo.MemorySize} GB");
        Logger.Info($"\tTHREADS: {SystemInfo.Threads}");
        Logger.Info($"\tOS: {SystemInfo.Os}");
        
        Logger.Debug("Initialize RayLib logger...");
        Logger.SetupRayLibLogger();
        
        Logger.Debug($"Setting target fps to: {this.Settings.TargetFps}");
        this.SetTargetFps(this.Settings.TargetFps);

        if (!this.Headless) {
            Logger.Debug("Initialize content manager...");
            this.Content = new ContentManager(this.Settings.ContentDirectory);
            
            Logger.Debug("Initialize graphics...");
            this.Graphics = new Graphics();
            
            Logger.Debug("Initialize audio device...");
            this.AudioDevice = new AudioDevice();
            this.AudioDevice.Init();

            Logger.Debug("Initialize window...");
            this.Window = new Window(this.Settings.Size, this.Settings.Title);
            this.Window.SetConfigFlag(this.Settings.ConfigFlag);
            this.Window.Init();
            
            this.Logo = this.Settings.IconPath == string.Empty ? ImageHelper.Load("content/icon.png") : this.Content.Load<Image>(this.Settings.IconPath);
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
    
    /// <summary>
    /// Used for Initializes objects.
    /// </summary>
    protected virtual void Init() {
        SceneManager.Init();

        foreach (Overlay overlay in Overlay.Overlays) {
            if (overlay.Enabled) {
                overlay.Init();
            }
        }
    }

    /// <summary>
    /// Is invoked during each tick and is used for updating dynamic elements and game logic.
    /// </summary>
    protected virtual void Update() {
        SceneManager.Update();
        GuiManager.Update();
        
        foreach (Overlay overlay in Overlay.Overlays) {
            if (overlay.Enabled) {
                overlay.Update();
            }
        }
    }

    /// <summary>
    /// Is invoked at a fixed rate of every 60 frames following the <see cref="Update"/> method.
    /// It is used for handling physics and other fixed-time operations.
    /// </summary>
    protected virtual void FixedUpdate() {
        SceneManager.FixedUpdate();
        GuiManager.FixedUpdate();
        
        foreach (Overlay overlay in Overlay.Overlays) {
            if (overlay.Enabled) {
                overlay.FixedUpdate();
            }
        }
    }
    
    /// <summary>
    /// Is called every tick, used for rendering stuff.
    /// </summary>
    protected virtual void Draw() {
        SceneManager.Draw();
        GuiManager.Draw();
        
        foreach (Overlay overlay in Overlay.Overlays) {
            if (overlay.Enabled) {
                overlay.Draw();
            }
        }
    }
    
    /// <summary>
    /// Is called when the <see cref="Game"/> is shutting down.
    /// </summary>
    protected virtual void OnClose() {
        Logger.Warn("Application shuts down!");
    }

    /// <summary>
    /// Retrieves the frames per second (FPS) of the application.
    /// </summary>
    /// <returns>The current frames per second (FPS) value.</returns>
    public int GetFps() {
        return Raylib.GetFPS();
    }

    /// <summary>
    /// Sets the target frames per second (FPS) for the application.
    /// </summary>
    /// <param name="fps">The desired target frames per second (FPS) value.</param>
    public void SetTargetFps(int fps) {
        if (fps > 0) {
            Raylib.SetTargetFPS(fps);
        }
    }

    /// <summary>
    /// Opens a specified URL in the default web browser.
    /// </summary>
    /// <param name="url">The URL to be opened.</param>
    public void OpenUrl(string url) {
        if (!this.Headless) {
            Raylib.OpenURL(url);
        }
    }

    public virtual void Dispose() {
        if (!this.Headless) {
            if (this.Settings.IconPath == string.Empty) {
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