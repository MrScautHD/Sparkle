using Raylib_cs;
using Sparkle.csharp.audio;
using Sparkle.csharp.content.type;

namespace Sparkle.csharp.content.processor; 

public class SoundProcessor : IContentProcessor {
    
    public object Load<T>(IContentType<T> type) {
        return SoundPlayer.Load(type.Path);
    }

    public void Unload(object item) {
        SoundPlayer.Unload((Sound) item);
    }
}