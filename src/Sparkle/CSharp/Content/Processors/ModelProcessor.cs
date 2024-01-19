using Raylib_cs;
using Sparkle.CSharp.Content.Types;
using Sparkle.CSharp.Rendering.Helpers;

namespace Sparkle.CSharp.Content.Processors; 

public class ModelProcessor : IContentProcessor {

    public object Load<T>(IContentType<T> type) {
        return ModelHelper.Load(type.Path);
    }
    
    public void Unload(object item) { // TODO UNLOAD MATERIAL SHADER AND TEXTURES
        ModelHelper.Unload((Model) item);
    }
}