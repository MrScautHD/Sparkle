using System.Numerics;
using Sparkle.csharp.graphics.util;
using Sparkle.csharp.gui.elements.data;

namespace Sparkle.csharp.gui.elements; 

public class LabelElement : GuiElement {
    
    public LabelData Data { get; private set; }
    
    public LabelElement(string name, LabelData data, Vector2 position, Func<bool>? clickClickFunc = null) : base(name, position, data.Size, clickClickFunc) {
        this.Data = data;
    }

    protected internal override void Update() {
        base.Update();
        
        this.Size = this.Data.Size;
        
        this.Data.Color = this.Data.DefaultColor;
        
        if (this.IsHovered && this.Enabled) {
            this.Data.Color = this.Data.HoverColor;
        }
    }

    protected internal override void Draw() {
        FontHelper.DrawText(this.Data.Font, this.Data.Text, this.Position, Vector2.Zero, this.Data.Rotation, this.Data.FontSize, this.Data.Spacing, this.Data.Color);
    }
}