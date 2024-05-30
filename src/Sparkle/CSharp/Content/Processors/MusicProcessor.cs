using Raylib_CSharp.Audio;
using Sparkle.CSharp.Content.Types;

namespace Sparkle.CSharp.Content.Processors;

public class MusicProcessor : IContentProcessor {
    
    public object Load<T>(IContentType<T> type) {
        return Music.Load(type.Path);
    }

    public void Unload(object item) {
        ((Music) item).UnloadStream();
    }
}