using System.Drawing;
using System.Numerics;
using Raylib_cs;
using Sparkle.csharp;
using Sparkle.csharp.graphics.util;
using Sparkle.csharp.gui;
using Sparkle.csharp.gui.elements;
using Color = Raylib_cs.Color;

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

        Texture2D texture = this.Content.Load<Texture2D>("icon.png");
        
        this.AddElement(new ButtonElement("button", texture, FontHelper.GetDefault(), "Hello", 18, 4, new Vector2(400, 400), new Size(texture.width, texture.height), Color.WHITE, Color.WHITE, () => {
            Logger.Error("Button get clicked!");
            return true;
        }));
    }
}