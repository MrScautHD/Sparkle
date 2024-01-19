using Raylib_cs;
using Sparkle.CSharp.Content.Types;
using Sparkle.CSharp.Rendering.Helpers;

namespace Sparkle.CSharp.Content.Processors; 

public class TextureProcessor : IContentProcessor {
    
    public object Load<T>(IContentType<T> type) {
        return TextureHelper.Load(type.Path);
    }
    
    public void Unload(object item) {
        TextureHelper.Unload((Texture2D) item);
    }
}