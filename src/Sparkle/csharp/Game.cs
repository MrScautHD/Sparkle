using System.Reflection;
using Raylib_cs;
using Sparkle.csharp.audio;
using Sparkle.csharp.content;
using Sparkle.csharp.content.type;
using Sparkle.csharp.graphics;
using Sparkle.csharp.graphics.helper;
using Sparkle.csharp.gui;
using Sparkle.csharp.overlay;
using Sparkle.csharp.physics;
using Sparkle.csharp.registry;
using Sparkle.csharp.registry.types;
using Sparkle.csharp.scene;
using Sparkle.csharp.window;
using Registry = Sparkle.csharp.registry.Registry;

namespace Sparkle.csharp; 

public class Game : Disposable {
    
    public static Game Instance { get; private set; }
    public static readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version!;
    
    private readonly double _fixedTimeStep;
    private double _timer;
    
    public readonly GameSettings Settings;
    public bool ShouldClose;
    
    public ContentManager Content { get; private set; }
    public Simulation Simulation { get; private set; }
    
    public Image Logo { get; private set; }
    
    public bool HasInitialized { get; private set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Game"/>, setting the static Instance to this object, initializing game settings, and calculating the delay based on the FixedTimeStep.
    /// </summary>
    /// <param name="settings">The game settings to be used for this Game instance.</param>
    public Game(GameSettings settings) {
        Instance = this;
        this.Settings = settings;
        this._fixedTimeStep = 1.0F / settings.FixedTimeStep;
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
        Logger.Info($"\tAPI: {Rlgl.GetVersion()}");
        
        Logger.Debug("Initialize Raylib logger...");
        Logger.SetupRaylibLogger();
        
        this.SetTargetFps(this.Settings.TargetFps);

        Logger.Debug("Initialize content manager...");
        this.Content = new ContentManager();
        
        Logger.Debug("Initialize audio device...");
        AudioDevice.Init();

        Logger.Debug("Initialize window...");
        Window.SetConfigFlags(this.Settings.WindowFlags);
        Window.Init(this.Settings.Width, this.Settings.Height, this.Settings.Title);
        
        this.Logo = this.Settings.IconPath == string.Empty ? this.Content.Load(new ImageContent("content/images/icon.png")) : this.Content.Load(new ImageContent(this.Settings.IconPath));
        Window.SetIcon(this.Logo);
        
        this.OnRun();
        
        Logger.Debug("Load content...");
        this.Load();
        
        Logger.Debug("Initialize physics...");
        this.Simulation = new Simulation(this.Settings.PhysicsSettings);
        
        Logger.Debug("Initialize default scene...");
        SceneManager.SetDefaultScene(scene!);
        
        this.Init();
        this.HasInitialized = true;
        
        Logger.Debug("Run ticks...");
        while (!this.ShouldClose && !Window.ShouldClose()) {
            this.Update();
            this.AfterUpdate();
            
            this._timer += Time.Delta;
            while (this._timer >= this._fixedTimeStep) {
                this.FixedUpdate();
                this._timer -= this._fixedTimeStep;
            }
            
            Graphics.BeginDrawing();
            Graphics.ClearBackground(Color.SKYBLUE);
            this.Draw();
            Graphics.EndDrawing();
        }
        
        this.OnClose();
    }

    /// <summary>
    /// This method is called when the game starts.
    /// </summary>
    protected virtual void OnRun() {
        RegistryManager.AddType(new ShaderRegistry());
    }
    
    /// <summary>
    /// Used for Initializes objects.
    /// </summary>
    protected virtual void Init() {
        RegistryManager.Init();
        SceneManager.Init();
        OverlayManager.Init();
    }

    /// <summary>
    /// Used for loading resources.
    /// </summary>
    protected virtual void Load() {
        RegistryManager.Load(this.Content);
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
    /// Called after the Update method on each tick to further update dynamic elements and game logic.
    /// </summary>
    protected virtual void AfterUpdate() {
        SceneManager.AfterUpdate();
        GuiManager.AfterUpdate();
        OverlayManager.AfterUpdate();
    }

    /// <summary>
    /// Is invoked at a fixed rate of every <see cref="GameSettings.FixedTimeStep"/> frames following the <see cref="AfterUpdate"/> method.
    /// It is used for handling physics and other fixed-time operations.
    /// </summary>
    protected virtual void FixedUpdate() {
        this.Simulation.Update(1.0F / this.Settings.FixedTimeStep, 1);
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

    /// <inheritdoc cref="Raylib.GetFPS"/>
    public int GetFps() => Raylib.GetFPS();

    /// <inheritdoc cref="Raylib.SetTargetFPS"/>
    public void SetTargetFps(int fps) => Raylib.SetTargetFPS(fps);

    /// <inheritdoc cref="Raylib.OpenURL(string)"/>
    public void OpenUrl(string url) => Raylib.OpenURL(url);

    protected override void Dispose(bool disposing) {
        if (disposing) {
            foreach (Registry overlay in RegistryManager.RegisterTypes.ToList()) {
                overlay.Dispose();
            }
            
            foreach (Overlay overlay in OverlayManager.Overlays.ToList()) {
                overlay.Dispose();
            }

            this.Content.Dispose();
            Window.Close();
            AudioDevice.Close();
            GuiManager.ActiveGui?.Dispose();
            SceneManager.ActiveScene?.Dispose();
            this.Simulation.Dispose();
        }
    }
}