using System.Numerics;
using Sparkle.CSharp.Graphics;
using Veldrid;

namespace Sparkle.CSharp.GUI.Elements;

public class RectangleTextBox : GuiElement {
    
    public RectangleTextBox(Anchor anchor, Vector2 offset, Vector2 size, Vector2? origin = null, float rotation = 0, Func<bool>? clickFunc = null) : base(anchor, offset, size, origin, rotation, clickFunc) { }

    protected internal override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        
    }
}