using Raylib_cs;
using Sparkle.csharp.content.type;
using Sparkle.csharp.graphics.helper;

namespace Sparkle.csharp.content.processor; 

public class ShaderProcessor : IContentProcessor {
    
    public object Load(IContentType type, string directory) {
        return ShaderHelper.Load(directory + ((ShaderContent) type).Path, directory + ((ShaderContent) type).FragPath);
    }

    public void Unload(object item) {
        ShaderHelper.Unload((Shader) item);
    }

    public Type GetContentType() {
        return typeof(ShaderContent);
    }
}