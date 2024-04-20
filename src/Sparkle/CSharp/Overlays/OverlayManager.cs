namespace Sparkle.CSharp.Overlays;

public static class OverlayManager {
    
    internal static List<Overlay> Overlays = new();
    
    /// <summary>
    /// Used for Initializes objects.
    /// </summary>
    internal static void Init() {
        foreach (Overlay overlay in Overlays) {
            if (!overlay.HasInitialized) {
                overlay.Init();
            }
        }
    }
    
    /// <summary>
    /// Is invoked during each tick and is used for updating dynamic elements and game logic.
    /// </summary>
    internal static void Update() {
        foreach (Overlay overlay in Overlays) {
            if (overlay.Enabled) {
                overlay.Update();
            }
        }
    }
    
    /// <summary>
    /// Called after the Update method on each tick to further update dynamic elements and game logic.
    /// </summary>
    internal static void AfterUpdate() {
        foreach (Overlay overlay in Overlays) {
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
        foreach (Overlay overlay in Overlays) {
            if (overlay.Enabled) {
                overlay.FixedUpdate();
            }
        }
    }
    
    /// <summary>
    /// Is called every tick, used for rendering stuff.
    /// </summary>
    internal static void Draw() {
        foreach (Overlay overlay in Overlays) {
            if (overlay.Enabled) {
                overlay.Draw();
            }
        }
    }
    
    /// <summary>
    /// Adds an overlay to the system and logs the addition for tracking purposes.
    /// </summary>
    /// <param name="overlay">The overlay to be added.</param>
    public static void Add(Overlay overlay) {
        if (Overlays.Contains(overlay)) {
            Logger.Warn($"The Overlay [{overlay.Name}] is already present in the OverlayManager!");
            return;
        }
        
        Logger.Info($"Added Overlay: {overlay.Name}");
        Overlays.Add(overlay);
    }
}