using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Fonts;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.GUI.Elements;
using Sparkle.CSharp.GUI.Elements.Data;
using Veldrid;

namespace Sparkle.CSharp.GUI.Loading;

public class TextLoadingScreen : LoadingGui {
    
    private Font _font;
    private float _fontSize;
    
    public TextLoadingScreen(string name, Font font, float fontSize, float minTime = 0.5F, (int, int)? size = null) : base(name, minTime, size) {
        this._font = font;
        this._fontSize = fontSize;
    }
    
    protected internal override void Init() {
        base.Init();
        
        LabelData labelData = new LabelData(this._font, "Loading", this._fontSize);
        this.AddElement("loading_label", new LabelElement(labelData, Anchor.Center, Vector2.Zero, new Vector2(5, 5)));
    }
    
    protected internal override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        context.CommandList.ClearColorTarget(0, Color.Black.ToRgbaFloat());
        base.Draw(context, framebuffer);
    }
}