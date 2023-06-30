using Sparkle.csharp.gui;
using Sparkle.csharp.scene;

namespace Test; 

public class MenuScene : Scene {
    
    public MenuScene(string name) : base(name) {
        
    }

    protected override void Init() {
        TestGui gui = new TestGui("test");
        GUIManager.SetGui(gui);
    }
}