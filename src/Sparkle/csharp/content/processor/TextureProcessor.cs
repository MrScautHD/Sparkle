using Raylib_cs;
using Sparkle.csharp.content.type;
using Sparkle.csharp.graphics.helper;

namespace Sparkle.csharp.content.processor; 

public class TextureProcessor : IContentProcessor {
    
    public object Load(IContentType type, string directory) {
        return TextureHelper.Load(directory + type.Path);
    }
    
    public void Unload(object item) {
        TextureHelper.Unload((Texture2D) item);
    }

    public Type GetContentType() {
        return typeof(TextureContent);
    }
}