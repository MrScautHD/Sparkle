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
/*
        ButtonData buttonData = new ButtonData() {
            //Texture = Game.Instance.Content.Load<Texture2D>("icon.png")
        };
        
        LabelData buttonLabelData = new LabelData() {
            Font = FontHelper.GetDefault(),
            FontSize = 25,
            Spacing = 4,
            Text = "Sparkle Engine!",
            Color = Color.RED
        };
        
        this.AddElement(new ButtonElement("button", buttonData, buttonLabelData, Anchor.Center, Vector2.Zero, new Vector2(300, 300), () => {
            Logger.Error("BUTTON GET PRESSED!");
            return true;
        }));*/

/*
        ToggleData toggleData = new ToggleData() {
            ToggledText = "Checked"
        };
        
        LabelData toggleLabelData = new LabelData() {
            Font = FontHelper.GetDefault(),
            FontSize = 25,
            Spacing = 4,
            Text = "Check",
            Color = Color.RED
        };

        this.AddElement(new ToggleElement("toggle", toggleData, toggleLabelData, Anchor.Center, Vector2.Zero, new Vector2(100, 100)));*/
    }

    protected override void Update() {
        base.Update();
        //Vector2 pos = new Vector2((this.Window.GetRenderWidth() - 100F) / 2F, (this.Window.GetRenderHeight() - 100F) / 2F);

        //this.GetElement("toggle").Position = pos;
    }
}