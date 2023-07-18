using Raylib_cs;
using Sparkle.csharp.graphics.util;

namespace Sparkle.csharp.content.processor;

public class ImageProcessor : IContentProcessor {

    public object Load(string path) {
        return ImageHelper.LoadImage(path);
    }

    public void Unload(object content) {
        ImageHelper.UnloadImage((Image) content);
    }
}