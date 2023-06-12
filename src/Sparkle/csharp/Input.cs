using Raylib_cs;

namespace Sparkle.csharp; 

public static class Input {

    public static bool IsKeyDown(KeyboardKey key) => Raylib.IsKeyDown(key);
    public static bool IsKeyPressed(KeyboardKey key) => Raylib.IsKeyPressed(key);
    public static bool IsKeyReleased(KeyboardKey key) => Raylib.IsKeyReleased(key);
    
    public static bool IsMouseDown(MouseButton button) => Raylib.IsMouseButtonDown(button);
    public static bool IsMousePressed(MouseButton button) => Raylib.IsMouseButtonPressed(button);
    public static bool IsMouseReleased(MouseButton button) => Raylib.IsMouseButtonReleased(button);
    
    public static void SetExitKey(KeyboardKey key) => Raylib.SetExitKey(key);
    
    public static void ShowCursor() => Raylib.ShowCursor();
    
    public static void HideCursor() => Raylib.HideCursor();
    
    public static bool IsCursorHidden() => Raylib.IsCursorHidden();
    
    public static void EnableCursor() => Raylib.EnableCursor();
    
    public static void DisableCursor() => Raylib.DisableCursor();
    
    public static bool IsCursorOnScreen() => Raylib.IsCursorOnScreen();
}