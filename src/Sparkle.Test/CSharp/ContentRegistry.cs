using Bliss.CSharp.Fonts;
using Bliss.CSharp.Geometry;
using Bliss.CSharp.Graphics.Rendering;
using Bliss.CSharp.Textures;
using Sparkle.CSharp.Content;
using Sparkle.CSharp.Content.Types;
using Sparkle.CSharp.Graphics.Rendering;
using Sparkle.CSharp.Registries;
using Veldrid;

namespace Sparkle.Test.CSharp;

public class ContentRegistry : Registry {
    
    public static Font Fontoe { get; private set; }
    public static Model PlayerModel { get; private set; }
    public static Model TreeModel { get; private set; }
    public static Texture2D Sprite { get; private set; }
    public static Texture2D Button { get; private set; }
    public static Texture2D TextBox { get; private set; }
    public static Texture2D ToggleBackground { get; private set; }
    public static Texture2D ToggleCheckmark { get; private set; }
    public static Texture2D EmptySliderBar { get; private set; }
    public static Texture2D FullSliderBar { get; private set; }
    public static Texture2D Slider { get; private set; }
    
    public static SkyBox SkyBox { get; private set; }
    
    protected override void Load(ContentManager content) {
        base.Load(content);
        
        // Fonts:
        Fontoe = content.Load(new FontContent("content/fontoe.ttf"));
        
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
        
        // Textures:
        Sprite = content.Load(new TextureContent("content/sprite.png"));
        Button = content.Load(new TextureContent("content/button.png"));
        TextBox = content.Load(new TextureContent("content/text-box.png"));
        ToggleBackground = content.Load(new TextureContent("content/toggle_background.png"));
        ToggleCheckmark = content.Load(new TextureContent("content/toggle_checkmark.png"));
        EmptySliderBar = content.Load(new TextureContent("content/empty_slider_bar.png"));
        FullSliderBar = content.Load(new TextureContent("content/full_slider_bar.png"));
        Slider = content.Load(new TextureContent("content/slider.png"));
        
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