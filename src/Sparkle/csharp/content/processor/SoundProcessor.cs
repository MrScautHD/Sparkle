using Raylib_cs;

namespace Sparkle.csharp.content.processor; 

public class SoundProcessor : IContentProcessor {
    
    public object Load(string path) {
        return Raylib.LoadSound(path);
    }

    public void Unload(object content) {
        Raylib.UnloadSound((Sound) content);
    }
}