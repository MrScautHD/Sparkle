using Raylib_cs;
using Sparkle.csharp.graphics.util;

namespace Sparkle.csharp.content.processor; 

public class ShaderProcessor : IContentProcessor {
    
    public object Load(string path) {
        return ShaderHelper.Load($"{path}.vs", $"{path}.fs");
    }

    public void Unload(object item) {
        ShaderHelper.Unload((Shader) item);
    }
}