using System.Numerics;
using Sparkle.CSharp;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.Overlays;

namespace Sparkle.Test.CSharp;

public class TestOverlay : Overlay {
    
    public TestOverlay(string name, bool enabled = false) : base(name, enabled) { }

    protected override void Draw(GraphicsContext context) {
        context.SpriteBatch.Begin(context.CommandList, context.Framebuffer.OutputDescription);
        context.SpriteBatch.DrawText(ContentRegistry.Fontoe, $"FPS: {(int) (1.0F / Time.Delta)}", new Vector2(10, 10), 18);
        context.SpriteBatch.End();
    }
}