using Bliss.CSharp.Colors;
using Sparkle.CSharp.Graphics;
using Veldrid;

namespace Sparkle.CSharp.GUI.Loading;

public class BlankLoadingGui : LoadingGui {
    
    /// <summary>
    /// Initializes a new instance of the <see cref="BlankLoadingGui"/> class.
    /// </summary>
    /// <param name="name">The unique name of the loading GUI.</param>
    /// <param name="minTime">The minimum amount of time, in seconds, that the loading screen should remain visible.</param>
    /// <param name="size">Optional size of the loading GUI in pixels as a tuple (width, height). If <c>null</c>, a default size is used.</param>
    public BlankLoadingGui(string name, float minTime = 0.5F, (int, int)? size = null) : base(name, minTime, size) { }
    
    /// <summary>
    /// Renders the loading GUI.
    /// </summary>
    /// <param name="context">The graphics context used for rendering operations.</param>
    /// <param name="framebuffer">The framebuffer to which the GUI is rendered.</param>
    protected internal override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        context.CommandList.ClearColorTarget(0, Color.Black.ToRgbaFloat());
        base.Draw(context, framebuffer);
    }
}