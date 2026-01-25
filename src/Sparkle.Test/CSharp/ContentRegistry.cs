using Bliss.CSharp;
using Bliss.CSharp.Fonts;
using Bliss.CSharp.Geometry;
using Bliss.CSharp.Graphics.Rendering;
using Bliss.CSharp.Images;
using Bliss.CSharp.Materials;
using Bliss.CSharp.Textures;
using Sparkle.CSharp.Content;
using Sparkle.CSharp.Content.Types;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.Graphics.Rendering;
using Sparkle.CSharp.Registries;
using Veldrid;

namespace Sparkle.Test.CSharp;

public class ContentRegistry : Registry {
    
    public static Font Fontoe { get; private set; }
    public static Texture2D Sprite { get; private set; }
    public static Texture2D Button { get; private set; }
    public static Texture2D TextBox { get; private set; }
    public static Texture2D ToggleBackground { get; private set; }
    public static Texture2D ToggleCheckmark { get; private set; }
    public static Texture2D SlideBar { get; private set; }
    public static Texture2D FilledSlideBar { get; private set; }
    public static Texture2D Slider { get; private set; }
    public static Texture2D CyberCarTexture { get; private set; }
    public static Texture2D UiBannerTexture { get; private set; }
    public static Texture2D UiBannerEdgeLessTexture { get; private set; }
    public static Texture2D UiArrowTexture { get; private set; }
    
    public static AnimatedImage AnimatedImage;
    public static Texture2D Gif;
    
    public static Model PlayerModel { get; private set; }
    public static Model TreeModel { get; private set; }
    public static Model CyberCarModel { get; private set; }
    
    public static MultiInstanceRenderer PlayerMultiInstanceRenderer { get; private set; }
    
    public static SkyBox SkyBox { get; private set; }
    
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
        CyberCarTexture = content.Load(new TextureContent("content/cybercar.png"));
        UiBannerTexture = content.Load(new TextureContent("content/ui_banner.png"));
        UiBannerEdgeLessTexture = content.Load(new TextureContent("content/ui_banner_edgeless.png"));
        UiArrowTexture = content.Load(new TextureContent("content/ui_arrow.png"));
        
        // Gifs:
        AnimatedImage = new AnimatedImage("content/test.gif");
        Gif = new Texture2D(content.GraphicsDevice, AnimatedImage.SpriteSheet);
        
        // Models:
        PlayerModel = content.Load(new ModelContent("content/model.glb"));
        foreach (Mesh mesh in PlayerModel.Meshes) {
            mesh.Material.RenderMode = RenderMode.Cutout;
        }
        
        TreeModel = content.Load(new ModelContent("content/tree.glb"));
        foreach (Mesh mesh in TreeModel.Meshes) {
            mesh.Material.RenderMode = RenderMode.Cutout;
            mesh.Material.RasterizerState = RasterizerStateDescription.CULL_NONE;
        }

        CyberCarModel = content.Load(new ModelContent("content/cybercar.glb", false));
        foreach (Mesh mesh in CyberCarModel.Meshes) {
            mesh.Material.SetMapTexture(MaterialMapType.Albedo, CyberCarTexture);
            mesh.Material.RenderMode = RenderMode.Cutout;
        }
        CyberCarModel.Meshes[12].Material.BlendState = BlendStateDescription.SINGLE_ALPHA_BLEND;
        CyberCarModel.Meshes[12].Material.RenderMode = RenderMode.Translucent;
        
        // MultiInstanceRenderer's:
        PlayerMultiInstanceRenderer = new MultiInstanceRenderer(PlayerModel, true);
        foreach (Mesh mesh in PlayerMultiInstanceRenderer.Meshes) {
            PlayerMultiInstanceRenderer.GetRenderableMaterialByMesh(mesh).Effect = GlobalResource.ModelInstancingEffect;
        }
        
        // Skybox's:
        SkyBox = new SkyBox(content.GraphicsDevice, content.Load(new CubemapContent("content/skybox.png")));
    }
    
    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        
        if (disposing) {
            SkyBox.Dispose();
        }
    }
}