using Bliss.CSharp.Geometry;
using Sparkle.CSharp.Content.Types;
using Veldrid;

namespace Sparkle.CSharp.Content.Processors;

public class ModelProcessor : IContentProcessor {

    public object Load<T>(GraphicsDevice graphicsDevice, IContentType<T> type) {
        return Model.Load(graphicsDevice, type.Path,);
    }
    
    public void Unload(object item) {
        ((Model) item).Unload();
    }
}