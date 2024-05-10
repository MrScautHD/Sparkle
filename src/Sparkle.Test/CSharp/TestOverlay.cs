using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;
using Sparkle.CSharp.Overlays;

namespace Sparkle.Test.CSharp;

public class TestOverlay : Overlay {

    public TestOverlay(string name) : base(name) {
        
    }

    protected override void Draw() {
        Graphics.DrawCircle(70, 70, 30, Color.Red);
    }
}