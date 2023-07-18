using Raylib_cs;
using Sparkle.csharp.audio;

namespace Sparkle.csharp.content.processor; 

public class WaveProcessor : IContentProcessor {
    
    public object Load(string path) {
        return AudioPlayer.LoadWave(path);
    }

    public void Unload(object content) {
        AudioPlayer.UnloadWave((Wave) content);
    }
}