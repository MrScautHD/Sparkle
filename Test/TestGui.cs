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

        TextData textData = new TextData() {
            Font = FontHelper.GetDefault(),
            FontSize = 50,
            Spacing = 4,
            Text = "Sparkle Engine!",
        };
        
        this.AddElement(new LabelElement("label", textData, new Vector2(200, 200)));
        
        ButtonData buttonData = new ButtonData() {
            Texture = this.Content.Load<Texture2D>("icon.png"),
        };
        
        TextData buttonLabelData = textData.Clone();
        buttonLabelData.FontSize = 25;
        
        this.AddElement(new ButtonElement("button", buttonData, buttonLabelData, new Vector2(500, 500), new Vector2(100, 100), () => {
            Logger.Error("BUTTON GET PRESSED!");
            return true;
        }));
        
        //this.AddElement(new ToggleElement("toggle", texture, FontHelper.GetDefault(), "Check", "Checked", 18, new Vector2(500, 400), Color.WHITE, Color.BLACK, null));
    }
}