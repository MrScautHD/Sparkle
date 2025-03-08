using Bliss.CSharp.Fonts;
using Sparkle.CSharp.Content.Types;
using Veldrid;

namespace Sparkle.CSharp.Content.Processors;

public class FontProcessor : IContentProcessor {

    public object Load<T>(GraphicsDevice graphicsDevice, IContentType<T> type) {
        return new Font(type.Path);
    }

    public void Unload(object item) {
        ((Font) item).Dispose();
    }
}