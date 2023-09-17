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
    
    private readonly double _delay;
    private double _timer;
    
    public readonly GameSettings Settings;
    public bool ShouldClose;
    
    public ContentManager Content { get; private set; }
    
    public Image Logo { get; private set; }
    
    public bool HasInitialized { get; private set; }
    public bool HasDisposed { get; private set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Game"/>, setting the static Instance to this object, initializing game settings, and calculating the delay based on the FixedTimeStep.
    /// </summary>
    /// <param name="settings">The game settings to be used for this Game instance.</param>
    public Game(GameSettings settings) {
        Instance = this;
        this.Settings = settings;
        this._delay = 1.0 / settings.FixedTimeStep;
    }
    
    /// <summary>
    /// Starts the <see cref="Game"/>.
    /// </summary>
    /// <param name="scene">The initial <see cref="Scene"/> to start with.</param>
    public void Run(Scene? scene) {
        this.ThrowIfDisposed();
        
        if (this.Settings.LogDirectory != string.Empty) {
            Logger.CreateLogFile(this.Settings.LogDirectory);
        }

        Logger.Info($"Hello World! Sparkle [{Version}] start...");
        Logger.Info($"\tCPU: {SystemInfo.Cpu}");
        Logger.Info($"\tMEMORY: {SystemInfo.MemorySize} GB");
        Logger.Info($"\tTHREADS: {SystemInfo.Threads}");
        Logger.Info($"\tOS: {SystemInfo.Os}");
        
        Logger.Debug("Initialize Raylib logger...");
        Logger.SetupRayLibLogger();
        
        Logger.Debug($"Setting target fps to: {(this.Settings.TargetFps > 0 ? this.Settings.TargetFps : "unlimited")}");
        this.SetTargetFps(this.Settings.TargetFps);

        Logger.Debug("Initialize content manager...");
        this.Content = new ContentManager(this.Settings.ContentDirectory);
        
        Logger.Debug("Initialize audio device...");
        AudioDevice.Init();

        Logger.Debug("Initialize window...");
        Window.SetConfigFlags(this.Settings.WindowFlags);
        Window.Init(this.Settings.WindowWidth, this.Settings.WindowHeight, this.Settings.Title);
            
        this.Logo = this.Settings.IconPath == string.Empty ? ImageHelper.Load("content/icon.png") : this.Content.Load<Image>(this.Settings.IconPath);
        Window.SetIcon(this.Logo);
        
        Logger.Debug("Initialize default scene...");
        SceneManager.SetDefaultScene(scene!);
        
        this.Init();
        this.HasInitialized = true;
        
        Logger.Debug("Run ticks...");
        while (!this.ShouldClose && !Window.ShouldClose()) {
            this.Update();
            
            this._timer += Time.Delta;
            while (this._timer >= this._delay) {
                this.FixedUpdate();
                this._timer -= this._delay;
            }
            
            Graphics.BeginDrawing();
            Graphics.ClearBackground(Color.SKYBLUE);
            this.Draw();
            Graphics.EndDrawing();
        }
        
        this.OnClose();
    }
    
    /// <summary>
    /// Used for Initializes objects.
    /// </summary>
    protected virtual void Init() {
        SceneManager.Init();
        OverlayManager.Init();
    }
    
    /// <summary>
    /// Is invoked during each tick and is used for updating dynamic elements and game logic.
    /// </summary>
    protected virtual void Update() {
        SceneManager.Update();
        GuiManager.Update();
        OverlayManager.Update();
    }

    /// <summary>
    /// Is invoked at a fixed rate of every <see cref="GameSettings.FixedTimeStep"/> frames following the <see cref="Update"/> method.
    /// It is used for handling physics and other fixed-time operations.
    /// </summary>
    protected virtual void FixedUpdate() {
        SceneManager.FixedUpdate();
        GuiManager.FixedUpdate();
        OverlayManager.FixedUpdate();
    }
    
    /// <summary>
    /// Is called every tick, used for rendering stuff.
    /// </summary>
    protected virtual void Draw() {
        SceneManager.Draw();
        GuiManager.Draw();
        OverlayManager.Draw();
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
        this.ThrowIfDisposed();
        return Raylib.GetFPS();
    }

    /// <summary>
    /// Sets the target frames per second (FPS) for the application.
    /// </summary>
    /// <param name="fps">The desired target frames per second (FPS) value.</param>
    public void SetTargetFps(int fps) {
        this.ThrowIfDisposed();
        if (fps > 0) {
            Raylib.SetTargetFPS(fps);
        }
    }

    /// <inheritdoc cref="Raylib.OpenURL(string)"/>
    public void OpenUrl(string url) {
        this.ThrowIfDisposed();
        Raylib.OpenURL(url);
    }

    public void Dispose() {
        if (this.HasDisposed) return;
        
        this.Dispose(true);
        GC.SuppressFinalize(this);
        this.HasDisposed = true;
    }

    protected virtual void Dispose(bool disposing) {
        if (disposing) {
            if (this.Settings.IconPath == string.Empty) {
                ImageHelper.Unload(this.Logo);
            }

            for (int i = 0; i < OverlayManager.Overlays.Count; i++) {
                OverlayManager.Overlays[i].Dispose();
            }

            this.Content.Dispose();
            Window.Close();
            AudioDevice.Close();
            GuiManager.ActiveGui?.Dispose();
            SceneManager.ActiveScene?.Dispose();
        }
    }
    
    public void ThrowIfDisposed() {
        if (this.HasDisposed) {
            throw new ObjectDisposedException(this.GetType().Name);
        }
    }
}