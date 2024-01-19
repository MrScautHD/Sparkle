using Raylib_cs;
using Sparkle.CSharp.Content.Types;
using Sparkle.CSharp.Rendering.Helpers;

namespace Sparkle.CSharp.Content.Processors; 

public class ShaderProcessor : IContentProcessor {
    
    public object Load<T>(IContentType<T> type) {
        return ShaderHelper.Load(((ShaderContent) type).Path, ((ShaderContent) type).FragPath);
    }

    public void Unload(object item) {
        ShaderHelper.Unload((Shader) item);
    }
}