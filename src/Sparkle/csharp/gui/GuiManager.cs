namespace Sparkle.csharp.gui; 

public static class GuiManager {
    
    public static Gui? ActiveGui { get; private set; }
    public static float Scale { get; private set; } = 1F;
    
    /// <summary>
    /// Is invoked during each tick and is used for updating dynamic elements and game logic.
    /// </summary>
    internal static void Update() {
        ActiveGui?.Update();
    }

    /// <summary>
    /// Called after the Update method on each tick to further update dynamic elements and game logic.
    /// </summary>
    internal static void AfterUpdate() {
        ActiveGui?.AfterUpdate();
    }
    
    /// <summary>
    /// Is invoked at a fixed rate of every <see cref="GameSettings.FixedTimeStep"/> frames following the <see cref="Update"/> method.
    /// It is used for handling physics and other fixed-time operations.
    /// </summary>
    internal static void FixedUpdate() {
        ActiveGui?.FixedUpdate();
    }
    
    /// <summary>
    /// Is called every tick, used for rendering stuff.
    /// </summary>
    internal static void Draw() {
        ActiveGui?.Draw();
    }
    
    /// <summary>
    /// Sets the active GUI and disposes of the previous one.
    /// </summary>
    /// <param name="gui">The GUI to be set as the active GUI.</param>
    public static void SetGui(Gui? gui) {
        ActiveGui?.Dispose();
        ActiveGui = gui;
        
        if (ActiveGui != null && !ActiveGui.HasInitialized) {
            ActiveGui.Init();
        }
    }

    /// <summary>
    /// Sets the application scale within a specified range.
    /// </summary>
    /// <param name="scale">The scale value to be set.</param>
    public static void SetScale(float scale) {
        Scale = Math.Clamp(scale, 0.5F, 1);
    }
}