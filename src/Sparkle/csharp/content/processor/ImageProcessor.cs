using Raylib_cs;
using Sparkle.csharp.content.type;
using Sparkle.csharp.graphics.helper;

namespace Sparkle.csharp.content.processor;

public class ImageProcessor : IContentProcessor {
    
    public object Load<T>(IContentType<T> type) {
        return ImageHelper.Load(type.Path);
    }

    public void Unload(object item) {
        ImageHelper.Unload((Image) item);
    }
}