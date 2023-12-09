using Raylib_cs;
using Sparkle.csharp.content.type;
using Sparkle.csharp.graphics.helper;

namespace Sparkle.csharp.content.processor; 

public class ShaderProcessor : IContentProcessor {
    
    public object Load<T>(IContentType<T> type) {
        return ShaderHelper.Load(((ShaderContent) type).Path, ((ShaderContent) type).FragPath);
    }

    public void Unload(object item) {
        ShaderHelper.Unload((Shader) item);
    }
}