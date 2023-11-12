using Raylib_cs;
using Sparkle.csharp.content;
using Sparkle.csharp.content.type;

namespace Sparkle.csharp.registry.types; 

public class ShaderRegistry : Registry {

    public static Shader Light { get; private set; }
    public static Shader DiscardAlpha { get; private set; }
    
    protected internal override void Load(ContentManager content) {
        base.Load(content);
        
        Light = this.Register("lighting", () => content.Load<Shader>(new ShaderContent("shaders/lighting.vert", "shaders/lighting.frag")));
        DiscardAlpha = this.Register("discard_alpha", () => content.Load<Shader>(new ShaderContent(null!, "shaders/discard_alpha.frag")));
    }
}