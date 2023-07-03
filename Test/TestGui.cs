using System.Numerics;
using Raylib_cs;
using Sparkle.csharp;
using Sparkle.csharp.gui;
using Sparkle.csharp.gui.elements;
using Color = Raylib_cs.Color;

namespace Test; 

public class TestGui : Gui {
    
    public TestGui(string name) : base(name) {
        
    }

    protected override void Init() {
        base.Init();

        Font font = this.Content.Load<Font>("....");

        this.AddElement(new LabelElement("label", font, "HII", 18, Vector2.One, () => {
            Logger.Error("HELLO");
            return true;
        }));
    }
}