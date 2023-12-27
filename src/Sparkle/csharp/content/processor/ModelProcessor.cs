using Raylib_cs;
using Sparkle.csharp.content.type;
using Sparkle.csharp.graphics.helper;

namespace Sparkle.csharp.content.processor; 

public class ModelProcessor : IContentProcessor {

    public object Load<T>(IContentType<T> type) {
        return ModelHelper.Load(type.Path);
    }
    
    public void Unload(object item) { // TODO UNLOAD MATERIAL SHADER AND TEXTURES
        ModelHelper.Unload((Model) item);
    }
}