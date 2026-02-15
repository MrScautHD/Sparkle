using Bliss.CSharp.Colors;
using Sparkle.CSharp.Graphics;
using Veldrid;

namespace Sparkle.CSharp.GUI.Loading;

public class BlankLoadingGui : LoadingGui {

    public BlankLoadingGui(string name, float minTime = 0.5F, (int, int)? size = null) : base(name, minTime, size) { }
    
    protected internal override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        context.CommandList.ClearColorTarget(0, Color.Black.ToRgbaFloat());
        base.Draw(context, framebuffer);
    }
}