using Raylib_cs;
using Sparkle.csharp.graphics.util;

namespace Sparkle.csharp.content.processor; 

public class FontProcessor : IContentProcessor {
    
    public object Load(string path) {
        return FontHelper.Load(path);
    }

    public void Unload(object item) {
        FontHelper.Unload((Font) item);
    }
}