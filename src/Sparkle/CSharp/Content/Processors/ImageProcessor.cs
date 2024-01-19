using Raylib_cs;
using Sparkle.CSharp.Content.Types;
using Sparkle.CSharp.Rendering.Helpers;

namespace Sparkle.CSharp.Content.Processors;

public class ImageProcessor : IContentProcessor {
    
    public object Load<T>(IContentType<T> type) {
        return ImageHelper.Load(type.Path);
    }

    public void Unload(object item) {
        ImageHelper.Unload((Image) item);
    }
}