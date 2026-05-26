using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.GUI.Loading;
using Veldrith;

namespace Sparkle.CSharp.GUI;

public static class GuiManager {
    
    /// <summary>
    /// The currently active GUI.
    /// </summary>
    public static Gui? ActiveGui => _activeLoadingGui ?? _activeGui;

    /// <summary>
    /// The highest GUI scale step the game allows.
    /// </summary>
    public static int MaxAllowedScaleFactor;
    
    /// <summary>
    /// The selected GUI scale.
    /// 0 means automatic scaling.
    /// 1 to 5 are relative scale steps around the automatic scale.
    /// </summary>
    public static int Scale { get; private set; }
    
    /// <summary>
    /// The maximum GUI scale factor that fits on the current screen for the active GUI.
    /// </summary>
    public static int MaxScaleFactor => ActiveGui?.MaxScaleFactor ?? 1;
    
    /// <summary>
    /// The final GUI scale factor currently used by the active GUI.
    /// </summary>
    public static int ScaleFactor => ActiveGui?.ScaleFactor ?? 1;
    
    /// <summary>
    /// The primary GUI instance currently active.
    /// </summary>
    private static Gui? _activeGui;
    
    /// <summary>
    /// The currently active loading GUI, if one is set.
    /// This is used to manage transitional or temporary GUI states, such as loading screens.
    /// </summary>
    private static LoadingGui? _activeLoadingGui;
    
    /// <summary>
    /// Initializes the GUI manager.
    /// </summary>
    internal static void Init() {
        MaxAllowedScaleFactor = 4;
        Scale = 0;
    }
    
    /// <summary>
    /// Updates the currently active GUI with the specified delta time.
    /// </summary>
    /// <param name="delta">The time, in seconds, since the last update.</param>
    internal static void OnUpdate(double delta) {
        if (ActiveGui != null && ActiveGui.IsInitialized) {
            ActiveGui.Update(delta);
        }
    }
    
    /// <summary>
    /// Executes logic that should happen after the GUI has been updated.
    /// </summary>
    /// <param name="delta">The time, in seconds, since the last update.</param>
    internal static void OnAfterUpdate(double delta) {
        if (ActiveGui != null && ActiveGui.IsInitialized) {
            ActiveGui.AfterUpdate(delta);
        }
    }
    
    /// <summary>
    /// Executes logic that relies on a fixed time step for the currently active GUI.
    /// </summary>
    /// <param name="fixedStep">The fixed time step interval, in seconds, for performing updates.</param>
    internal static void OnFixedUpdate(double fixedStep) {
        if (ActiveGui != null && ActiveGui.IsInitialized) {
            ActiveGui.FixedUpdate(fixedStep);
        }
    }
    
    /// <summary>
    /// Renders the currently active GUI onto the specified framebuffer using the provided graphics context.
    /// </summary>
    /// <param name="context">The graphics context used for rendering operations.</param>
    /// <param name="framebuffer">The framebuffer where the GUI will be drawn.</param>
    internal static void OnDraw(GraphicsContext context, Framebuffer framebuffer) {
        if (ActiveGui != null && ActiveGui.IsInitialized) {
            ActiveGui.Draw(context, framebuffer);
        }
    }
    
    /// <summary>
    /// Adjusts the size of the currently active GUI to fit within the specified rectangle.
    /// </summary>
    /// <param name="rectangle">The new dimensions and position to which the GUI should be resized.</param>
    internal static void OnResize(Rectangle rectangle) {
        if (ActiveGui != null && ActiveGui.IsInitialized) {
            ActiveGui.Resize(rectangle);
        }
    }
    
    /// <summary>
    /// Sets the selected GUI scale step.
    /// </summary>
    /// <param name="scale">The selected scale step. Use 0 for automatic scaling.</param>
    public static void SetScale(int scale) {
        if (scale <= 0) {
            Scale = 0;
            return;
        }
        
        Scale = Math.Clamp(scale, 1, MaxAllowedScaleFactor);
    }
    
    /// <summary>
    /// Gets all selectable GUI scale steps.
    /// </summary>
    /// <returns>An enumerable collection of selectable scale steps.</returns>
    public static IEnumerable<int> GetSelectableScaleSteps() {
        for (int scale = 1; scale <= MaxAllowedScaleFactor; scale++) {
            yield return scale;
        }
    }
    
    /// <summary>
    /// Sets the currently active GUI. Disposes of the previous one, if set, and initializes the new GUI.
    /// </summary>
    /// <param name="gui">The new GUI to set as active, or null to unset the active GUI.</param>
    public static void SetGui(Gui? gui) {
        _activeGui?.Dispose();
        _activeGui = gui;
        gui?.Init();
        gui?.IsInitialized = true;
    }
    
    /// <summary>
    /// Sets the active loading GUI used during scene transitions or other loading processes.
    /// Disposes of the currently active loading GUI, if any, and initializes the new one.
    /// </summary>
    /// <param name="loadingGui">The new loading GUI to be set. Pass null to clear the active loading GUI.</param>
    internal static void SetLoadingGui(LoadingGui? loadingGui) {
        _activeLoadingGui?.Dispose();
        _activeLoadingGui = loadingGui;
        loadingGui?.Init();
        loadingGui?.IsInitialized = true;
    }
    
    /// <summary>
    /// Releases resources associated with the currently active GUI, if any.
    /// </summary>
    internal static void Destroy() {
        ActiveGui?.Dispose();
    }
}