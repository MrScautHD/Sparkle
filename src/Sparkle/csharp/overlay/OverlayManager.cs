namespace Sparkle.csharp.overlay; 

public static class OverlayManager {
    
    private static readonly List<Overlay> _overlays = new();
    
    /// <summary>
    /// Retrieves a list of registered overlays for use in the system.
    /// </summary>
    /// <returns>A list of registered Overlay objects.</returns>
    public static List<Overlay> GetOverlays() {
        return _overlays;
    }
    
    /// <summary>
    /// Adds an overlay to the system and logs the addition for tracking purposes.
    /// </summary>
    /// <param name="overlay">The overlay to be added.</param>
    public static void AddOverlay(Overlay overlay) {
        Logger.Info($"Added Overlay: {overlay.Name}");
        _overlays.Add(overlay);
    }
    
    /// <summary>
    /// Used for Initializes objects.
    /// </summary>
    internal static void Init() {
        foreach (Overlay overlay in GetOverlays()) {
            if (!overlay.HasInitialized) {
                overlay.Init();
            }
        }
    }
    
    /// <summary>
    /// Is invoked during each tick and is used for updating dynamic elements and game logic.
    /// </summary>
    internal static void Update() {
        foreach (Overlay overlay in GetOverlays()) {
            if (overlay.Enabled) {
                overlay.Update();
            }
        }
    }
    
    /// <summary>
    /// Called after the Update method on each tick to further update dynamic elements and game logic.
    /// </summary>
    internal static void AfterUpdate() {
        foreach (Overlay overlay in GetOverlays()) {
            if (overlay.Enabled) {
                overlay.AfterUpdate();
            }
        }
    }
    
    /// <summary>
    /// Is invoked at a fixed rate of every <see cref="GameSettings.FixedTimeStep"/> frames following the <see cref="AfterUpdate"/> method.
    /// It is used for handling physics and other fixed-time operations.
    /// </summary>
    internal static void FixedUpdate() {
        foreach (Overlay overlay in GetOverlays()) {
            if (overlay.Enabled) {
                overlay.FixedUpdate();
            }
        }
    }
    
    /// <summary>
    /// Is called every tick, used for rendering stuff.
    /// </summary>
    internal static void Draw() {
        foreach (Overlay overlay in GetOverlays()) {
            if (overlay.Enabled) {
                overlay.Draw();
            }
        }
    }
}