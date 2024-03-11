using System.Numerics;
using Raylib_cs;

namespace Sparkle.CSharp;

public static class Input {
    
    /// <inheritdoc cref="Raylib.PollInputEvents"/>
    public static void PollInputEvents() => Raylib.PollInputEvents();
    

    /// <inheritdoc cref="Raylib.IsKeyPressed"/>
    public static bool IsKeyPressed(KeyboardKey key) => Raylib.IsKeyPressed(key);
    
    /// <inheritdoc cref="Raylib.IsKeyPressedRepeat"/>
    public static bool IsKeyPressedRepeat(KeyboardKey key) => Raylib.IsKeyPressedRepeat(key);
    
    /// <inheritdoc cref="Raylib.IsKeyDown"/>
    public static bool IsKeyDown(KeyboardKey key) => Raylib.IsKeyDown(key);
    
    /// <inheritdoc cref="Raylib.IsKeyReleased"/>
    public static bool IsKeyReleased(KeyboardKey key) => Raylib.IsKeyReleased(key);
    
    /// <inheritdoc cref="Raylib.IsKeyUp"/>
    public static bool IsKeyUp(KeyboardKey key) => Raylib.IsKeyUp(key);
    
    /// <inheritdoc cref="Raylib.GetKeyPressed"/>
    public static int GetKeyPressed() => Raylib.GetKeyPressed();
    
    /// <inheritdoc cref="Raylib.GetCharPressed"/>
    public static int GetCharPressed() => Raylib.GetCharPressed();
    
    /// <inheritdoc cref="Raylib.SetExitKey"/>
    public static void SetExitKey(KeyboardKey key) => Raylib.SetExitKey(key);
    
    
    /// <inheritdoc cref="Raylib.IsMouseButtonPressed"/>
    public static bool IsMouseButtonPressed(MouseButton button) => Raylib.IsMouseButtonPressed(button);
    
    /// <inheritdoc cref="Raylib.IsMouseButtonDown"/>
    public static bool IsMouseButtonDown(MouseButton button) => Raylib.IsMouseButtonDown(button);
    
    /// <inheritdoc cref="Raylib.IsMouseButtonReleased"/>
    public static bool IsMouseButtonReleased(MouseButton button) => Raylib.IsMouseButtonReleased(button);
    
    /// <inheritdoc cref="Raylib.IsMouseButtonUp"/>
    public static bool IsMouseButtonUp(MouseButton button) => Raylib.IsMouseButtonUp(button);
    
    /// <inheritdoc cref="Raylib.GetMouseX"/>
    public static int GetMouseX() => Raylib.GetMouseX();
    
    /// <inheritdoc cref="Raylib.GetMouseY"/>
    public static int GetMouseY() => Raylib.GetMouseY();
    
    /// <inheritdoc cref="Raylib.GetMousePosition"/>
    public static Vector2 GetMousePosition() => Raylib.GetMousePosition();
    
    /// <inheritdoc cref="Raylib.GetMouseDelta"/>
    public static Vector2 GetMouseDelta() => Raylib.GetMouseDelta();
    
    /// <inheritdoc cref="Raylib.GetMouseWheelMove"/>
    public static float GetMouseWheelMove() => Raylib.GetMouseWheelMove();
    
    /// <inheritdoc cref="Raylib.GetMouseWheelMoveV"/>
    public static Vector2 GetMouseWheelMoveV() => Raylib.GetMouseWheelMoveV();
    
    /// <inheritdoc cref="Raylib.SetMouseCursor"/>
    public static void SetMouseCursor(MouseCursor cursor) => Raylib.SetMouseCursor(cursor);
    
    /// <inheritdoc cref="Raylib.SetMousePosition"/>
    public static void SetMousePosition(int x, int y) => Raylib.SetMousePosition(x, y);
    
    /// <inheritdoc cref="Raylib.SetMouseOffset"/>
    public static void SetMouseOffset(int x, int y) => Raylib.SetMouseOffset(x, y);
    
    /// <inheritdoc cref="Raylib.SetMouseScale"/>
    public static void SetMouseScale(float scaleX, float scaleY) => Raylib.SetMouseScale(scaleX, scaleY);
    
    
    /// <inheritdoc cref="Raylib.IsGamepadAvailable"/>
    public static bool IsGamepadAvailable(int gamepad) => Raylib.IsGamepadAvailable(gamepad);
    
    /// <inheritdoc cref="Raylib.GetGamepadName_"/>
    public static string GetGamepadName(int gamepad) => Raylib.GetGamepadName_(gamepad);
    
    /// <inheritdoc cref="Raylib.IsGamepadButtonPressed"/>
    public static bool IsGamepadButtonPressed(int gamepad, GamepadButton button) => Raylib.IsGamepadButtonPressed(gamepad, button);
    
