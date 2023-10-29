using Raylib_cs;
using Sparkle.csharp.content.type;
using Sparkle.csharp.graphics.util;

namespace Sparkle.csharp.content.processor;

public class ImageProcessor : IContentProcessor {
    
    public object Load(IContentType type, string directory) {
        return ImageHelper.Load(directory + ((ImageContent) type).Path);
    }

    public void Unload(object item) {
        ImageHelper.Unload((Image) item);
    }
}