using OpenTK.Graphics;
using Raylib_CSharp;
using Raylib_CSharp.Audio;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Images;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Windowing;
using Sparkle.CSharp.Content;
using Sparkle.CSharp.Content.Types;
using Sparkle.CSharp.Effects;
using Sparkle.CSharp.GUI;
using Sparkle.CSharp.Logging;
using Sparkle.CSharp.Overlays;
using Sparkle.CSharp.Registries;
using Sparkle.CSharp.Registries.Types;
using Sparkle.CSharp.Rendering.Gifs;
using Sparkle.CSharp.Rendering.Gl;
using Sparkle.CSharp.Scenes;

namespace Sparkle.CSharp;

public class Game : Disposable {
    
    public static Game Instance { get; private set; }
    public static readonly Version Version = new Version(3, 1, 0);
    
    public readonly GameSettings Settings;
    public bool ShouldClose;
    
    public NativeBindingsContext BindingContext { get; private set; }
    public ContentManager Content { get; private set; }
    public Image Logo { get; private set; }
    
    public bool HasInitialized { get; private set; }
    
    private readonly double _fixedTimeStep;
    private double _timer;
    
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
        Logger.Info($"\t> CPU: {SystemInfo.Cpu}");
        Logger.Info($"\t> MEMORY: {SystemInfo.MemorySize} GB");
        Logger.Info($"\t> THREADS: {SystemInfo.Threads}");
        Logger.Info($"\t> OS: {SystemInfo.Os}");
        Logger.Info($"\t> Raylib-CSharp: {Raylib.Version}");
        Logger.Info($"\t> Raylib: {Raylib.RlVersion}");
        Logger.Info($"\t> API: {RlGl.GetVersion()}");
        
        Logger.Info("Initialize logger...");
        Logger.Init();
        
        Time.SetTargetFPS(this.Settings.TargetFps);

        Logger.Info("Initialize content manager...");
        this.Content = new ContentManager();
        
        Logger.Info("Initialize audio device...");
        AudioDevice.Init();

        Logger.Info("Initialize window...");
        Raylib.SetConfigFlags(this.Settings.WindowFlags);
        Window.Init(this.Settings.Width, this.Settings.Height, this.Settings.Title);
        
        this.Logo = this.Settings.IconPath == string.Empty ? this.Content.Load(new ImageContent("content/images/icon.png")) : this.Content.Load(new ImageContent(this.Settings.IconPath));
        Window.SetIcon(this.Logo);
        
        Logger.Info("Initialize OpenTK binding...");
        this.BindingContext = new NativeBindingsContext();
        GLLoader.LoadBindings(this.BindingContext);
        
        this.OnRun();
        
        Logger.Info("Load content...");
        this.Load();
        
        Logger.Info("Set default scene...");
        SceneManager.SetDefaultScene(scene);
        
        this.Init();
        this.HasInitialized = true;
        
        Logger.Info("Run ticks...");
        while (!this.ShouldClose && !Window.ShouldClose()) {
            this.Update();
            this.AfterUpdate();
            
            this._timer += Time.GetFrameTime();
            while (this._timer >= this._fixedTimeStep) {
                this.FixedUpdate();
                this._timer -= this._fixedTimeStep;
            }

            Graphics.BeginDrawing();
            Graphics.ClearBackground(Color.SkyBlue);
            this.Draw();
            Graphics.EndDrawing();
        }
        
        this.OnClose();
    }

    /// <summary>
    /// This method is called when the game starts.
    /// </summary>
    protected virtual void OnRun() {
        RegistryManager.AddType(new EffectRegistry());
    }
    
    /// <summary>
    /// Used for Initializes objects.
    /// </summary>
    protected virtual void Load() {
        RegistryManager.Load(this.Content);
    }

    /// <summary>
    /// Used for Initializes objects.
    /// </summary>
    protected virtual void Init() {
        RegistryManager.Init();
        EffectManager.Init();
        GifManager.Init();
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

    protected override void Dispose(bool disposing) {
        if (disposing) {
            RegistryManager.Destroy();
            OverlayManager.Destroy();
            EffectManager.Destroy();
            GifManager.Destroy();
            GuiManager.Destroy();
            SceneManager.Destroy();
            this.Content.Dispose();
            this.BindingContext.Dispose();
            AudioDevice.Close();
            Window.Close();
        }
    }
}