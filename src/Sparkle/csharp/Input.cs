using System.Numerics;
using Silk.NET.Input;

namespace Sparkle.csharp; 

public static class Input {

    public static IInputContext Context => Application.Instance.InputContext;
    
    private static HashSet<Key> KeysDown = new();
    private static HashSet<Key> KeysPressed = new();
    private static HashSet<Key> KeysReleased = new();
    
    private static HashSet<MouseButton> MouseButtonsDown = new();
    private static HashSet<MouseButton> MouseButtonsPressed = new();
    private static HashSet<MouseButton> MouseButtonsReleased = new();

    public delegate void OnMouseMove(IMouse mouse, Vector2 pos);
    public delegate void OnMouseScrolling(IMouse mouse, ScrollWheel scrollWheel);
    
    public static event OnMouseMove? MouseIsMoving;
    public static event OnMouseScrolling? MouseIsScrolling;

    internal static void Init() {
        foreach (var keyboard in Context.Keyboards) {
            keyboard.KeyDown += KeyPressed;
            keyboard.KeyUp += KeyReleased;
        }
        
        foreach (var mouse in Context.Mice) {
            mouse.MouseDown += MousePressed;
            mouse.MouseUp += MouseReleased;
            mouse.MouseMove += MouseMoving;
            mouse.Scroll += MouseScrolling;
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
        KeysPressed.Add(key);
        KeysDown.Add(key);
    }
    
    private static void KeyReleased(IKeyboard keyboard, Key key, int keyCode) {
        KeysReleased.Add(key);

        if (IsKeyDown(key)) {
            KeysDown.Remove(key);
        }
    }

    private static void MousePressed(IMouse mouse, MouseButton button) {
        MouseButtonsPressed.Add(button);
        MouseButtonsDown.Add(button);
    }
    
    private static void MouseReleased(IMouse mouse, MouseButton button) {
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