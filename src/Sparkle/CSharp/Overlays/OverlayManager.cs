using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Veldrid;

namespace Sparkle.CSharp.Overlays;

public static class OverlayManager {
    
    /// <summary>
    /// List of active overlays.
    /// </summary>
    private static List<Overlay> _overlays;
    
    /// <summary>
    /// Stores overlays that will be added to the manager during the next update pass.
    /// </summary>
    private static List<Overlay> _overlaysToAdd;
    
    /// <summary>
    /// Stores the Overlays scheduled for removal during the next update pass.
    /// </summary>
    private static List<Overlay> _overlaysToRemove;
    
    /// <summary>
    /// Initializes the <see cref="OverlayManager"/>, creating the overlay list.
    /// </summary>
    internal static void Init() {
        _overlays = new List<Overlay>();
        _overlaysToAdd = new List<Overlay>();
        _overlaysToRemove = new List<Overlay>();
    }
    
    /// <summary>
    /// Updates all enabled overlays.
    /// </summary>
    /// <param name="delta">The time delta since the last update.</param>
    internal static void OnUpdate(double delta) {
        
        // Handle adding overlays.
        foreach (Overlay overlay in _overlaysToAdd) {
            _overlays.Add(overlay);
        }
        
        _overlaysToAdd.Clear();
        
        // Handle removing overlays.
        foreach (Overlay overlay in _overlaysToRemove) {
            _overlays.Remove(overlay);
        }
        
        _overlaysToRemove.Clear();
        
        // Update overlays.
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
    /// Executes fixed-step logic for all enabled overlays.
    /// </summary>
    /// <param name="fixedStep">The fixed time step duration.</param>
    internal static void OnFixedUpdate(double fixedStep) {
        foreach (Overlay overlay in _overlays) {
            if (overlay.Enabled) {
                overlay.FixedUpdate(fixedStep);
            }
        }
    }
    
    /// <summary>
    /// Draws all active overlays using the provided graphics context and framebuffer.
    /// </summary>
    /// <param name="context">The graphics context used for rendering operations.</param>
    /// <param name="framebuffer">The framebuffer to which the overlays are rendered.</param>
    internal static void OnDraw(GraphicsContext context, Framebuffer framebuffer) {
        foreach (Overlay overlay in _overlays) {
            if (overlay.Enabled) {
                overlay.Draw(context, framebuffer);
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
        if (!TryAddOverlay(overlay)) {
            throw new Exception($"The overlay: [{overlay.Name}] is already present in the OverlayManager!");
        }
    }
    
    /// <summary>
    /// Attempts to add the specified overlay to the list of overlays to be added during the next update pass.
    /// Ensures the overlay is not already in the manager or queued for addition.
    /// </summary>
    /// <param name="overlay">The overlay to be added.</param>
    /// <returns>True if the overlay was successfully queued for addition; otherwise, false.</returns>
    public static bool TryAddOverlay(Overlay overlay) {
        if (_overlaysToAdd.Contains(overlay)) {
            return false;
        }
        
        if (_overlays.Contains(overlay)) {
            return false;
        }
        
        _overlaysToAdd.Add(overlay);
        return true;
    }
    
    /// <summary>
    /// Removes an overlay from the manager.
    /// </summary>
    /// <param name="overlay">The overlay to remove.</param>
    public static void RemoveOverlay(Overlay overlay) {
        if (!TryRemoveOverlay(overlay)) {
            throw new Exception($"Failed to remove the overlay: [{overlay.Name}] from the OverlayManager!");
        }
    }
    
    /// <summary>
    /// Attempts to schedule the specified overlay for removal from the active overlay list.
    /// </summary>
    /// <param name="overlay">The overlay instance to be removed.</param>
    /// <returns>
    /// Returns <c>true</c> if the overlay was successfully scheduled for removal; otherwise, <c>false</c>.
    /// This can happen if the overlay is not in the active overlay list or is already scheduled for removal.
    /// </returns>
    public static bool TryRemoveOverlay(Overlay overlay) {
        if (!_overlays.Contains(overlay)) {
            return false;
        }
        
        if (_overlaysToRemove.Contains(overlay)) {
            return false;
        }
        
        _overlaysToRemove.Add(overlay);
        return true;
    }
    
    /// <summary>
    /// Clears all overlays from the manager.
    /// </summary>
    internal static void Destroy() {
        _overlays.Clear();
    }
}