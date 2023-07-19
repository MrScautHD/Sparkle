using System.Numerics;
using Raylib_cs;

namespace Sparkle.csharp; 

public static class Input {

    // KEYBOARD
    public static bool IsKeyPressed(KeyboardKey key) => Raylib.IsKeyPressed(key);
    public static bool IsKeyDown(KeyboardKey key) => Raylib.IsKeyDown(key);
    public static bool IsKeyReleased(KeyboardKey key) => Raylib.IsKeyReleased(key);
    public static bool IsKeyUp(KeyboardKey key) => Raylib.IsKeyUp(key);
    public static int GetKeyPressed() => Raylib.GetKeyPressed();
    public static void SetExitKey(KeyboardKey key) => Raylib.SetExitKey(key);
    
    // MOUSE
    public static bool IsMouseButtonPressed(MouseButton button) => Raylib.IsMouseButtonPressed(button);
    public static bool IsMouseButtonDown(MouseButton button) => Raylib.IsMouseButtonDown(button);
    public static bool IsMouseButtonReleased(MouseButton button) => Raylib.IsMouseButtonReleased(button);
    public static bool IsMouseButtonUp(MouseButton button) => Raylib.IsMouseButtonUp(button);
    public static Vector2 GetMousePosition() => Raylib.GetMousePosition();
    public static Vector2 GetMouseDelta() => Raylib.GetMouseDelta();
    public static float GetMouseWheelMove() => Raylib.GetMouseWheelMove();
    public static void SetMouseCursor(MouseCursor cursor) => Raylib.SetMouseCursor(cursor);
    public static void SetMousePosition(Vector2 pos) => Raylib.SetMousePosition((int) pos.X, (int) pos.Y);
    public static void SetMouseOffset(Vector2 offset) => Raylib.SetMouseOffset((int) offset.X, (int) offset.Y);
    public static void SetMouseScale(Vector2 scale) => Raylib.SetMouseScale(scale.X, scale.Y);
    
    // GAMEPAD
    public static bool IsGamepadAvailable(int gamepad) => Raylib.IsGamepadAvailable(gamepad);
    public static bool IsGamepadButtonPressed(int gamepad, GamepadButton button) => Raylib.IsGamepadButtonPressed(gamepad, button);
    public static bool IsGamepadButtonDown(int gamepad, GamepadButton button) => Raylib.IsGamepadButtonDown(gamepad, button);
    public static bool IsGamepadButtonReleased(int gamepad, GamepadButton button) => Raylib.IsGamepadButtonReleased(gamepad, button);
    public static bool IsGamepadButtonUp(int gamepad, GamepadButton button) => Raylib.IsGamepadButtonUp(gamepad, button);
    public static int GetGamepadButtonPressed() => Raylib.GetGamepadButtonPressed();
    public static int GetGamepadAxisCount(int gamepad) => Raylib.GetGamepadAxisCount(gamepad);
    public static float GetGamepadAxisMovement(int gamepad, GamepadAxis axis) => Raylib.GetGamepadAxisMovement(gamepad, axis);
    public static unsafe void SetGamepadMappings(string mappings) => Raylib.SetGamepadMappings(mappings.ToUTF8Buffer().AsPointer());
    
    // TOUCH
    public static int GetTouchX() => Raylib.GetTouchX();
    public static int GetTouchY() => Raylib.GetTouchY();
    public static Vector2 GetTouchPosition(int index) => Raylib.GetTouchPosition(index);
    public static int GetTouchPointId(int index) => Raylib.GetTouchPointId(index);
    public static int GetTouchPointCount() => Raylib.GetTouchPointCount();
    
    // GESTURES
    public static void SetGesturesEnabled(Gesture flags) => Raylib.SetGesturesEnabled(flags);
    public static bool IsGestureDetected(Gesture gesture) => Raylib.IsGestureDetected(gesture);
    public static Gesture GetGestureDetected() => Raylib.GetGestureDetected();
    public static float GetGestureHoldDuration() => Raylib.GetGestureHoldDuration();
    public static Vector2 GetGestureDragVector() => Raylib.GetGestureDragVector();
    public static float GetGestureDragAngle() => Raylib.GetGestureDragAngle();
    public static Vector2 GetGesturePinchVector() => Raylib.GetGesturePinchVector();
    public static float GetGesturePinchAngle() => Raylib.GetGesturePinchAngle();
    
    // CURSOR
    public static void ShowCursor() => Raylib.ShowCursor();
    public static void HideCursor() => Raylib.HideCursor();
    public static bool IsCursorHidden() => Raylib.IsCursorHidden();
    public static void EnableCursor() => Raylib.EnableCursor();
    public static void DisableCursor() => Raylib.DisableCursor();
    public static bool IsCursorOnScreen() => Raylib.IsCursorOnScreen();
}