    /// <inheritdoc cref="Raylib.IsGamepadButtonDown"/>
    public static bool IsGamepadButtonDown(int gamepad, GamepadButton button) => Raylib.IsGamepadButtonDown(gamepad, button);
    
    /// <inheritdoc cref="Raylib.IsGamepadButtonReleased"/>
    public static bool IsGamepadButtonReleased(int gamepad, GamepadButton button) => Raylib.IsGamepadButtonReleased(gamepad, button);
    
    /// <inheritdoc cref="Raylib.IsGamepadButtonUp"/>
    public static bool IsGamepadButtonUp(int gamepad, GamepadButton button) => Raylib.IsGamepadButtonUp(gamepad, button);
    
    /// <inheritdoc cref="Raylib.GetGamepadButtonPressed"/>
    public static int GetGamepadButtonPressed() => Raylib.GetGamepadButtonPressed();
    
    /// <inheritdoc cref="Raylib.GetGamepadAxisCount"/>
    public static int GetGamepadAxisCount(int gamepad) => Raylib.GetGamepadAxisCount(gamepad);
    
    /// <inheritdoc cref="Raylib.GetGamepadAxisMovement"/>
    public static float GetGamepadAxisMovement(int gamepad, GamepadAxis axis) => Raylib.GetGamepadAxisMovement(gamepad, axis);
    
    /// <inheritdoc cref="Raylib.SetGamepadMappings(string)"/>
    public static void SetGamepadMappings(string mappings) => Raylib.SetGamepadMappings(mappings);
    
    
    /// <inheritdoc cref="Raylib.GetTouchX"/>
    public static int GetTouchX() => Raylib.GetTouchX();
    
    /// <inheritdoc cref="Raylib.GetTouchY"/>
    public static int GetTouchY() => Raylib.GetTouchY();
    
    /// <inheritdoc cref="Raylib.GetTouchPosition"/>
    public static Vector2 GetTouchPosition(int index) => Raylib.GetTouchPosition(index);
    
    /// <inheritdoc cref="Raylib.GetTouchPointId"/>
    public static int GetTouchPointId(int index) => Raylib.GetTouchPointId(index);
    
    /// <inheritdoc cref="Raylib.GetTouchPointCount"/>
    public static int GetTouchPointCount() => Raylib.GetTouchPointCount();
    

    /// <inheritdoc cref="Raylib.SetGesturesEnabled"/>
    public static void SetGesturesEnabled(Gesture flags) => Raylib.SetGesturesEnabled(flags);
    
    /// <inheritdoc cref="Raylib.IsGestureDetected"/>
    public static bool IsGestureDetected(Gesture gesture) => Raylib.IsGestureDetected(gesture);
    
    /// <inheritdoc cref="Raylib.GetGestureDetected"/>
    public static Gesture GetGestureDetected() => Raylib.GetGestureDetected();
    
    /// <inheritdoc cref="Raylib.GetGestureHoldDuration"/>
    public static float GetGestureHoldDuration() => Raylib.GetGestureHoldDuration();
    
    /// <inheritdoc cref="Raylib.GetGestureDragVector"/>
    public static Vector2 GetGestureDragVector() => Raylib.GetGestureDragVector();
    
    /// <inheritdoc cref="Raylib.GetGestureDragAngle"/>
    public static float GetGestureDragAngle() => Raylib.GetGestureDragAngle();
    
    /// <inheritdoc cref="Raylib.GetGesturePinchVector"/>
    public static Vector2 GetGesturePinchVector() => Raylib.GetGesturePinchVector();
    
    /// <inheritdoc cref="Raylib.GetGesturePinchAngle"/>
    public static float GetGesturePinchAngle() => Raylib.GetGesturePinchAngle();
    

    /// <inheritdoc cref="Raylib.ShowCursor"/>
    public static void ShowCursor() => Raylib.ShowCursor();
    
    /// <inheritdoc cref="Raylib.HideCursor"/>
    public static void HideCursor() => Raylib.HideCursor();
    
    /// <inheritdoc cref="Raylib.IsCursorHidden"/>
    public static bool IsCursorHidden() => Raylib.IsCursorHidden();
    
    /// <inheritdoc cref="Raylib.EnableCursor"/>
    public static void EnableCursor() => Raylib.EnableCursor();
    
    /// <inheritdoc cref="Raylib.DisableCursor"/>
    public static void DisableCursor() => Raylib.DisableCursor();
    
    /// <inheritdoc cref="Raylib.IsCursorOnScreen"/>
    public static bool IsCursorOnScreen() => Raylib.IsCursorOnScreen();
}