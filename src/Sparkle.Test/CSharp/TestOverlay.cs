using System.Numerics;
using Bliss.CSharp.Fonts;
using Sparkle.CSharp;
using Sparkle.CSharp.Content.Types;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.Overlays;

namespace Sparkle.Test.CSharp;

public class TestOverlay : Overlay {

    public Font Font;
    
    public TestOverlay(string name, bool enabled = false) : base(name, enabled) {
        this.Font = Game.Instance.Content.Load(new FontContent("content/fontoe.ttf"));
    }

    protected override void Draw(GraphicsContext context) {
        context.SpriteBatch.Begin(context.CommandList);
        context.SpriteBatch.DrawText(this.Font, $"FPS: {(int) (1.0F / Time.Delta)}", new Vector2(10, 10), 18);
        context.SpriteBatch.End();
    }
}