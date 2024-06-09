using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering.Gl;
using Sparkle.CSharp.Content;
using Sparkle.CSharp.Effects;
using Sparkle.CSharp.Effects.Types;

namespace Sparkle.CSharp.Registries.Types;

public class EffectRegistry : Registry {

    public static string GlslVersion => RlGl.GetVersion() == GlVersion.OpenGl33 ? "glsl330" : "glsl330";

    public static Effect DiscardAlpha { get; private set; }
    public static SkyboxEffect Skybox { get; private set; }
    public static PbrEffect Pbr { get; private set; }
    
    protected internal override void Load(ContentManager content) {
        base.Load(content);

        DiscardAlpha = new Effect(null!, "content/shaders/discard_alpha.frag");
        EffectManager.Add(DiscardAlpha);

        Skybox = new SkyboxEffect("content/shaders/skybox.vert", "content/shaders/skybox.frag");
        EffectManager.Add(Skybox);
        
        Pbr = new PbrEffect($"content/shaders/{GlslVersion}/pbr.vert", $"content/shaders/{GlslVersion}/pbr.frag", GlVersion.OpenGl33, Color.Blue);
        EffectManager.Add(Pbr);
    }
}