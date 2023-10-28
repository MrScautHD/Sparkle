using Raylib_cs;

namespace Sparkle.csharp.registry.types; 

public class ShaderRegistry : Registry {

    public static Shader LightShader { get; private set; }
    
    protected internal override void Load() {
        base.Load();

        LightShader = this.Register("light", this.Content.Load<Shader>("shaders/lighting"));
    }
}