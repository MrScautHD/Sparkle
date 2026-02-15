using Bliss.CSharp;
using Bliss.CSharp.Fonts;
using Bliss.CSharp.Geometry;
using Bliss.CSharp.Graphics.Rendering;
using Bliss.CSharp.Images;
using Bliss.CSharp.Materials;
using Bliss.CSharp.Textures;
using Sparkle.CSharp.Content;
using Sparkle.CSharp.Content.Types;
using Sparkle.CSharp.Graphics.Rendering;
using Sparkle.CSharp.Registries;
using Veldrid;

namespace Sparkle.Test.CSharp;

public class ContentRegistry : Registry {
    
    // Fonts:
    public static Font Fontoe { get; private set; }
    
    // Textures:
    public static Texture2D Sprite { get; private set; }
    public static Texture2D Button { get; private set; }
    public static Texture2D TextBox { get; private set; }
    public static Texture2D ToggleBackground { get; private set; }
    public static Texture2D ToggleCheckmark { get; private set; }
    public static Texture2D SlideBar { get; private set; }
    public static Texture2D FilledSlideBar { get; private set; }
    public static Texture2D Slider { get; private set; }
    public static Texture2D UiBannerTexture { get; private set; }
    public static Texture2D UiBannerEdgeLessTexture { get; private set; }
    public static Texture2D UiSliderTexture { get; private set; }
    public static Texture2D UiArrowTexture { get; private set; }
    
    // Gifs:
    public static AnimatedImage AnimatedImage;
    public static Texture2D Gif;
        
    // Skybox's:
    public static SkyBox SkyBox { get; private set; }
    
    // ------------ Scene 3D Collection Start ------------ \\
    
    public static ContentCollection Scene3DCollection { get; private set; }
    
    // Scene textures:
    public static Texture2D CyberCarTexture => Scene3DCollection.Get<Texture2D>("content/cybercar.png");
    
    // Scene models:
    public static Model PlayerModel => Scene3DCollection.Get<Model>("content/model.glb");
    public static Model TreeModel => Scene3DCollection.Get<Model>("content/tree.glb");
    public static Model CyberCarModel => Scene3DCollection.Get<Model>("content/cybercar.glb");
    
    // Scene player multiInstanceRenderer:
    public static MultiInstanceRenderer PlayerMultiInstanceRenderer { get; private set; }
    
    // ------------ Scene 3D Collection End ------------ \\
    
    /// <summary>
    /// Loads the content for the registry, including fonts, textures, models, and other assets.
    /// This method initializes the necessary assets required by the registry, utilizing the provided
    /// <see cref="ContentManager"/> to load resources.
    /// </summary>
    /// <param name="content">The <see cref="ContentManager"/> instance used to load resources.</param>
    protected override void Load(ContentManager content) {
        base.Load(content);
        
        // Fonts:
        Fontoe = content.Load(new FontContent("content/fontoe.ttf"));
        
        // Textures:
        Sprite = content.Load(new TextureContent("content/sprite.png"));
        Button = content.Load(new TextureContent("content/button.png"));
        TextBox = content.Load(new TextureContent("content/text-box.png"));
        ToggleBackground = content.Load(new TextureContent("content/toggle_background.png"));
        ToggleCheckmark = content.Load(new TextureContent("content/toggle_checkmark.png"));
        SlideBar = content.Load(new TextureContent("content/bar.png"));
        FilledSlideBar = content.Load(new TextureContent("content/filled_bar.png"));
        Slider = content.Load(new TextureContent("content/slider.png"));
        UiBannerTexture = content.Load(new TextureContent("content/ui_banner.png"));
        UiBannerEdgeLessTexture = content.Load(new TextureContent("content/ui_banner_edgeless.png"));
        UiSliderTexture = content.Load(new TextureContent("content/slider_high_res.png"));
        UiArrowTexture = content.Load(new TextureContent("content/ui_arrow.png"));
        
        // Gifs:
        AnimatedImage = new AnimatedImage("content/test.gif");
        Gif = new Texture2D(content.GraphicsDevice, AnimatedImage.SpriteSheet);
        
        // Skybox's:
        SkyBox = new SkyBox(content.GraphicsDevice, content.Load(new CubemapContent("content/skybox.png")));
        
        // Scene Collection:
        Scene3DCollection = content.DefineCollection("Scene-Test", [
            
            // Textures:
            new TextureContent("content/cybercar.png"),
            
            // Models:
            new ModelContent("content/model.glb").Do(model => {
                foreach (Mesh mesh in model.Meshes) {
                    mesh.Material.RenderMode = RenderMode.Cutout;
                }
                
                // Set player multi instance renderer.
                PlayerMultiInstanceRenderer = new MultiInstanceRenderer(model, true);
                
                foreach (Mesh mesh in PlayerMultiInstanceRenderer.Meshes) {
                    PlayerMultiInstanceRenderer.GetRenderableMaterialByMesh(mesh).Effect = GlobalResource.ModelInstancingEffect;
                }
            }),
            new ModelContent("content/tree.glb").Do(model => {
                foreach (Mesh mesh in model.Meshes) {
                    mesh.Material.RenderMode = RenderMode.Cutout;
                    mesh.Material.RasterizerState = RasterizerStateDescription.CULL_NONE;
                }
            }),
            new ModelContent("content/cybercar.glb").Do(model => {
                foreach (Mesh mesh in model.Meshes) {
                    mesh.Material.SetMapTexture(MaterialMapType.Albedo, CyberCarTexture);
                    mesh.Material.RenderMode = RenderMode.Cutout;
                }
                
                model.Meshes[12].Material.BlendState = BlendStateDescription.SINGLE_ALPHA_BLEND;
                model.Meshes[12].Material.RenderMode = RenderMode.Translucent;
            })
        ]);
    }
    
    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        
        if (disposing) {
            SkyBox.Dispose();
        }
    }
}