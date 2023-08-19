using System.Numerics;
using Raylib_cs;

namespace Sparkle.csharp; 

public static class Input {

    /// <summary> See <see cref="Raylib.IsKeyPressed"/> </summary>
    public static bool IsKeyPressed(KeyboardKey key) => Raylib.IsKeyPressed(key);
    
    /// <summary> See <see cref="Raylib.IsKeyDown"/> </summary>
    public static bool IsKeyDown(KeyboardKey key) => Raylib.IsKeyDown(key);
    
    /// <summary> See <see cref="Raylib.IsKeyReleased"/> </summary>
    public static bool IsKeyReleased(KeyboardKey key) => Raylib.IsKeyReleased(key);
    
    /// <summary> See <see cref="Raylib.IsKeyUp"/> </summary>
    public static bool IsKeyUp(KeyboardKey key) => Raylib.IsKeyUp(key);
    
    /// <summary> See <see cref="Raylib.GetKeyPressed"/> </summary>
    public static int GetKeyPressed() => Raylib.GetKeyPressed();
    
    /// <summary> See <see cref="Raylib.GetCharPressed"/> </summary>
    public static int GetCharPressed() => Raylib.GetCharPressed();
    
    /// <summary> See <see cref="Raylib.SetExitKey"/> </summary>
    public static void SetExitKey(KeyboardKey key) => Raylib.SetExitKey(key);
    
    
    /// <summary> See <see cref="Raylib.IsMouseButtonPressed"/> </summary>
    public static bool IsMouseButtonPressed(MouseButton button) => Raylib.IsMouseButtonPressed(button);
    
    /// <summary> See <see cref="Raylib.IsMouseButtonDown"/> </summary>
    public static bool IsMouseButtonDown(MouseButton button) => Raylib.IsMouseButtonDown(button);
    
    /// <summary> See <see cref="Raylib.IsMouseButtonReleased"/> </summary>
    public static bool IsMouseButtonReleased(MouseButton button) => Raylib.IsMouseButtonReleased(button);
    
    /// <summary> See <see cref="Raylib.IsMouseButtonUp"/> </summary>
    public static bool IsMouseButtonUp(MouseButton button) => Raylib.IsMouseButtonUp(button);
    
    /// <summary> See <see cref="Raylib.GetMouseX"/> </summary>
    public static int GetMouseX() => Raylib.GetMouseX();
    
    /// <summary> See <see cref="Raylib.GetMouseY"/> </summary>
    public static int GetMouseY() => Raylib.GetMouseY();
    
    /// <summary> See <see cref="Raylib.GetMousePosition"/> </summary>
    public static Vector2 GetMousePosition() => Raylib.GetMousePosition();
    
    /// <summary> See <see cref="Raylib.GetMouseDelta"/> </summary>
    public static Vector2 GetMouseDelta() => Raylib.GetMouseDelta();
    
    /// <summary> See <see cref="Raylib.GetMouseWheelMove"/> </summary>
    public static float GetMouseWheelMove() => Raylib.GetMouseWheelMove();
    
    /// <summary> See <see cref="Raylib.GetMouseWheelMoveV"/> </summary>
    public static Vector2 GetMouseWheelMoveV() => Raylib.GetMouseWheelMoveV();
    
    /// <summary> See <see cref="Raylib.SetMouseCursor"/> </summary>
    public static void SetMouseCursor(MouseCursor cursor) => Raylib.SetMouseCursor(cursor);
    
    /// <summary> See <see cref="Raylib.SetMousePosition"/> </summary>
    public static void SetMousePosition(int x, int y) => Raylib.SetMousePosition(x, y);
    
    /// <summary> See <see cref="Raylib.SetMouseOffset"/> </summary>
    public static void SetMouseOffset(int x, int y) => Raylib.SetMouseOffset(x, y);
    
    /// <summary> See <see cref="Raylib.SetMouseScale"/> </summary>
    public static void SetMouseScale(float scaleX, float scaleY) => Raylib.SetMouseScale(scaleX, scaleY);
    
    
    /// <summary> See <see cref="Raylib.IsGamepadAvailable"/> </summary>
    public static bool IsGamepadAvailable(int gamepad) => Raylib.IsGamepadAvailable(gamepad);
    
    /// <summary> See <see cref="Raylib.GetGamepadName_"/> </summary>
    public static string GetGamepadName(int gamepad) => Raylib.GetGamepadName_(gamepad);
    
    /// <summary> See <see cref="Raylib.IsGamepadButtonPressed"/> </summary>
    public static bool IsGamepadButtonPressed(int gamepad, GamepadButton button) => Raylib.IsGamepadButtonPressed(gamepad, button);
    
