using System.Numerics;
using Silk.NET.Input;

namespace Sparkle.csharp; 

public static class Input {

    public static IInputContext Context => Application.Instance.InputContext;
    
    public delegate void OnKeyDown(IKeyboard keyboard, Key key, int keyCode);
    public delegate void OnKeyReleased(IKeyboard keyboard, Key key, int keyCode);

    public delegate void OnMouseButtonDown(IMouse mouse, MouseButton button);
    public delegate void OnMouseButtonReleased(IMouse mouse, MouseButton button);
    public delegate void OnMouseMove(IMouse mouse, Vector2 pos);
    public delegate void OnMouseScrolling(IMouse mouse, ScrollWheel scrollWheel);
    
    public static event OnKeyDown? KeyIsDown;
    public static event OnKeyReleased? KeyIsReleased;
    
    public static event OnMouseButtonDown? MouseButtonIsDown;
    public static event OnMouseButtonReleased? MouseButtonIsReleased;
    public static event OnMouseMove? MouseIsMoving;
    public static event OnMouseScrolling? MouseIsScrolling;
    
    private static HashSet<Key> KeysDown = new();
    private static HashSet<Key> KeysPressed = new();
    private static HashSet<Key> KeysReleased = new();
    
    private static HashSet<MouseButton> MouseButtonsDown = new();
    private static HashSet<MouseButton> MouseButtonsPressed = new();
    private static HashSet<MouseButton> MouseButtonsReleased = new();

    internal static void Init() {
        foreach (var keyboard in Context.Keyboards) {
            keyboard.KeyDown += KeyPressed;
            keyboard.KeyUp += KeyReleased;
        }
        
        foreach (var mouse in Context.Mice) {
            mouse.MouseDown += MouseButtonPressed;
            mouse.MouseUp += MouseButtonReleased;
        }
    }

    internal static void Update() {
        KeysPressed.Clear();
        KeysReleased.Clear();
        MouseButtonsPressed.Clear();
        MouseButtonsReleased.Clear();
    }

    public static bool IsKeyDown(Key key) => KeysDown.Contains(key);
    public static bool IsKeyPressed(Key key) => KeysPressed.Contains(key);
    public static bool IsKeyReleased(Key key) => KeysReleased.Contains(key);
    
    public static bool IsMouseDown(MouseButton button) => MouseButtonsDown.Contains(button);
    public static bool IsMousePressed(MouseButton button) => MouseButtonsPressed.Contains(button);
    public static bool IsMouseReleased(MouseButton button) => MouseButtonsReleased.Contains(button);
    
    private static void KeyPressed(IKeyboard keyboard, Key key, int keyCode) {
        KeyIsDown?.Invoke(keyboard, key, keyCode);
        KeysPressed.Add(key);
        KeysDown.Add(key);
    }
    
    private static void KeyReleased(IKeyboard keyboard, Key key, int keyCode) {
        KeyIsReleased?.Invoke(keyboard, key, keyCode);
        KeysReleased.Add(key);

        if (IsKeyDown(key)) {
            KeysDown.Remove(key);
        }
    }

    private static void MouseButtonPressed(IMouse mouse, MouseButton button) {
        MouseButtonIsDown?.Invoke(mouse, button);
        MouseButtonsPressed.Add(button);
        MouseButtonsDown.Add(button);
    }
    
    private static void MouseButtonReleased(IMouse mouse, MouseButton button) {
        MouseButtonIsReleased?.Invoke(mouse, button);
        MouseButtonsReleased.Add(button);
        
        if (IsMouseDown(button)) {
            MouseButtonsDown.Remove(button);
        }
    }
    
    private static void MouseMoving(IMouse mouse, Vector2 pos) {
        MouseIsMoving?.Invoke(mouse, pos);
    }
    
    private static void MouseScrolling(IMouse mouse, ScrollWheel scrollWheel) {
        MouseIsScrolling?.Invoke(mouse, scrollWheel);
    }
}