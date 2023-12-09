using Raylib_cs;
using Sparkle.csharp.content.type;
using Sparkle.csharp.graphics.helper;

namespace Sparkle.csharp.content.processor; 

public class TextureProcessor : IContentProcessor {
    
    public object Load<T>(IContentType<T> type) {
        return TextureHelper.Load(type.Path);
    }
    
    public void Unload(object item) {
        TextureHelper.Unload((Texture2D) item);
    }
}