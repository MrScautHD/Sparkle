using Bliss.CSharp.Colors;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.Scenes;
using Veldrid;

namespace Sparkle.ImGUI.Scenes;

public class MainScene() : Scene("MainScene", SceneType.Scene2D)
{
    public RectangleF Rectangle = new(10, 10, 256, 128);
    
    protected override void Draw(GraphicsContext context, Framebuffer framebuffer)
    {
        base.Draw(context, framebuffer);
        context.PrimitiveBatch.DrawEmptyRectangle(Rectangle, 1f, color: Color.White);
    }
}