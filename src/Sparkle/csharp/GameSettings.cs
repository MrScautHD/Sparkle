using System.Reflection;
using Raylib_cs;
using Sparkle.csharp.physics;

namespace Sparkle.csharp;

public struct GameSettings {
    
    public string Title;
    public int WindowWidth;
    public int WindowHeight;
    public string IconPath;
    public string LogDirectory;
    public string ContentDirectory;
    public int TargetFps;
    public int FixedTimeStep;
    public ConfigFlags WindowFlags;
    public PhysicsSettings PhysicsSettings;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="GameSettings"/> with default values for various game settings such as window size, icon path, log directory, content directory, and more.
    /// </summary>
    public GameSettings() {
        this.Title = Assembly.GetEntryAssembly()!.GetName().Name ?? "Sparkle";
        this.WindowWidth = 1280;
        this.WindowHeight = 720;
        this.IconPath = string.Empty;
        this.LogDirectory = "logs";
        this.ContentDirectory = "content";
        this.TargetFps = 0;
        this.FixedTimeStep = 60;
        this.WindowFlags = ConfigFlags.FLAG_VSYNC_HINT | ConfigFlags.FLAG_WINDOW_RESIZABLE;
        this.PhysicsSettings = new PhysicsSettings();
    }
}