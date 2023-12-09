using Raylib_cs;
using Sparkle.csharp.audio;
using Sparkle.csharp.content.type;

namespace Sparkle.csharp.content.processor; 

public class WaveProcessor : IContentProcessor {
    
    public object Load<T>(IContentType<T> type) {
        return WavePlayer.Load(type.Path);
    }

    public void Unload(object item) {
        WavePlayer.Unload((Wave) item);
    }
}