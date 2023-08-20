using System.Reflection;
using Raylib_cs;

namespace Sparkle.csharp;

public struct GameSettings {
    
    public string Title;
    public int WindowWidth;
    public int WindowHeight;
    public string IconPath;
    public string LogDirectory;
    public string ContentDirectory;
    public int TargetFps;
    public ConfigFlags ConfigFlag;
    
    public GameSettings() {
        this.Title = Assembly.GetEntryAssembly()!.GetName().Name ?? "Sparkle";
        this.WindowWidth = 1280;
        this.WindowHeight = 720;
        this.IconPath = string.Empty;
        this.LogDirectory = "logs";
        this.ContentDirectory = "content";
        this.TargetFps = 0;
        this.ConfigFlag = ConfigFlags.FLAG_VSYNC_HINT | ConfigFlags.FLAG_WINDOW_RESIZABLE;
    }
}