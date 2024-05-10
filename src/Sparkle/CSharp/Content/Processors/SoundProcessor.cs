using Raylib_CSharp.Audio;
using Sparkle.CSharp.Content.Types;

namespace Sparkle.CSharp.Content.Processors;

public class SoundProcessor : IContentProcessor {
    
    public object Load<T>(IContentType<T> type) {
        return Sound.Load(type.Path);
    }

    public void Unload(object item) {
        Sound.Unload((Sound) item);
    }
}