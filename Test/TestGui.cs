using System.Drawing;
using System.Numerics;
using Sparkle.csharp;
using Sparkle.csharp.gui;
using Sparkle.csharp.gui.elements;

namespace Test; 

public class TestGui : GUI {
    
    public TestGui(string name) : base(name) {
        
    }

    protected override void Init() {
        base.Init();

        this.AddElement(new BoxElement("box", new Vector2(1, 1), new Size(10, 10), () => {
            Logger.Error("Write your click code here!");
            
            return true; // If you return "false" you cancel the click!
        }));
    }
}