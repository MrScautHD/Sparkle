using System.Drawing;
using System.Numerics;
using Raylib_cs;

namespace Sparkle.csharp.window; 

public class Window {

    private Size _size;
    private string _title;

    internal Window(Size size, string title) {
        this._size = size;
        this._title = title;
    }

    internal void Init() {
        Raylib.InitWindow(this._size.Width, this._size.Height, this._title);
    }

    public bool ShouldClose() => Raylib.WindowShouldClose();

    public void Close() => Raylib.CloseWindow();
    
    public void TakeScreenshot(string path) => Raylib.TakeScreenshot(path);

    public bool IsReady() => Raylib.IsWindowReady();

    public bool IsFullscreen() => Raylib.IsWindowFullscreen();

    public bool IsHidden() => Raylib.IsWindowHidden();

    public bool IsMinimized() => Raylib.IsWindowMinimized();

    public bool IsMaximized() => Raylib.IsWindowMaximized();

    public bool IsFocused() => Raylib.IsWindowFocused();

    public bool IsResized() => Raylib.IsWindowResized();

    public bool IsState(ConfigFlags state) => Raylib.IsWindowState(state);

    public bool SetState(ConfigFlags state) => Raylib.SetWindowState(state);

    public void ClearState(ConfigFlags state) => Raylib.ClearWindowState(state);

    public void ToggleFullscreen() => Raylib.ToggleFullscreen();

    public void Maximize() => Raylib.MaximizeWindow();

    public void Minimize() => Raylib.MinimizeWindow();

    public void Restore() => Raylib.RestoreWindow();

    public void SetIcon(Image image) => Raylib.SetWindowIcon(image);

    public unsafe void SetIcons(Image* images, int count) => Raylib.SetWindowIcons(images, count);

    public void SetTitle(string title) => Raylib.SetWindowTitle(title);
    
    public void SetClipboardText(string path) => Raylib.SetClipboardText(path);

    public void SetPosition(int x, int y) => Raylib.SetWindowPosition(x, y);

    public void SetMonitor(int monitor) => Raylib.SetWindowMonitor(monitor);

    public void SetMinSize(int width, int height) => Raylib.SetWindowMinSize(width, height);

    public void SetSize(int width, int height) => Raylib.SetWindowSize(width, height);

    public void SetOpacity(float opacity) => Raylib.SetWindowOpacity(opacity);
    
    public void EnableEventWaiting() => Raylib.EnableEventWaiting();
    
    public void DisableEventWaiting() => Raylib.DisableEventWaiting();

    public Size GetScreenSize() => new Size(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());

    public Size GetRenderHeight() => new Size(Raylib.GetRenderHeight(), Raylib.GetRenderWidth());

    public int GetMonitorCount() => Raylib.GetMonitorCount();

    public int GetCurrentMonitor() => Raylib.GetCurrentMonitor();

    public Vector2 GetMonitorPosition(int monitor) => Raylib.GetMonitorPosition(monitor);

    public Size GetMonitorSize(int monitor) => new Size(Raylib.GetMonitorWidth(monitor), Raylib.GetMonitorHeight(monitor));

    public Size GetMonitorPhysicalSize(int monitor) => new Size(Raylib.GetMonitorPhysicalWidth(monitor), Raylib.GetMonitorPhysicalHeight(monitor));

    public int GetMonitorRefreshRate(int monitor) => Raylib.GetMonitorRefreshRate(monitor);

    public Vector2 GetPosition() => Raylib.GetWindowPosition();

    public Vector2 GetScaleDpi() => Raylib.GetWindowScaleDPI();

    public string GetMonitorName(int monitor) => Raylib.GetMonitorName_(monitor);

    public void SetStates(ConfigFlags[] states) {
        foreach (ConfigFlags state in states) {
            this.SetState(state);
        }
    }
}