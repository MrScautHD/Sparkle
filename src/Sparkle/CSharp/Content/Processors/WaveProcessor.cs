using Raylib_cs;
using Sparkle.CSharp.Audio;
using Sparkle.CSharp.Content.Types;

namespace Sparkle.CSharp.Content.Processors; 

public class WaveProcessor : IContentProcessor {
    
    public object Load<T>(IContentType<T> type) {
        return WavePlayer.Load(type.Path);
    }

    public void Unload(object item) {
        WavePlayer.Unload((Wave) item);
    }
}