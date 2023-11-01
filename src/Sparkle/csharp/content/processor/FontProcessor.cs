using Raylib_cs;
using Sparkle.csharp.content.type;
using Sparkle.csharp.graphics.util;

namespace Sparkle.csharp.content.processor; 

public class FontProcessor : IContentProcessor {
    
    public object Load(IContentType type, string directory) {
        return FontHelper.Load(directory + type.Path);
    }

    public void Unload(object item) {
        FontHelper.Unload((Font) item);
    }
}