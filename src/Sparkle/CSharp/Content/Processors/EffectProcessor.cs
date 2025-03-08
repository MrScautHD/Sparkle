using Bliss.CSharp.Effects;
using Sparkle.CSharp.Content.Types;
using Veldrid;

namespace Sparkle.CSharp.Content.Processors;

public class EffectProcessor : IContentProcessor {
    
    public object Load<T>(GraphicsDevice graphicsDevice, IContentType<T> type) {
        EffectContent contentType = (EffectContent) type;
        return new Effect(graphicsDevice, contentType.VertexLayout, contentType.Path, contentType.FragPath);
    }

    public void Unload(object item) {
        ((Effect) item).Dispose();
    }
}