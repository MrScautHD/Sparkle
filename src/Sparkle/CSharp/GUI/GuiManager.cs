using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Veldrid;

namespace Sparkle.CSharp.GUI;

public static class GuiManager {
    
    /// <summary>
    /// The currently active GUI.
    /// </summary>
    public static Gui? ActiveGui { get; private set; }
    
    /// <summary>
    /// A scaling factor applied to the GUI.
    /// </summary>
    public static float Scale;

    /// <summary>
    /// Retrieves the current scale factor for the GUI.
    /// </summary>
    /// <returns>The computed scale factor.</returns>
    public static float ScaleFactor => Scale * ((float) GlobalGraphicsAssets.Window.GetWidth() / (float) GlobalGraphicsAssets.Window.GetHeight());
    
    /// <summary>
    /// Initializes the GUI manager.
    /// </summary>
    internal static void Init() {
        Scale = 1.0F;
    }
    
    /// <summary>
    /// Updates the currently active GUI with the specified delta time.
    /// </summary>
    /// <param name="delta">The time, in seconds, since the last update.</param>
    internal static void OnUpdate(double delta) {
        ActiveGui?.Update(delta);
    }
    
    /// <summary>
    /// Executes logic that should happen after the GUI has been updated.
    /// </summary>
    /// <param name="delta">The time, in seconds, since the last update.</param>
    internal static void OnAfterUpdate(double delta) {
        ActiveGui?.AfterUpdate(delta);
    }
    
    /// <summary>
    /// Executes logic that relies on a fixed time step for the currently active GUI.
    /// </summary>
    /// <param name="fixedStep">The fixed time step interval, in seconds, for performing updates.</param>
    internal static void OnFixedUpdate(double fixedStep) {
        ActiveGui?.FixedUpdate(fixedStep);
    }
    
    /// <summary>
    /// Renders the currently active GUI onto the specified framebuffer using the provided graphics context.
    /// </summary>
    /// <param name="context">The graphics context used for rendering operations.</param>
    /// <param name="framebuffer">The framebuffer where the GUI will be drawn.</param>
    internal static void OnDraw(GraphicsContext context, Framebuffer framebuffer) {
        ActiveGui?.Draw(context, framebuffer);
    }
    
    /// <summary>
    /// Adjusts the size of the currently active GUI to fit within the specified rectangle.
    /// </summary>
    /// <param name="rectangle">The new dimensions and position to which the GUI should be resized.</param>
    internal static void OnResize(Rectangle rectangle) {
        ActiveGui?.Resize(rectangle);
    }
    
    /// <summary>
    /// Sets the currently active GUI. Disposes of the previous one, if set, and initializes the new GUI.
    /// </summary>
    /// <param name="gui">The new GUI to set as active, or null to unset the active GUI.</param>
    public static void SetGui(Gui? gui) {
        ActiveGui?.Dispose();
        ActiveGui = gui;
        gui?.Init();
    }

    /// <summary>
    /// Releases resources associated with the currently active GUI, if any.
    /// </summary>
    internal static void Destroy() {
        ActiveGui?.Dispose();
    }
}