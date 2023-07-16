using Raylib_cs;
using Sparkle.csharp.graphics.util;

namespace Sparkle.csharp.content.processor; 

public class ModelProcessor : IContentProcessor {

    public object Load(string path) {
        return ModelHelper.LoadModel(path);
    }
    
    public void Unload(object content) {
        ModelHelper.UnloadModel((Model) content);
    }
}