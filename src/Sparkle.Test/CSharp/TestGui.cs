using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Logging;
using Bliss.CSharp.Transformations;
using FontStashSharp.RichText;
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
        
        TextureButtonData textureButtonData = new TextureButtonData(ContentRegistry.Button, hoverColor: Color.LightGray);
        LabelData textureButtonLabelData = new LabelData(ContentRegistry.Fontoe, "TTT", 18, hoverColor: Color.Green);
        
        this.AddElement("Test-Texture-Button", new TextureButtonElement(textureButtonData, textureButtonLabelData, Anchor.Center, new Vector2(0, 60), rotation: 0, clickFunc: () => {
            Logger.Error("CLICKED!");
            return true;
        }));
        
        RectangleButtonData rectangleButtonData = new RectangleButtonData(Color.Gray, Color.LightGray, 4, Color.DarkGray, Color.Gray);
        LabelData rectangleButtonLabelData = new LabelData(ContentRegistry.Fontoe, "Hello!", 18, hoverColor: Color.Green);
        
        this.AddElement("Test-Rectangle-Button", new RectangleButtonElement(rectangleButtonData, rectangleButtonLabelData, Anchor.Center, Vector2.Zero, new Vector2(200, 30), rotation: 0, clickFunc: () => {
            Logger.Error("CLICKED!");
            return true;
        }));
        
        TextureTextBoxData textBoxData = new TextureTextBoxData(ContentRegistry.TextBox, hoverColor: Color.LightGray);
        LabelData textBoxLabelData = new LabelData(ContentRegistry.Fontoe, "", 18, hoverColor: Color.Green);
        LabelData hintTextBoxLabelData = new LabelData(ContentRegistry.Fontoe, "Write...", 18, color: Color.Gray);
        
        this.AddElement("Text-Box-Button", new TextureTextBoxElement(textBoxData, textBoxLabelData, hintTextBoxLabelData, Anchor.Center, new Vector2(0, -60), 160, TextAlignment.Center, (12, 12), rotation: 0, clickFunc: () => {
            Logger.Error("BOX1!");
            return true;
        }));
        
        LabelData testLabelData = new LabelData(ContentRegistry.Fontoe, "Hello Sparkle!", 18, scale: new Vector2(1, 1));
        
        this.AddElement("Test-Label", new LabelElement(testLabelData, Anchor.BottomLeft, Vector2.Zero, clickFunc: () => {
            Logger.Error("CLICKED!");
            return true;
        }));
    }
    
    protected override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        
        // Draw background.
        context.PrimitiveBatch.Begin(context.CommandList, framebuffer.OutputDescription);
        context.PrimitiveBatch.DrawFilledRectangle(new RectangleF(0, 0, GlobalGraphicsAssets.Window.GetWidth(), GlobalGraphicsAssets.Window.GetHeight()), color: new Color(128, 128, 128, 128));
        context.PrimitiveBatch.End();
        
        // Draw elements.
        base.Draw(context, framebuffer);
    }
}