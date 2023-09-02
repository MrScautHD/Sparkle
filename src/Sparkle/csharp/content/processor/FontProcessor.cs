using Raylib_cs;
using Sparkle.csharp.graphics.util;

namespace Sparkle.csharp.content.processor; 

public class FontProcessor : IContentProcessor {
    
    public object Load(string path) {
        return FontHelper.Load(path);
    }

    public void Unload(object content) {
        FontHelper.Unload((Font) content);
    }
}