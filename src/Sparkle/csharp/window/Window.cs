using System.Numerics;
using Raylib_cs;

namespace Sparkle.csharp.window; 

public static class Window {

    /// <inheritdoc cref="Raylib.InitWindow(int, int, string)"/>
    public static void Init(int width, int height, string title) => Raylib.InitWindow(width, height, title);
    
    /// <inheritdoc cref="Raylib.WindowShouldClose"/>
    public static bool ShouldClose() => Raylib.WindowShouldClose();
    
    /// <inheritdoc cref="Raylib.CloseWindow"/>
    public static void Close() => Raylib.CloseWindow();
    
    /// <inheritdoc cref="Raylib.TakeScreenshot(string)"/>
    public static void TakeScreenshot(string path) => Raylib.TakeScreenshot(path);
    
    
    /// <inheritdoc cref="Raylib.IsWindowReady"/>
    public static bool IsReady() => Raylib.IsWindowReady();
    
    /// <inheritdoc cref="Raylib.IsWindowFullscreen"/>
    public static bool IsFullscreen() => Raylib.IsWindowFullscreen();
    
    /// <inheritdoc cref="Raylib.IsWindowHidden"/>
    public static bool IsHidden() => Raylib.IsWindowHidden();
    
    /// <inheritdoc cref="Raylib.IsWindowMinimized"/>
    public static bool IsMinimized() => Raylib.IsWindowMinimized();
    
    /// <inheritdoc cref="Raylib.IsWindowMaximized"/>
    public static bool IsMaximized() => Raylib.IsWindowMaximized();
    
    /// <inheritdoc cref="Raylib.IsWindowFocused"/>
    public static bool IsFocused() => Raylib.IsWindowFocused();
    
    /// <inheritdoc cref="Raylib.IsWindowResized"/>
    public static bool IsResized() => Raylib.IsWindowResized();
    
    /// <inheritdoc cref="Raylib.IsWindowState"/>
    public static bool IsState(ConfigFlags state) => Raylib.IsWindowState(state);
    
    
    /// <inheritdoc cref="Raylib.SetConfigFlags"/>
    public static void SetConfigFlags(ConfigFlags state) => Raylib.SetConfigFlags(state);
    
    /// <inheritdoc cref="Raylib.SetWindowState"/>
    public static bool SetState(ConfigFlags state) => Raylib.SetWindowState(state);
    
    /// <inheritdoc cref="Raylib.ClearWindowState"/>
    public static void ClearState(ConfigFlags state) => Raylib.ClearWindowState(state);
    
    /// <inheritdoc cref="Raylib.ToggleFullscreen"/>
    public static void ToggleFullscreen() => Raylib.ToggleFullscreen();
    
    /// <inheritdoc cref="Raylib.MaximizeWindow"/>
    public static void Maximize() => Raylib.MaximizeWindow();
    
    /// <inheritdoc cref="Raylib.MinimizeWindow"/>
    public static void Minimize() => Raylib.MinimizeWindow();
    
    /// <inheritdoc cref="Raylib.RestoreWindow"/>
    public static void Restore() => Raylib.RestoreWindow();
    
    /// <inheritdoc cref="Raylib.SetWindowIcon"/>
    public static void SetIcon(Image image) => Raylib.SetWindowIcon(image);
    
    /// <inheritdoc cref="Raylib.SetWindowIcons"/>
    public static unsafe void SetIcons(Image* images, int count) => Raylib.SetWindowIcons(images, count);
    
    /// <inheritdoc cref="Raylib.SetWindowTitle(string)"/>
    public static void SetTitle(string title) => Raylib.SetWindowTitle(title);
    
    /// <inheritdoc cref="Raylib.SetClipboardText(string)"/>
    public static void SetClipboardText(string path) => Raylib.SetClipboardText(path);
    
    /// <inheritdoc cref="Raylib.SetWindowPosition"/>
    public static void SetPosition(int x, int y) => Raylib.SetWindowPosition(x, y);
    
