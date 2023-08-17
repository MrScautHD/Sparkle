using System.Numerics;
using Sparkle.csharp.graphics.util;
using Sparkle.csharp.gui.elements.data;
using Raylib_cs;
namespace Sparkle.csharp.gui.elements;

public class LabelElement : GuiElement {
    
    public TextData textData { get; private set; }
    
    public LabelElement(string name, TextData data, Vector2 position, Func<bool>? clickClickFunc = null) 
        : base(
            name,
            new GUIElementData() 
            {  
                Position = position, 
                Size = data.Size 
            }, clickClickFunc) 
    {
        this.textData = data;
    }

    protected internal override void Update() {
        base.Update();
    }

    protected internal override void Draw() {
        FontHelper.DrawText(
            textData.Font, 
            textData.Text, 
            elementData.Position, 
            Vector2.Zero, 
            elementData.Rotation, 
            textData.FontSize, 
            textData.Spacing, 
            IsHovered ? elementData.HoverColor: elementData.Color);
    }
}
