using Raylib_cs;
using Sparkle.csharp.audio;
using Sparkle.csharp.content.type;

namespace Sparkle.csharp.content.processor; 

public class WaveProcessor : IContentProcessor {
    
    public object Load(IContentType type, string directory) {
        return WavePlayer.Load(directory + type.Path);
    }

    public void Unload(object item) {
        WavePlayer.Unload((Wave) item);
    }

    public Type GetContentType() {
        return typeof(WaveContent);
    }
}