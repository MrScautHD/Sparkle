using Raylib_cs;
using Sparkle.csharp.graphics.util;

namespace Sparkle.csharp.content.processor; 

public class TextureProcessor : IContentProcessor {
    
    public object Load(string path) {
        return TextureHelper.Load(path);
    }
    
    public void Unload(object item) {
        TextureHelper.Unload((Texture2D) item);
    }
}