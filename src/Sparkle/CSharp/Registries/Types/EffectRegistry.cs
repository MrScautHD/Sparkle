using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering.Gl;
using Sparkle.CSharp.Content;
using Sparkle.CSharp.Effects;
using Sparkle.CSharp.Effects.Types;

namespace Sparkle.CSharp.Registries.Types;

public class EffectRegistry : Registry {

    public static string GlslVersion => RlGl.GetVersion() == GlVersion.OpenGl33 ? "glsl330" : "glsl430";

    // Shaders
    public static Effect DiscardAlpha { get; private set; }
    public static SkyboxEffect Skybox { get; private set; }
    public static PbrEffect Pbr { get; private set; }
    
    // Filters
    public static FxaaEffect Fxaa { get; private set; }
    public static Effect GrayScale { get; private set; }
    public static PosterizationEffect Posterization { get; private set; }
    public static Effect DreamVision { get; private set; }
    public static PixelizerEffect Pixelizer { get; private set; }
    public static Effect CrossHatching { get; private set; }
    public static Effect CrossStitching { get; private set; }
    
    
    public static BlurEffect Blur { get; private set; }
    
    protected internal override void Load(ContentManager content) {
        base.Load(content);

        // Shaders
        DiscardAlpha = new Effect(null!, "content/shaders/glsl330/discard_alpha.frag");
        EffectManager.Add(DiscardAlpha);
        
        Skybox = new SkyboxEffect("content/shaders/glsl330/skybox.vert", "content/shaders/glsl330/skybox.frag");
        EffectManager.Add(Skybox);
        
        Pbr = new PbrEffect($"content/shaders/{GlslVersion}/pbr.vert", $"content/shaders/{GlslVersion}/pbr.frag", RlGl.GetVersion(), Color.Blue);
        EffectManager.Add(Pbr);
        
        // Filters
        Fxaa = new FxaaEffect(null!, "content/shaders/glsl330/filter/fxaa.frag");
        EffectManager.Add(Fxaa);
        
        GrayScale = new Effect(null!, "content/shaders/glsl330/filter/gray_scale.frag");
        EffectManager.Add(GrayScale);
        
        Posterization = new PosterizationEffect(null!, "content/shaders/glsl330/filter/posterization.frag");
        EffectManager.Add(Posterization);
        
        DreamVision = new Effect(null!, "content/shaders/glsl330/filter/dream_vision.frag");
        EffectManager.Add(DreamVision);
        
        Pixelizer = new PixelizerEffect(null!, "content/shaders/glsl330/filter/pixelizer.frag");
        EffectManager.Add(Pixelizer);
                
        CrossHatching = new Effect(null!, "content/shaders/glsl330/filter/cross_hatching.frag");
        EffectManager.Add(CrossHatching);
        
        CrossStitching = new Effect(null!, "content/shaders/glsl330/filter/cross_stitching.frag");
        EffectManager.Add(CrossStitching);
        
        Blur = new BlurEffect(null!, "content/shaders/glsl330/filter/blur.frag");
        EffectManager.Add(Blur);
    }
}