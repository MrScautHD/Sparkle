using Raylib_cs;
using Sparkle.csharp.audio;
using Sparkle.csharp.content.type;

namespace Sparkle.csharp.content.processor; 

public class SoundProcessor : IContentProcessor {
    
    public object Load(IContentType type, string directory) {
        return SoundPlayer.Load(directory + type.Path);
    }

    public void Unload(object item) {
        SoundPlayer.Unload((Sound) item);
    }
}