namespace Sparkle.csharp.gui; 

public static class GUIManager {
    
    public static GUI? ActiveGui { get; private set; }

    internal static void Init() {
        ActiveGui?.Init();
    }

    internal static void Update() {
        ActiveGui?.Update();
    }

    internal static void Draw() {
        ActiveGui?.Draw();
    }
    
    public static void SetGui(GUI gui) {
        ActiveGui?.Dispose();
        ActiveGui = gui;
        ActiveGui.Init();
    }
}