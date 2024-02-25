using Raylib_cs;
using Sparkle.CSharp.Overlays;
using Sparkle.CSharp.Rendering.Helpers;

namespace Sparkle.Test; 

public class TestOverlay : Overlay {

    public TestOverlay(string name) : base(name) {
        
    }

    protected override void Draw() {
        ShapeHelper.DrawCircle(70, 70, 30, Color.Red);
    }
}