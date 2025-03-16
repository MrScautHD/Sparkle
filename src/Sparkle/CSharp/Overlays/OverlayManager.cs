using Bliss.CSharp.Logging;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;

namespace Sparkle.CSharp.Overlays;

public static class OverlayManager {

    /// <summary>
    /// List of active overlays.
    /// </summary>
    private static List<Overlay> _overlays;

    /// <summary>
    /// Initializes the <see cref="OverlayManager"/>, creating the overlay list.
    /// </summary>
    internal static void Init() {
        _overlays = new List<Overlay>();
    }

    /// <summary>
    /// Updates all enabled overlays.
    /// </summary>
    /// <param name="delta">The time delta since the last update.</param>
    internal static void OnUpdate(double delta) {
        foreach (Overlay overlay in _overlays) {
            if (overlay.Enabled) {
                overlay.Update(delta);
            }
        }
    }
    
    /// <summary>
    /// Executes logic after the update step for all enabled overlays.
    /// </summary>
    /// <param name="delta">The time delta since the last update.</param>
    internal static void OnAfterUpdate(double delta) {
        foreach (Overlay overlay in _overlays) {
            if (overlay.Enabled) {
                overlay.AfterUpdate(delta);
            }
        }
    }
    
    /// <summary>
    /// Executes fixed-step updates for all enabled overlays.
    /// </summary>
    /// <param name="timeStep">The fixed time step interval for the update.</param>
    internal static void OnFixedUpdate(double timeStep) {
        foreach (Overlay overlay in _overlays) {
            if (overlay.Enabled) {
                overlay.FixedUpdate(timeStep);
            }
        }
    }
    
    /// <summary>
    /// Draws all enabled overlays.
    /// </summary>
    /// <param name="context">The graphics context used for rendering.</param>
    internal static void OnDraw(GraphicsContext context) {
        foreach (Overlay overlay in _overlays) {
            if (overlay.Enabled) {
                overlay.Draw(context);
            }
        }
    }

    /// <summary>
    /// Executes when the window is resized.
    /// </summary>
    /// <param name="rectangle">The rectangle specifying the window's updated size.</param>
    internal static void OnResize(Rectangle rectangle) {
        foreach (Overlay overlay in _overlays) {
            if (overlay.Enabled) {
                overlay.Resize(rectangle);
            }
        }
    }

    /// <summary>
    /// Retrieves the list of active overlays managed by the <see cref="OverlayManager"/>.
    /// </summary>
    /// <returns>A collection of overlays currently managed by the <see cref="OverlayManager"/>.</returns>
    public static IEnumerable<Overlay> GetOverlays() {
        return _overlays;
    }

    /// <summary>
    /// Adds an overlay to the manager.
    /// </summary>
    /// <param name="overlay">The overlay to add.</param>
    public static void AddOverlay(Overlay overlay) {
        if (_overlays.Contains(overlay)) {
            Logger.Warn($"The overlay: [{overlay.Name}] is already present in the OverlayManager!");
        }
        else {
            _overlays.Add(overlay);
        }
    }
    
    /// <summary>
    /// Removes an overlay from the manager.
    /// </summary>
    /// <param name="overlay">The overlay to remove.</param>
    public static void RemoveOverlay(Overlay overlay) {
        if (!_overlays.Remove(overlay)) {
            Logger.Warn($"Failed to remove the overlay: [{overlay.Name}] from the OverlayManager!");
        }
    }

    /// <summary>
    /// Clears all overlays from the manager.
    /// </summary>
    internal static void Destroy() {
        _overlays.Clear();
    }
}