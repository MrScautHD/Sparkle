using Raylib_cs;
using Sparkle.csharp.content;
using Sparkle.csharp.content.type;

namespace Sparkle.csharp.registry.types; 

public class ShaderRegistry : Registry {

    public static Shader Pbr { get; private set; }
    public static Shader DiscardAlpha { get; private set; }
    
    protected internal override void Load(ContentManager content) {
        base.Load(content);
        
        Pbr = this.Register("pbr", () => content.Load(new ShaderContent("content/shaders/pbr.vert", "content/shaders/pbr.frag")));
        DiscardAlpha = this.Register("discard_alpha", () => content.Load(new ShaderContent(null!, "content/shaders/discard_alpha.frag")));
    }
}