using Raylib_cs;
using Sparkle.CSharp.Content;
using Sparkle.CSharp.Effects;
using Sparkle.CSharp.Effects.Types;

namespace Sparkle.CSharp.Registries.Types;

public class EffectRegistry : Registry {

    public static Effect DiscardAlpha { get; private set; }
    public static PbrEffect Pbr { get; private set; }
    
    protected internal override void Load(ContentManager content) {
        base.Load(content);

        DiscardAlpha = new Effect(null!, "content/shaders/discard_alpha.frag");
        EffectManager.Add(DiscardAlpha);

        Pbr = new PbrEffect("content/shaders/pbr.vert", "content/shaders/pbr.frag", Color.Blue);
        EffectManager.Add(Pbr);
    }
}