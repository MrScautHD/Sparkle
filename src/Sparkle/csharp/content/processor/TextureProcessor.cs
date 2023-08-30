using Raylib_cs;
using Sparkle.csharp.graphics.util;

namespace Sparkle.csharp.content.processor; 

#if !HEADLESS
public class TextureProcessor : IContentProcessor {
    
    public object Load(string path) {
        return TextureHelper.Load(path);
    }
    
    public void Unload(object content) {
        TextureHelper.Unload((Texture2D) content);
    }
}
#endif