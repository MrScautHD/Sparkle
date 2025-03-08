using Bliss.CSharp.Textures;
using Sparkle.CSharp.Content.Types;
using Veldrid;

namespace Sparkle.CSharp.Content.Processors;

public class TextureProcessor : IContentProcessor {
    
    public object Load<T>(GraphicsDevice graphicsDevice, IContentType<T> type) {
        TextureContent contentType = (TextureContent) type;
        return new Texture2D(graphicsDevice, contentType.Path, contentType.Mipmap, contentType.UseSrgbFormat);
    }

    public void Unload(object item) {
        ((Texture2D) item).Dispose();
    }
}