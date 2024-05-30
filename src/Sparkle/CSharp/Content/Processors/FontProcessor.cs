using Raylib_CSharp.Fonts;
using Sparkle.CSharp.Content.Types;

namespace Sparkle.CSharp.Content.Processors;

public class FontProcessor : IContentProcessor {

    public object Load<T>(IContentType<T> type) {
        return Font.Load(type.Path);
    }

    public void Unload(object item) {
        ((Font) item).Unload();
    }
}