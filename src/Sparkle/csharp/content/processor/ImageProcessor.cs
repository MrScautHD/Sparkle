using Raylib_cs;
using Sparkle.csharp.graphics.util;

namespace Sparkle.csharp.content.processor;

#if !HEADLESS
public class ImageProcessor : IContentProcessor {
    
    public object Load(string path) {
        return ImageHelper.Load(path);
    }

    public void Unload(object content) {
        ImageHelper.Unload((Image) content);
    }
}
#endif