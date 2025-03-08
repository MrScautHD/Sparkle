using Bliss.CSharp.Logging;
using Sparkle.CSharp.Graphics;

namespace Sparkle.CSharp.Overlays;

public static class OverlayManager {

    /// <summary>
    /// List of active overlays.
    /// </summary>
    internal static List<Overlay> Overlays;

    /// <summary>
    /// Initializes the <see cref="OverlayManager"/>, creating the overlay list.
    /// </summary>
    internal static void Init() {
        Overlays = new List<Overlay>();
    }

    /// <summary>
    /// Updates all enabled overlays.
    /// </summary>
    internal static void Update() {
        foreach (Overlay overlay in Overlays) {
            if (overlay.Enabled) {
                overlay.Update();
            }
        }
    }
    
    /// <summary>
    /// Executes logic after the update step for all enabled overlays.
    /// </summary>
    internal static void AfterUpdate() {
        foreach (Overlay overlay in Overlays) {
            if (overlay.Enabled) {
                overlay.AfterUpdate();
            }
        }
    }
    
    /// <summary>
    /// Executes fixed-step updates for all enabled overlays.
    /// </summary>
    internal static void FixedUpdate() {
        foreach (Overlay overlay in Overlays) {
            if (overlay.Enabled) {
                overlay.FixedUpdate();
            }
        }
    }
    
    /// <summary>
    /// Draws all enabled overlays.
    /// </summary>
    /// <param name="context">The graphics context used for rendering.</param>
    internal static void Draw(GraphicsContext context) {
        foreach (Overlay overlay in Overlays) {
            if (overlay.Enabled) {
                overlay.Draw(context);
            }
        }
    }

    /// <summary>
    /// Adds an overlay to the manager.
    /// </summary>
    /// <param name="overlay">The overlay to add.</param>
    public static void Add(Overlay overlay) {
        if (Overlays.Contains(overlay)) {
            Logger.Warn($"The overlay [{overlay.Name}] is already present in the OverlayManager!");
        }
        else {
            Logger.Info($"Added overlay successfully: [{overlay.Name}]");
            Overlays.Add(overlay);
        }
    }

    /// <summary>
    /// Removes an overlay from the manager.
    /// </summary>
    /// <param name="overlay">The overlay to remove.</param>
    public static void Remove(Overlay overlay) {
        if (Overlays.Contains(overlay)) {
            Overlays.Remove(overlay);
        }
        else {
            Logger.Warn($"Failed to remove the overlay [{overlay.Name}] from the OverlayManager!");
        }
    }

    /// <summary>
    /// Clears all overlays from the manager.
    /// </summary>
    internal static void Destroy() {
        Overlays.Clear();
    }
}