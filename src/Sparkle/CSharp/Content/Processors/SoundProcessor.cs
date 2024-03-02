using Raylib_cs;
using Sparkle.CSharp.Audio;
using Sparkle.CSharp.Content.Types;

namespace Sparkle.CSharp.Content.Processors;

public class SoundProcessor : IContentProcessor {
    
    public object Load<T>(IContentType<T> type) {
        return SoundPlayer.Load(type.Path);
    }

    public void Unload(object item) {
        SoundPlayer.Unload((Sound) item);
    }
}