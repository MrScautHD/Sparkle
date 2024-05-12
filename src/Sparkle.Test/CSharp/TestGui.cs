using System.Numerics;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Fonts;
using Sparkle.CSharp;
using Sparkle.CSharp.GUI;
using Sparkle.CSharp.GUI.Elements;
using Sparkle.CSharp.GUI.Elements.Data;

namespace Sparkle.Test.CSharp;

public class TestGui : Gui {
    
    public TestGui(string name) : base(name) { }

    protected override void Init() {
        base.Init();

        LabelData labelData = new LabelData() {
            Font = Font.GetDefault(),
            FontSize = 50,
            Spacing = 4,
            Text = "Sparkle Engine!",
            Color = Color.White,
            HoverColor = Color.Gray,
            Rotation = 0
        };
        this.AddElement(new LabelElement("label", labelData, Anchor.BottomCenter, Vector2.Zero));
        
        ButtonData buttonData = new ButtonData() {
            Texture = TestGame.SpriteTexture,
            Color = Color.Orange
        };
        
        LabelData buttonLabelData = new LabelData() {
            Font = Font.GetDefault(),
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

        ToggleData toggleData = new ToggleData() {
            ToggledText = "Checked",
            ToggledColor = Color.Green,
            ToggledTextColor = Color.DarkGreen,
            Rotation = 45
        };
        
        LabelData toggleLabelData = new LabelData() {
            Font = Font.GetDefault(),
            FontSize = 25,
            Spacing = 4,
            Text = "Check",
            Color = Color.Red,
            HoverColor = Color.Gold
        };

        this.AddElement(new ToggleElement("toggle", toggleData, toggleLabelData, Anchor.TopCenter, Vector2.Zero, new Vector2(100, 100)));
    }
}