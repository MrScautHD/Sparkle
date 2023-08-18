namespace Sparkle.csharp.gui; 

public static class GuiManager {
    
    public static Gui? ActiveGui { get; private set; }
    public static float Scale { get; private set; } = 1F;
    
    internal static void Update() {
        ActiveGui?.Update();
    }
    
    internal static void FixedUpdate() {
        ActiveGui?.FixedUpdate();
    }
    
    internal static void Draw() {
        ActiveGui?.Draw();
    }
    
    public static void SetGui(Gui? gui) {
        ActiveGui?.Dispose();
        ActiveGui = gui;
        
        if (ActiveGui != null && !ActiveGui.HasInitialized) {
            ActiveGui.Init();
        }
    }

    public static void SetScale(float scale) {
        Scale = Math.Clamp(scale, 0.5F, 1);
    }
}