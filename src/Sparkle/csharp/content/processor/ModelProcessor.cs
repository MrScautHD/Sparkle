using Raylib_cs;
using Sparkle.csharp.content.type;
using Sparkle.csharp.graphics.util;

namespace Sparkle.csharp.content.processor; 

public class ModelProcessor : IContentProcessor {

    public object Load(IContentType type, string directory) {
        return ModelHelper.Load(directory + type.Path);
    }
    
    public void Unload(object item) {
        ModelHelper.Unload((Model) item);
    }
}