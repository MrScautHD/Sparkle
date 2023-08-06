using System.Numerics;
using Sparkle.csharp.graphics.util;
using Sparkle.csharp.gui.elements.data;
using Color = Raylib_cs.Color;

namespace Sparkle.csharp.gui.elements; 

public class ButtonElement : GuiElement {
    
    public ButtonData ButtonData { get; private set; }
    public LabelData LabelData { get; private set; }
    
    public Color LabelColor;

    public ButtonElement(string name, ButtonData buttonData, LabelData labelData, Vector2 position, Vector2 size, Color color, Color labelColor, Func<bool>? clickClickFunc = null) : base(name, position, size, color, clickClickFunc) {
        this.ButtonData = buttonData;
        this.LabelData = labelData;
        this.LabelColor = labelColor;
    }

    protected internal override void Update() {
        base.Update();
        
        this.Color = this.DefaultColor;
        
        if (this.IsHovered && this.Enabled) {
            this.Color = this.ButtonData.HoverColor;
        }

        if (this.IsClicked && this.Enabled) {
            this.Color = this.ButtonData.PressColor;
        }
    }
    
    protected internal override void Draw() {
        TextureHelper.Draw(this.ButtonData.Texture, this.Position, this.Color);

        Vector2 textPos = new Vector2(this.Position.X + this.Size.X / 2F - this.LabelData.TextSize.X / 2F, this.Position.Y + this.Size.Y / 2F - this.LabelData.TextSize.Y);
        FontHelper.DrawText(this.LabelData.Font, this.LabelData.Text, textPos, Vector2.Zero, 0, this.LabelData.FontSize, this.LabelData.Spacing, this.LabelColor);
    }
}