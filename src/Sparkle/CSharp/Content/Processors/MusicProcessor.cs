using Raylib_cs;
using Sparkle.CSharp.Audio;
using Sparkle.CSharp.Content.Types;

namespace Sparkle.CSharp.Content.Processors;

public class MusicProcessor : IContentProcessor {
    
    public object Load<T>(IContentType<T> type) {
        return MusicPlayer.LoadStream(type.Path);
    }

    public void Unload(object item) {
        MusicPlayer.UnloadStream((Music) item);
    }
}