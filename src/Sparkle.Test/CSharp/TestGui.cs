using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Graphics;
using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Sprites;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Mice.Cursors;
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
        
        this.AddElement("Label", new LabelElement(labelData, Anchor.BottomLeft, Vector2.Zero, clickFunc: (element) => {
            Logger.Error("CLICKED!");
            return true;
        }));
        
        // Toggle.
        ToggleData toggleData = new ToggleData(ContentRegistry.ToggleBackground, ContentRegistry.ToggleCheckmark, checkboxHoverColor: Color.LightGray, checkmarkHoverColor: Color.LightGray);
        LabelData toggleLabelData = new LabelData(ContentRegistry.Fontoe, "Toggle ME!", 18);
        
        this.AddElement("Toggle", new ToggleElement(toggleData, toggleLabelData, Anchor.Center, new Vector2(0, 120), 5, clickFunc: (element) => {
            Logger.Error("CLICKED!");
            return true;
        }));
        
        // Texture button.
        TextureButtonData textureButtonData = new TextureButtonData(ContentRegistry.UiBannerTexture, hoverColor: Color.LightGray, resizeMode: ResizeMode.None, borderInsets: new BorderInsets(12), flip: SpriteFlip.None);
        LabelData textureButtonLabelData = new LabelData(ContentRegistry.Fontoe, "TTT", 18, hoverColor: Color.Green);
        
        this.AddElement("Texture-Button", new TextureButtonElement(textureButtonData, textureButtonLabelData, Anchor.Center, new Vector2(0, 60), textOffset: new Vector2(0, 1), clickFunc: (element) => {
            Logger.Error("CLICKED!");
            return true;
        }));
        
        // Rectangle button.
        RectangleButtonData rectangleButtonData = new RectangleButtonData(Color.Gray, Color.LightGray, null, 4, Color.DarkGray, Color.Gray);
        LabelData rectangleButtonLabelData = new LabelData(ContentRegistry.Fontoe, "Hello!", 18, hoverColor: Color.Green);
        
        this.AddElement("Rectangle-Button", new RectangleButtonElement(rectangleButtonData, rectangleButtonLabelData, Anchor.Center, Vector2.Zero, new Vector2(200, 30), clickFunc: (element) => {
            Logger.Error("CLICKED!");
            return true;
        }));
        
        // Texture text box.
        TextureTextBoxData textureTextBoxData = new TextureTextBoxData(ContentRegistry.UiBannerTexture, hoverColor: Color.LightGray, resizeMode: ResizeMode.NineSlice, borderInsets: new BorderInsets(12), flip: SpriteFlip.None);
        LabelData textureTextBoxLabelData = new LabelData(ContentRegistry.Fontoe, "", 18, hoverColor: Color.Green);
        LabelData textureHintTextBoxLabelData = new LabelData(ContentRegistry.Fontoe, "Write...", 18, color: Color.Gray);
        
        this.AddElement("Texture-Text-Box", new TextureTextBoxElement(textureTextBoxData, textureTextBoxLabelData, textureHintTextBoxLabelData, Anchor.Center, new Vector2(0, -60), 40, TextAlignment.Center, new Vector2(0, 1), (12, 12), new Vector2(260, 30), rotation: 0, clickFunc: (element) => {
            Logger.Error("BOX1!");
            return true;
        }));
        
        // Rectangle text box.
        RectangleTextBoxData rectangleTextBoxData = new RectangleTextBoxData(Color.Gray, Color.LightGray, null, 4, Color.DarkGray, Color.Gray);
        LabelData rectangleTextBoxLabelData = new LabelData(ContentRegistry.Fontoe, "", 18, hoverColor: Color.Green);
        LabelData rectangleHintTextBoxLabelData = new LabelData(ContentRegistry.Fontoe, "Write...", 18, color: Color.LightGray, hoverColor: Color.Gray);
        
        this.AddElement("Rectangle-Text-Box", new RectangleTextBoxElement(rectangleTextBoxData, rectangleTextBoxLabelData, rectangleHintTextBoxLabelData, Anchor.Center, new Vector2(0, -110), new Vector2(200, 35), 40, new Vector2(1, 1), TextAlignment.Left, new Vector2(0, 0), (12, 12), rotation: 0, clickFunc: (element) => {
            Logger.Error("BOX1!");
            return true;
        }));
        
        // Texture slider bar.
        TextureSlideBarData textureSlideBarData = new TextureSlideBarData(
            ContentRegistry.SlideBar,
            ContentRegistry.FilledSlideBar,
            ContentRegistry.Slider,
            barResizeMode: ResizeMode.NineSlice,
            filledBarResizeMode: ResizeMode.NineSlice,
            barBorderInsets: new BorderInsets(3),
            filledBarBorderInsets: new BorderInsets(3));
        
        this.AddElement("Texture-Slider-Bar", new TextureSlideBarElement(textureSlideBarData, Anchor.Center, new Vector2(0, 170), 0, 10, wholeNumbers: false, size: new Vector2(140, 8), scale: new Vector2(2, 2), clickFunc: (element) => {
            Logger.Error("BOX1!");
            return true;
        }));
        
        // Rectangle slider bar.
        RectangleSlideBarData rectangleSlideBarData = new RectangleSlideBarData() {
            SliderSize = new Vector2(16, 16),
            BarOutlineThickness = 4,
            FilledBarOutlineThickness = 4,
            SliderOutlineThickness = 4
        };
        
        this.AddElement("Rectangle-Slider-Bar", new RectangleSlideBarElement(rectangleSlideBarData, Anchor.Center, new Vector2(0, 200), new Vector2(280, 16), 0, 10, clickFunc: (element) => {
            Logger.Error("BOX1!");
            return true;
        }));
        
        // Texture drop down.
        TextureDropDownData textureDropDownData = new TextureDropDownData(
            ContentRegistry.UiBannerTexture,
            ContentRegistry.UiBannerEdgeLessTexture,
            ContentRegistry.UiBannerEdgeLessTexture,
            ContentRegistry.UiSliderTexture,
            ContentRegistry.UiArrowTexture,
            sliderBarSourceRect: new Rectangle(2, 0, (int) ContentRegistry.UiBannerEdgeLessTexture.Width - 2, (int) ContentRegistry.UiBannerEdgeLessTexture.Height),
            fieldResizeMode: ResizeMode.NineSlice,
            menuResizeMode: ResizeMode.NineSlice,
            sliderBarResizeMode: ResizeMode.NineSlice,
            fieldBorderInsets: new BorderInsets(12),
            menuBorderInsets: new BorderInsets(5),
            sliderBarBorderInsets: new BorderInsets(5)
            //fieldHoverColor: Color.Gray
            //menuHoverColor: Color.Gray,
            //arrowHoverColor: Color.Gray
         );
        
        List<LabelData> options = [
            new LabelData(ContentRegistry.Fontoe, "Option 1", 18),
            new LabelData(ContentRegistry.Fontoe, "Option 2", 18),
            new LabelData(ContentRegistry.Fontoe, "Option 3", 18),
            new LabelData(ContentRegistry.Fontoe, "Option 4", 18),
            new LabelData(ContentRegistry.Fontoe, "Option 5", 18),
            new LabelData(ContentRegistry.Fontoe, "Option 6", 18),
            new LabelData(ContentRegistry.Fontoe, "Option 7", 18),
            new LabelData(ContentRegistry.Fontoe, "Option 8", 18),
            new LabelData(ContentRegistry.Fontoe, "Option 9", 18)
        ];
        
        TextureDropDownElement dropDownElement = new TextureDropDownElement(
            textureDropDownData,
            options,
            4,
            Anchor.CenterLeft,
            new Vector2(40, 0),
            size: new Vector2(140F, 30),
            rotation: 0,
            scale: new Vector2(2, 2),
            fieldTextOffset: new Vector2(10, 1),
            menuTextOffset: new Vector2(10, 1),
            menuTextAlignment: TextAlignment.Left,
            sliderOffset: new Vector2(-1F, 0),
            scrollMaskInsets: (3, 3)
        );
        
        dropDownElement.MenuToggled += (isMenuOpen) => {
            if (isMenuOpen) {
                if (dropDownElement.Options.Count > dropDownElement.MaxVisibleOptions) {
                    dropDownElement.DropDownData.MenuSourceRect = new Rectangle(0, 0, (int) ContentRegistry.UiBannerEdgeLessTexture.Width - 2, (int) ContentRegistry.UiBannerEdgeLessTexture.Height);
                } 
            }
            else {
                dropDownElement.DropDownData.MenuSourceRect = new Rectangle(0, 0, (int) ContentRegistry.UiBannerEdgeLessTexture.Width, (int) ContentRegistry.UiBannerEdgeLessTexture.Height);
            }
        };
        
        this.AddElement("Texture-Drop-Down", dropDownElement);
        
        // TWO
        TextureDropDownElement dropDownElement2 = new TextureDropDownElement(
            textureDropDownData,
            options,
            4,
            Anchor.CenterLeft,
            new Vector2(40, -70),
            size: new Vector2(140F, 30),
            rotation: 0,
            scale: new Vector2(2, 2),
            fieldTextOffset: new Vector2(10, 1),
            menuTextOffset: new Vector2(10, 1),
            menuTextAlignment: TextAlignment.Left,
            sliderOffset: new Vector2(-1F, 0),
            scrollMaskInsets: (3, 3)
        );
        
        dropDownElement2.MenuToggled += (isMenuOpen) => {
            if (isMenuOpen) {
                if (dropDownElement2.Options.Count > dropDownElement2.MaxVisibleOptions) {
                    dropDownElement2.DropDownData.MenuSourceRect = new Rectangle(0, 0, (int) ContentRegistry.UiBannerEdgeLessTexture.Width - 2, (int) ContentRegistry.UiBannerEdgeLessTexture.Height);
                } 
            }
            else {
                dropDownElement2.DropDownData.MenuSourceRect = new Rectangle(0, 0, (int) ContentRegistry.UiBannerEdgeLessTexture.Width, (int) ContentRegistry.UiBannerEdgeLessTexture.Height);
            }
        };
        
        this.AddElement("Texture-Drop-Down2", dropDownElement2);
    }

    protected override void Update(double delta) {
        base.Update(delta);
        
        // Set the cursor state. // !WARNING! It's not recommended to create a cursor every tick!
        //foreach (GuiElement element in this.GetElements()) {
        //    if (element.IsHovered) {
        //        if (element is TextureTextBoxElement textBoxElement) {
        //            if (textBoxElement.IsSelected) {
        //                Input.SetMouseCursor(new Sdl3Cursor(SystemCursor.Text));
        //            }
        //            else {
        //                Input.SetMouseCursor(new Sdl3Cursor(SystemCursor.Pointer));
        //            }
        //        }
        //        else {
        //            Input.SetMouseCursor(new Sdl3Cursor(SystemCursor.Pointer));
        //        }
        //        break;
        //    }
        //    else {
        //        Input.SetMouseCursor(new Sdl3Cursor(SystemCursor.Default));
        //    }
        //}
    }
    
    protected override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        //GuiManager.Scale = 1.5F;
        
        // Draw background.
        context.PrimitiveBatch.Begin(context.CommandList, framebuffer.OutputDescription);
        context.PrimitiveBatch.DrawFilledRectangle(new RectangleF(0, 0, GlobalGraphicsAssets.Window.GetWidth(), GlobalGraphicsAssets.Window.GetHeight()), color: new Color(128, 128, 128, 128));
        context.PrimitiveBatch.End();
        
        // Draw elements.
        base.Draw(context, framebuffer);
    }
}