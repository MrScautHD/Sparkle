using System.Drawing;
using System.Reflection;
using Raylib_cs;

namespace Sparkle.csharp; 

public class ApplicationSettings {
    
    public string Title;
    public Size Size;
    public int TargetFps;
    public bool Headless;
    public Image? Icon;
    public ConfigFlags[] WindowState;
    
    public ApplicationSettings() {
        this.Title = Assembly.GetEntryAssembly()!.GetName().Name ?? "Sparkle Engine";
        this.Size = new Size(1280, 720);
        this.TargetFps = 0;
        this.Headless = false;
        this.WindowState = new[] {
            ConfigFlags.FLAG_VSYNC_HINT
        };
        this.Icon = null;
    }
}