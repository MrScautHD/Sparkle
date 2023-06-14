using Raylib_cs;

namespace Sparkle.csharp.content.processor; 

public class TextureProcessor : IContentProcessor {
    
    public object Load(string path) {
        return Raylib.LoadTexture(path);
    }
    
    public void Unload(object content) {
        Raylib.UnloadTexture((Texture2D) content);
    }
}