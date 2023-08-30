using Raylib_cs;
using Sparkle.csharp.audio;

namespace Sparkle.csharp.content.processor; 

#if !HEADLESS
public class WaveProcessor : IContentProcessor {
    
    public object Load(string path) {
        return WavePlayer.Load(path);
    }

    public void Unload(object content) {
        WavePlayer.Unload((Wave) content);
    }
}
#endif