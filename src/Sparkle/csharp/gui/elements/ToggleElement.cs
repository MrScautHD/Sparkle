using System.Numerics;
using Sparkle.csharp.graphics.util;
using Sparkle.csharp.gui.elements.data;

namespace Sparkle.csharp.gui.elements; 

public class ToggleElement : GuiElement {
    
    public ToggleData ToggleData { get; private set; }
    public LabelData LabelData { get; private set; }
    
    public bool IsToggled { get; private set; }
    
    public ToggleElement(string name, ToggleData toggleData, LabelData labelData, Vector2 position, Vector2 size, Func<bool>? clickClickFunc = null) : base(name, position, size, clickClickFunc) {
        this.ToggleData = toggleData;
        this.LabelData = labelData;
    }

    protected internal override void Update() {
        base.Update();
        
        this.ToggleData.Color = this.ToggleData.DefaultColor;
        this.LabelData.Color = this.LabelData.DefaultColor;
        
        if (this.IsToggled && this.Enabled) {
            //this.ToggleData.Color = this.ToggleData.HoverColor;
            this.LabelData.Color = this.LabelData.HoverColor;
        }
    }
    
    protected internal override void Draw() {
        TextureHelper.Draw(this.ToggleData.Texture, this.Position, this.ToggleData.Color);

        Vector2 textPos = new Vector2(this.Position.X + this.Size.X / 2F - this.LabelData.Size.X / 2F, this.Position.Y + this.Size.Y / 2F - this.LabelData.Size.Y);
        FontHelper.DrawText(this.LabelData.Font, this.LabelData.Text, textPos, Vector2.Zero, 0, this.LabelData.FontSize, this.LabelData.Spacing, this.LabelData.Color);
    }
}