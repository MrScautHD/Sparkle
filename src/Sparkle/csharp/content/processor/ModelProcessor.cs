using Raylib_cs;
using Sparkle.csharp.graphics.util;

namespace Sparkle.csharp.content.processor; 

#if !HEADLESS
public class ModelProcessor : IContentProcessor {

    public object Load(string path) {
        return ModelHelper.Load(path);
    }
    
    public void Unload(object content) {
        ModelHelper.Unload((Model) content);
    }
}
#endif