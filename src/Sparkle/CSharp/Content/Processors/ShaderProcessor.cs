using Raylib_CSharp.Shaders;
using Sparkle.CSharp.Content.Types;

namespace Sparkle.CSharp.Content.Processors;

public class ShaderProcessor : IContentProcessor {
    
    public object Load<T>(IContentType<T> type) {
        return Shader.Load(((ShaderContent) type).Path, ((ShaderContent) type).FragPath);
    }

    public void Unload(object item) {
        Shader.Unload((Shader) item);
    }
}