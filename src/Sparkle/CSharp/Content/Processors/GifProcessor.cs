using Sparkle.CSharp.Content.Types;
using Sparkle.CSharp.Rendering.Gifs;

namespace Sparkle.CSharp.Content.Processors;

public class GifProcessor : IContentProcessor {
    
    public object Load<T>(IContentType<T> type) {
        Gif gif = new Gif(type.Path);
        GifManager.Add(gif);
        
        return gif;
    }

    public void Unload(object item) {
        ((Gif) item).Dispose();
    }
}