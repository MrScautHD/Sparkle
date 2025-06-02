using System.Numerics;
using Bliss.CSharp.Colors;
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
        
        TextureButtonData textureButtonData = new TextureButtonData(ContentRegistry.Button, hoverColor: Color.LightGray);
        LabelData testButtonLabelData = new LabelData(ContentRegistry.Fontoe, "Hello!", 18, hoverColor: Color.Green);
        
        this.AddElement("Test-Button", new TextureButtonElement(textureButtonData, testButtonLabelData, Anchor.Center, Vector2.Zero, rotation: 0, clickFunc: () => {
            Logger.Error("CLICKED!");
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