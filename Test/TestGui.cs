using System.Numerics;
using Raylib_cs;
using Sparkle.csharp;
using Sparkle.csharp.graphics.util;
using Sparkle.csharp.gui;
using Sparkle.csharp.gui.elements;

namespace Test; 

public class TestGui : Gui {
    
    public TestGui(string name) : base(name) {
        
    }

    protected override void Init() {
        base.Init();

        Font font = FontHelper.GetDefault();

        Logger.Error("ADDED LABEL!");
        this.AddElement(new LabelElement("label", font, "SPARKLE ENGINE!", 50, new Vector2(50, 50), () => {
            Logger.Error("HELLO");
            return true;
        }));
    }
}