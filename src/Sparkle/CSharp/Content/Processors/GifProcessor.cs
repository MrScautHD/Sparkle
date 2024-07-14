using Sparkle.CSharp.Content.Types;
using Sparkle.CSharp.Gifs;

namespace Sparkle.CSharp.Content.Processors;

public class GifProcessor : IContentProcessor {
    
    public object Load<T>(IContentType<T> type) {
        Gif gif = new Gif(type.Path);
        gif.Init();
        
        return gif;
    }

    public void Unload(object item) {
        ((Gif) item).Dispose();
    }
}