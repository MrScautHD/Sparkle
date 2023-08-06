using System.Numerics;
using Sparkle.csharp.graphics.util;
using Sparkle.csharp.gui.elements.data;
using Color = Raylib_cs.Color;

namespace Sparkle.csharp.gui.elements; 

public class LabelElement : GuiElement {

    public LabelData Data { get; private set; }

    public LabelElement(string name, LabelData data, Vector2 position, Color color, Func<bool>? clickClickFunc = null) : base(name, position, Vector2.Zero, color, clickClickFunc) {
        this.Data = data;
    }

    protected internal override void Update() {
        base.Update();
        this.Size = this.Data.TextSize;
    }

    protected internal override void Draw() {
        FontHelper.DrawText(this.Data.Font, this.Data.Text, this.Position, Vector2.Zero, this.Data.Rotation, this.Data.FontSize, this.Data.Spacing, this.Color);
    }
}