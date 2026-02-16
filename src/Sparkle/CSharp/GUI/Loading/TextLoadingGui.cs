using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Fonts;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.GUI.Elements;
using Sparkle.CSharp.GUI.Elements.Data;
using Veldrid;

namespace Sparkle.CSharp.GUI.Loading;

public class TextLoadingScreen : LoadingGui {
    
    /// <summary>
    /// The font used to render the loading text.
    /// </summary>
    private Font _font;
    
    /// <summary>
    /// The font size used to render the loading text.
    /// </summary>
    private float _fontSize;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TextLoadingScreen"/> class.
    /// </summary>
    /// <param name="name">The unique name of the loading GUI.</param>
    /// <param name="font">The font used to render the loading text.</param>
    /// <param name="fontSize">The size of the loading text.</param>
    /// <param name="minTime">The minimum amount of time, in seconds, that the loading screen should remain visible.</param>
    /// <param name="size">Optional size of the GUI in pixels as a tuple (width, height). If <c>null</c>, a default size is used.</param>
    public TextLoadingScreen(string name, Font font, float fontSize, float minTime = 0.5F, (int, int)? size = null) : base(name, minTime, size) {
        this._font = font;
        this._fontSize = fontSize;
    }
    
    /// <summary>
    /// Initializes the loading GUI and creates the centered loading label.
    /// </summary>
    protected internal override void Init() {
        base.Init();
        
        LabelData labelData = new LabelData(this._font, "Loading", this._fontSize);
        this.AddElement("loading_label", new LabelElement(labelData, Anchor.Center, Vector2.Zero, new Vector2(5, 5)));
    }
    
    /// <summary>
    /// Renders the loading screen.
    /// </summary>
    /// <param name="context">The graphics context used for rendering operations.</param>
    /// <param name="framebuffer">The framebuffer to which the GUI is rendered.</param>
    protected internal override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        context.CommandList.ClearColorTarget(0, Color.Black.ToRgbaFloat());
        base.Draw(context, framebuffer);
    }
}