using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Veldrith;

namespace Sparkle.CSharp.ImGUI;

public static class ImGuiOverlayManager {
    
    /// <summary>
    /// List of active overlays.
    /// </summary>
    private static List<ImGuiOverlay> _overlays;
    
    /// <summary>
    /// Stores overlays that will be added to the manager during the next update pass.
    /// </summary>
    private static List<ImGuiOverlay> _overlaysToAdd;
    
    /// <summary>
    /// Stores overlays scheduled for removal during the next update pass.
    /// </summary>
    private static List<ImGuiOverlay> _overlaysToRemove;
    
    /// <summary>
    /// Initializes the overlay manager.
    /// </summary>
    internal static void Init() {
        _overlays = new List<ImGuiOverlay>();
        _overlaysToAdd = new List<ImGuiOverlay>();
        _overlaysToRemove = new List<ImGuiOverlay>();
    }
    
    /// <summary>
    /// Updates all enabled overlays.
    /// </summary>
    /// <param name="delta">Delta time in seconds.</param>
    internal static void OnUpdate(double delta) {
        
        // Handle removing overlays.
        foreach (ImGuiOverlay overlay in _overlaysToRemove) {
            if (_overlays.Remove(overlay)) {
                overlay.Dispose();
            }
        }
        
        _overlaysToRemove.Clear();
        
        // Handle adding overlays.
        foreach (ImGuiOverlay overlay in _overlaysToAdd) {
            _overlays.Add(overlay);
        }
        
        _overlaysToAdd.Clear();
        
        // Update overlays.
        foreach (ImGuiOverlay overlay in _overlays) {
            if (overlay.Enabled) {
                overlay.Update(delta);
            }
        }
    }
    
    /// <summary>
    /// Runs after-update logic for all enabled overlays.
    /// </summary>
    /// <param name="delta">Delta time in seconds.</param>
    internal static void OnAfterUpdate(double delta) {
        foreach (ImGuiOverlay overlay in _overlays) {
            if (overlay.Enabled) {
                overlay.AfterUpdate(delta);
            }
        }
    }
    
    /// <summary>
    /// Runs fixed update logic for all enabled overlays.
    /// </summary>
    /// <param name="fixedStep">Fixed timestep in seconds.</param>
    internal static void OnFixedUpdate(double fixedStep) {
        foreach (ImGuiOverlay overlay in _overlays) {
            if (overlay.Enabled) {
                overlay.FixedUpdate(fixedStep);
            }
        }
    }
    
    /// <summary>
    /// Draws all enabled overlays by rendering each one to a scaled RenderTexture first, then drawing that texture 1:1 onto the final framebuffer.
    /// </summary>
    /// <param name="context">Graphics context.</param>
    /// <param name="framebuffer">Final destination framebuffer.</param>
    internal static void OnDraw(GraphicsContext context, Framebuffer framebuffer) {
        foreach (ImGuiOverlay overlay in _overlays) {
            if (!overlay.Enabled) {
                continue;
            }
            
            Texture2D? texture = overlay.RenderToTexture(context.CommandList, context.ImGuiController);
            
            if (texture == null) {
                continue;
            }
            
            Rectangle sourceRect = new Rectangle(0, 0, (int) texture.Width, (int) texture.Height);
            RectangleF destination = overlay.DestinationRectangle;
            
            context.CommandList.SetFramebuffer(framebuffer);
            context.SpriteBatch.Begin(context.CommandList, framebuffer);
            context.SpriteBatch.DrawTexture(texture, new Vector2(destination.X, destination.Y), 0.5F, sourceRect, Vector2.One, Vector2.Zero, true, 0.0F, Color.White);
            context.SpriteBatch.End();
        }
    }
    
    /// <summary>
    /// Handles window resize events.
    /// </summary>
    /// <param name="rectangle">The new window rectangle.</param>
    internal static void OnResize(Rectangle rectangle) {
        foreach (ImGuiOverlay overlay in _overlays) {
            if (overlay.Enabled) {
                overlay.Resize(rectangle);
            }
        }
    }
    
    /// <summary>
    /// Gets all overlays.
    /// </summary>
    /// <returns>All active overlays.</returns>
    public static IEnumerable<ImGuiOverlay> GetOverlays() {
        return _overlays;
    }
    
    /// <summary>
    /// Adds an overlay.
    /// </summary>
    /// <param name="overlay">The overlay to add.</param>
    public static void AddOverlay(ImGuiOverlay overlay) {
        if (!TryAddOverlay(overlay)) {
            throw new Exception($"The ImGUI overlay: [{overlay.Name}] is already present in the ImGuiOverlayManager!");
        }
    }
    
    /// <summary>
    /// Attempts to add an overlay.
    /// </summary>
    /// <param name="overlay">The overlay to add.</param>
    /// <returns>True if queued successfully.</returns>
    public static bool TryAddOverlay(ImGuiOverlay overlay) {
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
    /// Removes an overlay.
    /// </summary>
    /// <param name="overlay">The overlay to remove.</param>
    public static void RemoveOverlay(ImGuiOverlay overlay) {
        if (!TryRemoveOverlay(overlay)) {
            throw new Exception($"Failed to remove the ImGUI overlay: [{overlay.Name}] from the ImGuiOverlayManager!");
        }
    }
    
    /// <summary>
    /// Attempts to remove an overlay.
    /// </summary>
    /// <param name="overlay">The overlay to remove.</param>
    /// <returns>True if queued successfully.</returns>
    public static bool TryRemoveOverlay(ImGuiOverlay overlay) {
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
    /// Clears all overlays.
    /// </summary>
    internal static void Destroy() {
        HashSet<ImGuiOverlay> disposedOverlays = new HashSet<ImGuiOverlay>();
        
        foreach (ImGuiOverlay overlay in _overlays) {
            if (disposedOverlays.Add(overlay)) {
                overlay.Dispose();
            }
        }
        
        foreach (ImGuiOverlay overlay in _overlaysToAdd) {
            if (disposedOverlays.Add(overlay)) {
                overlay.Dispose();
            }
        }
        
        _overlays.Clear();
        _overlaysToAdd.Clear();
        _overlaysToRemove.Clear();
    }
}