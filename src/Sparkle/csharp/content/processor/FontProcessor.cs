using Raylib_cs;

namespace Sparkle.csharp.content.processor; 

public class FontProcessor : IContentProcessor {
    
    public object Load(string path) {
        return Raylib.LoadFont(path);
    }

    public void Unload(object content) {
        Raylib.UnloadFont((Font) content);
    }
}