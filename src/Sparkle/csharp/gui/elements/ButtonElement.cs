using System.Drawing;
using System.Numerics;
using Color = Raylib_cs.Color;

namespace Sparkle.csharp.gui.elements; 

public class ButtonElement : GuiElement {
    
    public ButtonElement(string name, Vector2 position, Size size, Color color, Func<bool> clickClickFunc) : base(name, position, size, color, clickClickFunc) {
        
    }

    protected internal override void Draw() {
        
    }
}