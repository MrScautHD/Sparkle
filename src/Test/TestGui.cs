using System.Numerics;
using Raylib_cs;
using Sparkle.csharp;
using Sparkle.csharp.graphics.helper;
using Sparkle.csharp.gui;
using Sparkle.csharp.gui.element;
using Sparkle.csharp.gui.element.data;
using Sparkle.csharp.window;
using Color = Raylib_cs.Color;

namespace Test; 

public class TestGui : Gui {
    
    public TestGui(string name) : base(name) { }

    protected override void Init() {
        base.Init();

        LabelData labelData = new LabelData() {
            Font = FontHelper.GetDefault(),
            FontSize = 50,
            Spacing = 4,
            Text = "Sparkle Engine!",
            Color = Color.WHITE,
            HoverColor = Color.GRAY,
            Rotation = 0
        };
        
        this.AddElement(new LabelElement("label", labelData, Anchor.Center, Vector2.Zero));
    }

    protected override void Update() {
        base.Update();

        this.GetElement("label"); // TODO: cast it later cuz I am lazy and I made scaut speak german :P
    }
}