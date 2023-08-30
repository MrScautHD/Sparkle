using Raylib_cs;
using Sparkle.csharp.audio;

namespace Sparkle.csharp.content.processor; 

#if !HEADLESS
public class MusicProcessor : IContentProcessor {
    
    public object Load(string path) {
        return MusicPlayer.LoadStream(path);
    }

    public void Unload(object content) {
        MusicPlayer.UnloadStream((Music) content);
    }
}
#endif