using System.Numerics;
using Sparkle.csharp.graphics.util;
using Sparkle.csharp.gui.elements.data;

namespace Sparkle.csharp.gui.elements; 

public class ButtonElement : GuiElement {
    
    public ButtonData ButtonData { get; private set; }
    public TextData textData { get; private set; }
    
    public ButtonElement(string name, ButtonData buttonData, TextData labelData, Vector2 position, Vector2 size, Func<bool>? clickClickFunc = null) 
        : base(name, 
            new GUIElementData() 
            { 
                Size = size, 
                Position = position
            }, clickClickFunc) 
    {
        this.ButtonData = buttonData;
        this.textData = labelData;
    }

    protected internal override void Update() {
        base.Update();
        
    }
    
    protected internal override void Draw() {
        TextureHelper.Draw(this.ButtonData.Texture, this.elementData.Position, this.IsHovered ? this.elementData.HoverColor :this.elementData.Color);

        Vector2 textPos = new Vector2(this.elementData.Position.X + this.elementData.Size.X / 2F - this.textData.Size.X / 2F, this.elementData.Position.Y + this.elementData.Size.Y / 2F - this.textData.Size.Y);
        FontHelper.DrawText(this.textData.Font, this.textData.Text, textPos, Vector2.Zero, 0, this.textData.FontSize, this.textData.Spacing, this.IsHovered ? this.elementData.HoverColor : this.elementData.Color);
    }
}