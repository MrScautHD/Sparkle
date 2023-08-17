using System.Numerics;
using Raylib_cs;
using Sparkle.csharp;
using Sparkle.csharp.graphics.util;
using Sparkle.csharp.gui;
using Sparkle.csharp.gui.elements;
using Sparkle.csharp.gui.elements.data;
using Color = Raylib_cs.Color;

namespace Test; 

public class TestGui : Gui {
    
    public TestGui(string name) : base(name) {
        
    }

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
        
        this.AddElement(new LabelElement("label", labelData, new Vector2(200, 200)));

        ButtonData buttonData = new ButtonData() {
            Texture = this.Content.Load<Texture2D>("icon.png"),
            Rotation = 60,
        };
        
        LabelData buttonLabelData = new LabelData() {
            Font = FontHelper.GetDefault(),
            FontSize = 25,
            Spacing = 4,
            Text = "Sparkle Engine!",
            Color = Color.RED
        };
        
        this.AddElement(new ButtonElement("button", buttonData, buttonLabelData, new Vector2(500, 400), new Vector2(300, 300), () => {
            Logger.Error("BUTTON GET PRESSED!");
            return true;
        }));

        ToggleData toggleData = new ToggleData() {
            ToggledText = "Checked"
        };
        
        LabelData toggleLabelData = new LabelData() {
            Font = FontHelper.GetDefault(),
            FontSize = 25,
            Spacing = 4,
            Text = "Check"
        };
        
        this.AddElement(new ToggleElement("toggle", toggleData, toggleLabelData, new Vector2(300, 300), new Vector2(100, 100)));
    }
}