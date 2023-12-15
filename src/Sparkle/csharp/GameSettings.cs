using System.Reflection;
using JoltPhysicsSharp;
using Raylib_cs;
using Sparkle.csharp.physics;

namespace Sparkle.csharp;

public struct GameSettings {
    
    public string Title;
    public int Width;
    public int Height;
    public string IconPath;
    public string LogDirectory;
    public int TargetFps;
    public int FixedTimeStep;
    public ConfigFlags WindowFlags;
    public PhysicsSettings PhysicsSettings;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="GameSettings"/> with default values for various game settings such as window size, icon path, log directory, content directory, and more.
    /// </summary>
    public GameSettings() {
        this.Title = Assembly.GetEntryAssembly()!.GetName().Name ?? "Sparkle";
        this.Width = 1280;
        this.Height = 720;
        this.IconPath = string.Empty;
        this.LogDirectory = "logs";
        this.TargetFps = 0;
        this.FixedTimeStep = 60;
        this.WindowFlags = ConfigFlags.FLAG_VSYNC_HINT | ConfigFlags.FLAG_WINDOW_RESIZABLE;
        this.PhysicsSettings = new PhysicsSettings();
    }
}