using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.GUI;
using Sparkle.CSharp.GUI.Elements;
using Sparkle.CSharp.GUI.Elements.Data;
using Sparkle.CSharp.Scenes;
using Sparkle.Test.CSharp.Dim2D;
using Sparkle.Test.CSharp.Dim3D;
using Veldrid;

namespace Sparkle.Test.CSharp.Menus;

public class SceneSwitcherMenu : Gui {
    
    public SceneSwitcherMenu(string name) : base(name) { }
    
    protected override void Init() {
        base.Init();
        
        // Relative mouse mode.
        Input.DisableRelativeMouseMode();
        
        // Title label.
        LabelData titleLabelData = new LabelData(ContentRegistry.Fontoe, "Scene Switcher", 18);
        this.AddElement("Title", new LabelElement(titleLabelData, Anchor.TopCenter, new Vector2(0, 50), new Vector2(4, 4)));
        
        // Scene drop-down.
        TextureDropDownData chooserDropDownData = new TextureDropDownData(
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
         );
        
        List<LabelData> options = [
            new LabelData(ContentRegistry.Fontoe, "Test 3D", 18),
            new LabelData(ContentRegistry.Fontoe, "Test Player", 18),
            new LabelData(ContentRegistry.Fontoe, "Terrain", 18),
            new LabelData(ContentRegistry.Fontoe, "Test 2D", 18),
        ];
        
        string currentSceneName = SceneManager.ActiveScene switch {
            TestScene3D => "Test 3D",
            PlayerMovementScene => "Test Player",
            TerrainScene => "Terrain",
            TestScene2D => "Test 2D",
            _ => "Test 3D"
        };
        
        options.Sort((a, b) => a.Text == currentSceneName ? -1 : b.Text == currentSceneName ? 1 : 0);
        
        TextureDropDownElement chooserDownElement = new TextureDropDownElement(
            chooserDropDownData,
            options,
            4,
            Anchor.Center,
            new Vector2(0, 0),
            size: new Vector2(150, 30),
            rotation: 0,
            scale: new Vector2(2, 2),
            fieldTextOffset: new Vector2(10, 1),
            menuTextOffset: new Vector2(10, 1),
            menuTextAlignment: TextAlignment.Left,
            sliderOffset: new Vector2(-1F, 0),
            scrollMaskInsets: (3, 3)
        );
        
        chooserDownElement.MenuToggled += (isMenuOpen) => {
            if (isMenuOpen) {
                if (chooserDownElement.Options.Count > chooserDownElement.MaxVisibleOptions) {
                    chooserDownElement.DropDownData.MenuSourceRect = new Rectangle(0, 0, (int) ContentRegistry.UiBannerEdgeLessTexture.Width - 2, (int) ContentRegistry.UiBannerEdgeLessTexture.Height);
                }
            }
            else {
                chooserDownElement.DropDownData.MenuSourceRect = new Rectangle(0, 0, (int) ContentRegistry.UiBannerEdgeLessTexture.Width, (int) ContentRegistry.UiBannerEdgeLessTexture.Height);
            }
        };
        
        chooserDownElement.OptionChanged += optionData => {
            switch (optionData.Text) {
                case "Test 3D":
                    GuiManager.SetGui(null);
                    SceneManager.LoadSceneAsync(new TestScene3D(), new ProgressBarLoadingGui("Loading"));
                    break;
                
                case "Test Player":
                    GuiManager.SetGui(null);
                    SceneManager.LoadSceneAsync(new PlayerMovementScene(), new ProgressBarLoadingGui("Loading"));
                    break;
                
                case "Terrain":
                    GuiManager.SetGui(null);
                    SceneManager.LoadSceneAsync(new TerrainScene(), new ProgressBarLoadingGui("Loading"));
                    break;
                
                case "Test 2D":
                    GuiManager.SetGui(null);
                    SceneManager.LoadSceneAsync(new TestScene2D(), new ProgressBarLoadingGui("Loading"));
                    break;
            }
        };
        
        this.AddElement("Chooser-Drop-Down", chooserDownElement);
    }
    
    protected override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        
        // Draw background.
        context.PrimitiveBatch.Begin(context.CommandList, framebuffer.OutputDescription);
        context.PrimitiveBatch.DrawFilledRectangle(new RectangleF(0, 0, GlobalGraphicsAssets.Window.GetWidth(), GlobalGraphicsAssets.Window.GetHeight()), color: new Color(128, 128, 128, 128));
        context.PrimitiveBatch.End();
        
        base.Draw(context, framebuffer);
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            // Relative mouse mode.
            Input.EnableRelativeMouseMode();
        }
    }
}