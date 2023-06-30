using System.Drawing;
using System.Numerics;

namespace Sparkle.csharp.gui.elements; 

public class BoxElement : GUIElement {
    
    public BoxElement(string name, Vector2 position, Size size, Func<bool> clickFunc) : base(name, position, size, clickFunc) {
        
    }

    protected internal override void Draw() {
        throw new NotImplementedException();
    }
}