using Raylib_cs;
using Sparkle.csharp.graphics.util;

namespace Sparkle.csharp.content.processor; 

public class FontProcessor : IContentProcessor {
    
    public object Load(string path) {
        return TextHelper.LoadFont(path);
    }

    public void Unload(object content) {
        TextHelper.UnloadFont((Font) content);
    }
}