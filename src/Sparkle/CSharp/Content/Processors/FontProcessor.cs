using Raylib_cs;
using Sparkle.CSharp.Content.Types;
using Sparkle.CSharp.Rendering.Helpers;

namespace Sparkle.CSharp.Content.Processors;

public class FontProcessor : IContentProcessor {

    public object Load<T>(IContentType<T> type) {
        return FontHelper.Load(type.Path);
    }

    public void Unload(object item) {
        FontHelper.Unload((Font) item);
    }
}