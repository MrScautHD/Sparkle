using System.Numerics;
using Sparkle.CSharp;
using Sparkle.CSharp.GUI;
using Sparkle.CSharp.GUI.Elements;
using Sparkle.CSharp.GUI.Elements.Data;
using Sparkle.CSharp.Rendering.Helpers;
using Color = Raylib_cs.Color;

namespace Sparkle.Test; 

public class TestGui : Gui {
    
    public TestGui(string name) : base(name) { }

    protected override void Init() {
        base.Init();

        LabelData labelData = new LabelData() {
            Font = FontHelper.GetDefault(),
            FontSize = 50,
            Spacing = 4,
            Text = "Sparkle Engine!",
            Color = Color.White,
            HoverColor = Color.Gray,
            Rotation = 0
        };
        
        this.AddElement(new LabelElement("label", labelData, Anchor.BottomCenter, Vector2.Zero));
        
        ButtonData buttonData = new ButtonData() {
            Color = Color.Orange
        };
        
        LabelData buttonLabelData = new LabelData() {
            Font = FontHelper.GetDefault(),
            FontSize = 25,
            Spacing = 4,
            Text = "Sparkle Engine!",
            Color = Color.Red,
            HoverColor = Color.Gold
        };
        
        this.AddElement(new ButtonElement("button", buttonData, buttonLabelData, Anchor.Center, Vector2.Zero, new Vector2(300, 300), () => {
            Logger.Error("BUTTON GET PRESSED!");
            return true;
        }));

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