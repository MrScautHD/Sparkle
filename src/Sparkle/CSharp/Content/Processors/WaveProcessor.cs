using Raylib_CSharp.Audio;
using Sparkle.CSharp.Content.Types;

namespace Sparkle.CSharp.Content.Processors;

public class WaveProcessor : IContentProcessor {
    
    public object Load<T>(IContentType<T> type) {
        return Wave.Load(type.Path);
    }

    public void Unload(object item) {
        Wave.Unload((Wave) item);
    }
}