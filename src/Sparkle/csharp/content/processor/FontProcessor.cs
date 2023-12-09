using Raylib_cs;
using Sparkle.csharp.content.type;
using Sparkle.csharp.graphics.helper;

namespace Sparkle.csharp.content.processor; 

public class FontProcessor : IContentProcessor {

    public object Load<T>(IContentType<T> type) {
        return FontHelper.Load(type.Path);
    }

    public void Unload(object item) {
        FontHelper.Unload((Font) item);
    }
}