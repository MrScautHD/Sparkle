using System.Numerics;
using Sparkle.csharp.graphics.util;
using Sparkle.csharp.gui.elements.data;

namespace Sparkle.csharp.gui.elements; 

public class ButtonElement : GuiElement {
    
    public ButtonData ButtonData { get; private set; }
    public LabelData LabelData { get; private set; }
    
    public ButtonElement(string name, ButtonData buttonData, LabelData labelData, Vector2 position, Vector2 size, Func<bool>? clickClickFunc = null) : base(name, position, size, clickClickFunc) {
        this.ButtonData = buttonData;
        this.LabelData = labelData;
    }

    protected internal override void Update() {
        base.Update();
        
        this.ButtonData.Color = this.ButtonData.DefaultColor;
        this.LabelData.Color = this.LabelData.DefaultColor;
        
        if (this.IsHovered && this.Enabled) {
            this.ButtonData.Color = this.ButtonData.HoverColor;
            this.LabelData.Color = this.LabelData.HoverColor;
        }
    }
    
    protected internal override void Draw() {
        TextureHelper.Draw(this.ButtonData.Texture, this.Position, this.ButtonData.Color);

        Vector2 textPos = new Vector2(this.Position.X + this.Size.X / 2F - this.LabelData.Size.X / 2F, this.Position.Y + this.Size.Y / 2F - this.LabelData.Size.Y);
        FontHelper.DrawText(this.LabelData.Font, this.LabelData.Text, textPos, Vector2.Zero, 0, this.LabelData.FontSize, this.LabelData.Spacing, this.LabelData.Color);
    }
}