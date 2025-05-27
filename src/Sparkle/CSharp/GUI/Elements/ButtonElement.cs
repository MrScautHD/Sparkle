using System.Numerics;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Veldrid;

namespace Sparkle.CSharp.GUI.Elements;

public class ButtonElement : GuiElement {
    
    public ButtonElement(Anchor anchor, Vector2 offset, Vector2 size, Vector2? origin = null, float rotation = 0, Func<bool>? clickFunc = null) : base(anchor, offset, size, origin, rotation, clickFunc) {
        
    }
    
    protected internal override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        context.PrimitiveBatch.Begin(context.CommandList, framebuffer.OutputDescription);
        context.PrimitiveBatch.DrawFilledRectangle(new RectangleF(this.Position.X, this.Position.Y, this.ScaledSize.X, this.ScaledSize.Y), this.Origin, this.Rotation);
        context.PrimitiveBatch.End();
    }
}