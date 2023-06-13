using Raylib_cs;

namespace Sparkle.csharp.content.processor;

public class ImageProcessor : IContentProcessor {

    public object Load(string path) {
        return Raylib.LoadImage(path);
    }

    public void UnLoad(object content) {
        Raylib.UnloadImage((Image) content);
    }
}