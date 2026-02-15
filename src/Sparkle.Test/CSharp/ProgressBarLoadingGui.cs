using System.Numerics;
using Bliss.CSharp.Colors;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.GUI;
using Sparkle.CSharp.GUI.Elements;
using Sparkle.CSharp.GUI.Elements.Data;
using Sparkle.CSharp.GUI.Loading;
using Veldrid;

namespace Sparkle.Test.CSharp;

public class ProgressBarLoadingGui : LoadingGui {
    
    public ProgressBarLoadingGui(string name, float minTime = 2.5F, (int, int)? size = null) : base(name, minTime, size) { }
    
    protected override void Init() {
        base.Init();
        
        LabelData labelData = new LabelData(ContentRegistry.Fontoe, "Loading", 18);
        this.AddElement("loading_label", new LabelElement(labelData, Anchor.Center, Vector2.Zero, new Vector2(5, 5)));
        
        TextureSlideBarData textureSlideBarData = new TextureSlideBarData(
            ContentRegistry.SlideBar,
            ContentRegistry.FilledSlideBar,
            null,
            barResizeMode: ResizeMode.NineSlice,
            filledBarResizeMode: ResizeMode.NineSlice,
            barBorderInsets: new BorderInsets(3),
            filledBarBorderInsets: new BorderInsets(3),
            disabledBarColor: Color.White,
            disabledFilledBarColor: Color.White);
        
        TextureSlideBarElement slideBarElement = new TextureSlideBarElement(textureSlideBarData, Anchor.Center, new Vector2(0, 80), 0, 1, wholeNumbers: false, size: new Vector2(140, 8), scale: new Vector2(5, 5)) {
            Interactable = false
        };
        this.AddElement("progress_bar", slideBarElement);
    }
    
    protected override void Update(double delta) {
        base.Update(delta);
        
        if (this.TryGetElement("progress_bar", out GuiElement? element)) {
            if (element is TextureSlideBarElement barElement) {
                barElement.Value = float.Lerp(barElement.Value, this.Progress, (float) delta * 5.0F);
            }
        }
    }
    
    protected override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        context.CommandList.ClearColorTarget(0, Color.Black.ToRgbaFloat());
        base.Draw(context, framebuffer);
    }
}