using Raylib_cs;
using Sparkle.csharp.graphics.util;

namespace Sparkle.csharp.content.processor; 

public class TextureProcessor : IContentProcessor {
    
    public object Load(string path) {
        return TextureHelper.LoadTexture(path);
    }
    
    public void Unload(object content) {
        TextureHelper.UnloadTexture((Texture2D) content);
    }
}