using System.Reflection;
using Raylib_cs;

namespace Sparkle.CSharp;

public struct GameSettings {
    
    public string Title { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public string IconPath { get; init; }
    public string LogDirectory { get; init; }
    public int TargetFps { get; init; }
    public int FixedTimeStep { get; init; }
    public ConfigFlags WindowFlags { get; init; }
    
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
        this.WindowFlags = ConfigFlags.VSyncHint | ConfigFlags.ResizableWindow;
    }
}