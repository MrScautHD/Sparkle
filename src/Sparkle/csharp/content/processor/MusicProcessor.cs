using Raylib_cs;
using Sparkle.csharp.audio;

namespace Sparkle.csharp.content.processor; 

public class MusicProcessor : IContentProcessor {
    
    public object Load(string path) {
        return MusicPlayer.LoadStream(path);
    }

    public void Unload(object item) {
        MusicPlayer.UnloadStream((Music) item);
    }
}