    /// <inheritdoc cref="Raylib.SetWindowMonitor"/>
    public static void SetMonitor(int monitor) => Raylib.SetWindowMonitor(monitor);
    
    /// <inheritdoc cref="Raylib.SetWindowMinSize"/>
    public static void SetMinSize(int width, int height) => Raylib.SetWindowMinSize(width, height);
    
    /// <inheritdoc cref="Raylib.SetWindowSize"/>
    public static void SetSize(int width, int height) => Raylib.SetWindowSize(width, height);
    
    /// <inheritdoc cref="Raylib.SetWindowOpacity"/>
    public static void SetOpacity(float opacity) => Raylib.SetWindowOpacity(opacity);
    
    /// <inheritdoc cref="Raylib.EnableEventWaiting"/>
    public static void EnableEventWaiting() => Raylib.EnableEventWaiting();
    
    /// <inheritdoc cref="Raylib.DisableEventWaiting"/>
    public static void DisableEventWaiting() => Raylib.DisableEventWaiting();
    
    
    /// <inheritdoc cref="Raylib.GetScreenWidth"/>
    public static int GetScreenWidth() => Raylib.GetScreenWidth();
    
    /// <inheritdoc cref="Raylib.GetScreenHeight"/>
    public static int GetScreenHeight() => Raylib.GetScreenHeight();
    
    /// <inheritdoc cref="Raylib.GetRenderWidth"/>
    public static int GetRenderWidth() => Raylib.GetRenderWidth();
    
    /// <inheritdoc cref="Raylib.GetRenderHeight"/>
    public static int GetRenderHeight() => Raylib.GetRenderHeight();
    
    /// <inheritdoc cref="Raylib.GetMonitorCount"/>
    public static int GetMonitorCount() => Raylib.GetMonitorCount();
    
    /// <inheritdoc cref="Raylib.GetCurrentMonitor"/>
    public static int GetCurrentMonitor() => Raylib.GetCurrentMonitor();
    
    /// <inheritdoc cref="Raylib.GetMonitorPosition"/>
    public static Vector2 GetMonitorPosition(int monitor) => Raylib.GetMonitorPosition(monitor);
    
    /// <inheritdoc cref="Raylib.GetMonitorWidth"/>
    public static int GetMonitorWidth(int monitor) => Raylib.GetMonitorWidth(monitor);
    
    /// <inheritdoc cref="Raylib.GetMonitorHeight"/>
    public static int GetMonitorHeight(int monitor) => Raylib.GetMonitorHeight(monitor);
    
    /// <inheritdoc cref="Raylib.GetMonitorPhysicalWidth"/>
    public static int GetMonitorPhysicalWidth(int monitor) => Raylib.GetMonitorPhysicalWidth(monitor);
    
    /// <inheritdoc cref="Raylib.GetMonitorPhysicalHeight"/>
    public static int GetMonitorPhysicalHeight(int monitor) => Raylib.GetMonitorPhysicalHeight(monitor);
    
    /// <inheritdoc cref="Raylib.GetMonitorRefreshRate"/>
    public static int GetMonitorRefreshRate(int monitor) => Raylib.GetMonitorRefreshRate(monitor);
    
    /// <inheritdoc cref="Raylib.GetWindowPosition"/>
    public static Vector2 GetPosition() => Raylib.GetWindowPosition();
    
    /// <inheritdoc cref="Raylib.GetWindowScaleDPI"/>
    public static Vector2 GetScaleDpi() => Raylib.GetWindowScaleDPI();
    
    /// <inheritdoc cref="Raylib.GetMonitorName_"/>
    public static string GetMonitorName(int monitor) => Raylib.GetMonitorName_(monitor);
    
    /// <inheritdoc cref="Raylib.GetClipboardText_"/>
    public static string GetClipboardText() => Raylib.GetClipboardText_();
}