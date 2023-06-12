using System.Drawing;
using System.Numerics;
using Raylib_cs;

namespace Sparkle.csharp.window; 

public class Window {

    internal Window(Size size, string title) {
        Raylib.InitWindow(size.Width, size.Height, title);
    }

    public bool WindowShouldClose() => Raylib.WindowShouldClose();

    public void CloseWindow() => Raylib.CloseWindow();

    public bool IsWindowReady() => Raylib.IsWindowReady();

    public bool IsWindowFullscreen() => Raylib.IsWindowFullscreen();

    public bool IsWindowHidden() => Raylib.IsWindowHidden();

    public bool IsWindowMinimized() => Raylib.IsWindowMinimized();

    public bool IsWindowMaximized() => Raylib.IsWindowMaximized();

    public bool IsWindowFocused() => Raylib.IsWindowFocused();

    public bool IsWindowResized() => Raylib.IsWindowResized();

    public bool IsWindowState(ConfigFlags state) => Raylib.IsWindowState(state);

    public bool SetWindowState(ConfigFlags state) => Raylib.SetWindowState(state);

    public void ClearWindowState(ConfigFlags state) => Raylib.ClearWindowState(state);

    public void ToggleFullscreen() => Raylib.ToggleFullscreen();

    public void MaximizeWindow() => Raylib.MaximizeWindow();

    public void MinimizeWindow() => Raylib.MinimizeWindow();

    public void RestoreWindow() => Raylib.RestoreWindow();

    public void SetWindowIcon(Image image) => Raylib.SetWindowIcon(image);

    public unsafe void SetWindowIcons(Image* images, int count) => Raylib.SetWindowIcons(images, count);

    public void SetWindowTitle(string title) => Raylib.SetWindowTitle(title);

    public void SetWindowPosition(int x, int y) => Raylib.SetWindowPosition(x, y);

    public void SetWindowMonitor(int monitor) => Raylib.SetWindowMonitor(monitor);

    public void SetWindowMinSize(int width, int height) => Raylib.SetWindowMinSize(width, height);

    public void SetWindowSize(int width, int height) => Raylib.SetWindowSize(width, height);

    public void SetWindowOpacity(float opacity) => Raylib.SetWindowOpacity(opacity);

    public Size GetScreenSize() => new Size(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());

    public Size GetRenderHeight() => new Size(Raylib.GetRenderHeight(), Raylib.GetRenderWidth());

    public int GetMonitorCount() => Raylib.GetMonitorCount();

    public int GetCurrentMonitor() => Raylib.GetCurrentMonitor();

    public Vector2 GetMonitorPosition(int monitor) => Raylib.GetMonitorPosition(monitor);

    public Size GetMonitorSize(int monitor) => new Size(Raylib.GetMonitorWidth(monitor), Raylib.GetMonitorHeight(monitor));

    public Size GetMonitorPhysicalSize(int monitor) => new Size(Raylib.GetMonitorPhysicalWidth(monitor), Raylib.GetMonitorPhysicalHeight(monitor));

    public int GetMonitorRefreshRate(int monitor) => Raylib.GetMonitorRefreshRate(monitor);

    public Vector2 GetWindowPosition() => Raylib.GetWindowPosition();

    public Vector2 GetWindowScaleDpi() => Raylib.GetWindowScaleDPI();

    public string GetMonitorName(int monitor) => Raylib.GetMonitorName_(monitor);

    public void SetWindowStates(ConfigFlags[] states) {
        foreach (ConfigFlags state in states) {
            this.SetWindowState(state);
        }
    }
}