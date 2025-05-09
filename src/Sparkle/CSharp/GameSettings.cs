using System.Reflection;
using Bliss.CSharp.Windowing;
using Veldrid;

namespace Sparkle.CSharp;

public struct GameSettings {
    
    /// <summary>
    /// The title of the game window.
    /// </summary>
    public string Title { get; init; }
    
    /// <summary>
    /// The width of the game window in pixels.
    /// </summary>
    public int Width { get; init; }
    
    /// <summary>
    /// The height of the game window in pixels.
    /// </summary>
    public int Height { get; init; }
    
    /// <summary>
    /// The file path to the icon used for the game window.
    /// </summary>
    public string IconPath { get; init; }
    
    /// <summary>
    /// The directory path where log files will be stored.
    /// </summary>
    public string LogDirectory { get; init; }
    
    /// <summary>
    /// The target frames per second (FPS). A value of 0 means no FPS cap.
    /// </summary>
    public int TargetFps { get; init; }
    
    /// <summary>
    /// The fixed time step for game updates, typically used for physics calculations.
    /// </summary>
    public float FixedTimeStep { get; init; }
    
    /// <summary>
    /// The window state flags that define the behavior and appearance of the game window.
    /// </summary>
    public WindowState WindowFlags { get; init; }
    
    /// <summary>
    /// The graphics backend to use for rendering (e.g., Direct3D, Vulkan, Metal, OpenGL, OpenGL-ES).
    /// </summary>
    public GraphicsBackend Backend { get; init; }
    
    /// <summary>
    /// Indicates whether vertical sync (VSync) is enabled to prevent screen tearing.
    /// </summary>
    public bool VSync { get; init; }
    
    /// <summary>
    /// Indicates whether ImGui is enabled.
    /// </summary>
    public bool ImGui { get; init; }
    
    /// <summary>
    /// The sample count for anti-aliasing. Higher values result in smoother edges but increase performance cost.
    /// </summary>
    public TextureSampleCount SampleCount { get; init; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="GameSettings"/> struct with default values.
    /// </summary>
    public GameSettings() {
        this.Title = Assembly.GetEntryAssembly()?.GetName().Name ?? "Sparkle";
        this.Width = 1280;
        this.Height = 720;
        this.IconPath = string.Empty;
        this.LogDirectory = "logs";
        this.TargetFps = 0;
        this.FixedTimeStep = 1.0F / 60.0F;
        this.WindowFlags = WindowState.Resizable;
        this.Backend = Window.GetPlatformDefaultBackend();
        this.VSync = true;
        this.ImGui = true;
        this.SampleCount = TextureSampleCount.Count1;
    }
}