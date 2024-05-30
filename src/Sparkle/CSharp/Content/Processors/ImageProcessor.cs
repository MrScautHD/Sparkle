using Raylib_CSharp.Images;
using Sparkle.CSharp.Content.Types;

namespace Sparkle.CSharp.Content.Processors;

public class ImageProcessor : IContentProcessor {
    
    public object Load<T>(IContentType<T> type) {
        return Image.Load(type.Path);
    }

    public void Unload(object item) {
        ((Image) item).Unload();
    }
}