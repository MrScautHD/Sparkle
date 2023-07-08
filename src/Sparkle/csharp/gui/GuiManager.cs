namespace Sparkle.csharp.gui; 

public static class GuiManager {
    
    public static Gui? ActiveGui { get; private set; }

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
        ActiveGui?.Init();
    }
}