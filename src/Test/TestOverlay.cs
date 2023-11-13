using Raylib_cs;
using Sparkle.csharp.graphics.helper;
using Sparkle.csharp.overlay;

namespace Test; 

public class TestOverlay : Overlay {

    public TestOverlay(string name) : base(name) {
        
    }

    protected override void Draw() {
        ShapeHelper.DrawCircle(70, 70, 30, Color.RED);
    }
}