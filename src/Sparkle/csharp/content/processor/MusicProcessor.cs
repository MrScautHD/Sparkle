using Raylib_cs;
using Sparkle.csharp.audio;
using Sparkle.csharp.content.type;

namespace Sparkle.csharp.content.processor; 

public class MusicProcessor : IContentProcessor {
    
    public object Load(IContentType type, string directory) {
        return MusicPlayer.LoadStream(directory + type.Path);
    }

    public void Unload(object item) {
        MusicPlayer.UnloadStream((Music) item);
    }

    public Type GetContentType() {
        return typeof(MusicContent);
    }
}