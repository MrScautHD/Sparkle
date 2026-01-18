using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Sprites;
using Bliss.CSharp.Logging;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.GUI;
using Sparkle.CSharp.GUI.Elements;
using Sparkle.CSharp.GUI.Elements.Data;
using Veldrid;

namespace Sparkle.Test.CSharp;

public class TestGui : Gui {
    
    public TestGui(string name) : base(name) { }
    
    protected override void Init() {
        base.Init();
        
        // Label.
        LabelData labelData = new LabelData(ContentRegistry.Fontoe, "Hello Sparkle!", 18, hoverColor: Color.Gray);
        
        this.AddElement("Label", new LabelElement(labelData, Anchor.BottomLeft, Vector2.Zero, clickFunc: () => {
            Logger.Error("CLICKED!");
            return true;
        }));
        
        // Toggle.
        ToggleData toggleData = new ToggleData(ContentRegistry.ToggleBackground, ContentRegistry.ToggleCheckmark, checkboxHoverColor: Color.LightGray, checkmarkHoverColor: Color.LightGray);
        LabelData toggleLabelData = new LabelData(ContentRegistry.Fontoe, "Toggle ME!", 18);
        
        this.AddElement("Toggle", new ToggleElement(toggleData, toggleLabelData, Anchor.Center, new Vector2(0, 120), 5, clickFunc: () => {
            Logger.Error("CLICKED!");
            return true;
        }));
        
        // Texture button.
        TextureButtonData textureButtonData = new TextureButtonData(ContentRegistry.UiBannerTexture, hoverColor: Color.LightGray, resizeMode: ResizeMode.NineSlice, borderInsets: new BorderInsets(12), flip: SpriteFlip.None);
        LabelData textureButtonLabelData = new LabelData(ContentRegistry.Fontoe, "TTT", 18, hoverColor: Color.Green);
        
        this.AddElement("Texture-Button", new TextureButtonElement(textureButtonData, textureButtonLabelData, Anchor.Center, new Vector2(0, 60), size: new Vector2(120, 30), textOffset: new Vector2(0, 1), clickFunc: () => {
            Logger.Error("CLICKED!");
            return true;
        }));
        
        // Rectangle button.
        RectangleButtonData rectangleButtonData = new RectangleButtonData(Color.Gray, Color.LightGray, null, 4, Color.DarkGray, Color.Gray);
        LabelData rectangleButtonLabelData = new LabelData(ContentRegistry.Fontoe, "Hello!", 18, hoverColor: Color.Green);
        
        this.AddElement("Rectangle-Button", new RectangleButtonElement(rectangleButtonData, rectangleButtonLabelData, Anchor.Center, Vector2.Zero, new Vector2(200, 30), clickFunc: () => {
            Logger.Error("CLICKED!");
            return true;
        }));
        
        // Texture text box.
        TextureTextBoxData textureTextBoxData = new TextureTextBoxData(ContentRegistry.UiBannerTexture, hoverColor: Color.LightGray, resizeMode: ResizeMode.NineSlice, borderInsets: new BorderInsets(12), flip: SpriteFlip.None);
        LabelData textureTextBoxLabelData = new LabelData(ContentRegistry.Fontoe, "", 18, hoverColor: Color.Green);
        LabelData textureHintTextBoxLabelData = new LabelData(ContentRegistry.Fontoe, "Write...", 18, color: Color.Gray);
        
        this.AddElement("Texture-Text-Box", new TextureTextBoxElement(textureTextBoxData, textureTextBoxLabelData, textureHintTextBoxLabelData, Anchor.Center, new Vector2(0, -60), 40, TextAlignment.Center, new Vector2(0, 1), (12, 12), new Vector2(260, 30), rotation: 0, clickFunc: () => {
            Logger.Error("BOX1!");
            return true;
        }));
        
        // Rectangle text box.
        RectangleTextBoxData rectangleTextBoxData = new RectangleTextBoxData(Color.Gray, Color.LightGray, null, 4, Color.DarkGray, Color.Gray);
        LabelData rectangleTextBoxLabelData = new LabelData(ContentRegistry.Fontoe, "", 18, hoverColor: Color.Green);
        LabelData rectangleHintTextBoxLabelData = new LabelData(ContentRegistry.Fontoe, "Write...", 18, color: Color.LightGray, hoverColor: Color.Gray);
        
        this.AddElement("Rectangle-Text-Box", new RectangleTextBoxElement(rectangleTextBoxData, rectangleTextBoxLabelData, rectangleHintTextBoxLabelData, Anchor.Center, new Vector2(0, -110), new Vector2(200, 35), 40, new Vector2(1, 1), TextAlignment.Left, new Vector2(0, 0), (12, 12), rotation: 0, clickFunc: () => {
            Logger.Error("BOX1!");
            return true;
        }));
        
        // Texture slider bar.
        TextureSlideBarData textureSlideBarData = new TextureSlideBarData(ContentRegistry.SliderBar, ContentRegistry.Slider, barResizeMode: ResizeMode.NineSlice, barBorderInsets: new BorderInsets(3));
        
        this.AddElement("Texture-Slider-Bar", new TextureSlideBarElement(textureSlideBarData, Anchor.Center, new Vector2(0, 170), 0, 10, wholeNumbers: false, size: new Vector2(140, 8), rotation: 0, clickFunc: () => {
            Logger.Error("BOX1!");
            return true;
        }));
    }
    
    protected override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        GuiManager.Scale = 1;
        
        // Draw background.
        context.PrimitiveBatch.Begin(context.CommandList, framebuffer.OutputDescription);
        context.PrimitiveBatch.DrawFilledRectangle(new RectangleF(0, 0, GlobalGraphicsAssets.Window.GetWidth(), GlobalGraphicsAssets.Window.GetHeight()), color: new Color(128, 128, 128, 128));
        context.PrimitiveBatch.End();
        
        // Draw elements.
        base.Draw(context, framebuffer);
    }
}