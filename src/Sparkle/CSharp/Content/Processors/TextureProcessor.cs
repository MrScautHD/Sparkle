using Raylib_CSharp.Textures;
using Sparkle.CSharp.Content.Types;

namespace Sparkle.CSharp.Content.Processors;

public class TextureProcessor : IContentProcessor {
    
    public object Load<T>(IContentType<T> type) {
        return Texture2D.Load(type.Path);
    }
    
    public void Unload(object item) {
        ((Texture2D) item).Unload();
    }
}