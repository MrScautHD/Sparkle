using System.Reflection;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Sparkle.csharp; 

public class ApplicationSettings {
    
    public string Title;
    public Vector2D<int> Size;
    public WindowState WindowState;
    public WindowBorder WindowBorder;
    public bool VSync;
    public bool IsVisible;
    public int TargetFps;
    public bool Headless;
    //public Texture Icon

    public ApplicationSettings() {
        this.Title = Assembly.GetEntryAssembly()!.GetName().Name ?? "Bliss Engine";
        this.Size = new Vector2D<int>(1280, 720);
        this.WindowState = WindowState.Normal;
        this.WindowBorder = WindowBorder.Resizable;
        this.VSync = true;
        this.IsVisible = true;
        this.TargetFps = 0;
        this.Headless = false;
    }
}