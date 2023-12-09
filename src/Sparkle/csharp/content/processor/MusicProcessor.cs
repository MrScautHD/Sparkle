using Raylib_cs;
using Sparkle.csharp.audio;
using Sparkle.csharp.content.type;

namespace Sparkle.csharp.content.processor; 

public class MusicProcessor : IContentProcessor {
    
    public object Load<T>(IContentType<T> type) {
        return MusicPlayer.LoadStream(type.Path);
    }

    public void Unload(object item) {
        MusicPlayer.UnloadStream((Music) item);
    }
}