    /// <summary> See <see cref="Raylib.IsGamepadButtonDown"/> </summary>
    public static bool IsGamepadButtonDown(int gamepad, GamepadButton button) => Raylib.IsGamepadButtonDown(gamepad, button);
    
    /// <summary> See <see cref="Raylib.IsGamepadButtonReleased"/> </summary>
    public static bool IsGamepadButtonReleased(int gamepad, GamepadButton button) => Raylib.IsGamepadButtonReleased(gamepad, button);
    
    /// <summary> See <see cref="Raylib.IsGamepadButtonUp"/> </summary>
    public static bool IsGamepadButtonUp(int gamepad, GamepadButton button) => Raylib.IsGamepadButtonUp(gamepad, button);
    
    /// <summary> See <see cref="Raylib.GetGamepadButtonPressed"/> </summary>
    public static int GetGamepadButtonPressed() => Raylib.GetGamepadButtonPressed();
    
    /// <summary> See <see cref="Raylib.GetGamepadAxisCount"/> </summary>
    public static int GetGamepadAxisCount(int gamepad) => Raylib.GetGamepadAxisCount(gamepad);
    
    /// <summary> See <see cref="Raylib.GetGamepadAxisMovement"/> </summary>
    public static float GetGamepadAxisMovement(int gamepad, GamepadAxis axis) => Raylib.GetGamepadAxisMovement(gamepad, axis);
    
    /// <summary> See <see cref="Raylib.SetGamepadMappings(string)"/> </summary>
    public static void SetGamepadMappings(string mappings) => Raylib.SetGamepadMappings(mappings);
    
    
    /// <summary> See <see cref="Raylib.GetTouchX"/> </summary>
    public static int GetTouchX() => Raylib.GetTouchX();
    
    /// <summary> See <see cref="Raylib.GetTouchY"/> </summary>
    public static int GetTouchY() => Raylib.GetTouchY();
    
    /// <summary> See <see cref="Raylib.GetTouchPosition"/> </summary>
    public static Vector2 GetTouchPosition(int index) => Raylib.GetTouchPosition(index);
    
    /// <summary> See <see cref="Raylib.GetTouchPointId"/> </summary>
    public static int GetTouchPointId(int index) => Raylib.GetTouchPointId(index);
    
    /// <summary> See <see cref="Raylib.GetTouchPointCount"/> </summary>
    public static int GetTouchPointCount() => Raylib.GetTouchPointCount();
    

    /// <summary> See <see cref="Raylib.SetGesturesEnabled"/> </summary>
    public static void SetGesturesEnabled(Gesture flags) => Raylib.SetGesturesEnabled(flags);
    
    /// <summary> See <see cref="Raylib.IsGestureDetected"/> </summary>
    public static bool IsGestureDetected(Gesture gesture) => Raylib.IsGestureDetected(gesture);
    
    /// <summary> See <see cref="Raylib.GetGestureDetected"/> </summary>
    public static Gesture GetGestureDetected() => Raylib.GetGestureDetected();
    
    /// <summary> See <see cref="Raylib.GetGestureHoldDuration"/> </summary>
    public static float GetGestureHoldDuration() => Raylib.GetGestureHoldDuration();
    
    /// <summary> See <see cref="Raylib.GetGestureDragVector"/> </summary>
    public static Vector2 GetGestureDragVector() => Raylib.GetGestureDragVector();
    
    /// <summary> See <see cref="Raylib.GetGestureDragAngle"/> </summary>
    public static float GetGestureDragAngle() => Raylib.GetGestureDragAngle();
    
    /// <summary> See <see cref="Raylib.GetGesturePinchVector"/> </summary>
    public static Vector2 GetGesturePinchVector() => Raylib.GetGesturePinchVector();
    
    /// <summary> See <see cref="Raylib.GetGesturePinchAngle"/> </summary>
    public static float GetGesturePinchAngle() => Raylib.GetGesturePinchAngle();
    

    /// <summary> See <see cref="Raylib.ShowCursor"/> </summary>
    public static void ShowCursor() => Raylib.ShowCursor();
    
    /// <summary> See <see cref="Raylib.HideCursor"/> </summary>
    public static void HideCursor() => Raylib.HideCursor();
    
    /// <summary> See <see cref="Raylib.IsCursorHidden"/> </summary>
    public static bool IsCursorHidden() => Raylib.IsCursorHidden();
    
    /// <summary> See <see cref="Raylib.EnableCursor"/> </summary>
    public static void EnableCursor() => Raylib.EnableCursor();
    
    /// <summary> See <see cref="Raylib.DisableCursor"/> </summary>
    public static void DisableCursor() => Raylib.DisableCursor();
    
    /// <summary> See <see cref="Raylib.IsCursorOnScreen"/> </summary>
    public static bool IsCursorOnScreen() => Raylib.IsCursorOnScreen();
}