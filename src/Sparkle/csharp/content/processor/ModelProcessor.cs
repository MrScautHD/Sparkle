using Raylib_cs;
using Sparkle.csharp.graphics.util;

namespace Sparkle.csharp.content.processor; 

public class ModelProcessor : IContentProcessor {

    public object Load(string path) {
        return ModelHelper.Load(path);
    }
    
    public void Unload(object item) {
        ModelHelper.Unload((Model) item);
    }
}