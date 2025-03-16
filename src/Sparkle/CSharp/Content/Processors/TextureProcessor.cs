using Bliss.CSharp.Textures;
using Sparkle.CSharp.Content.Types;
using Veldrid;

namespace Sparkle.CSharp.Content.Processors;

public class TextureProcessor : IContentProcessor {
    
    /// <summary>
    /// Loads a texture resource of type <typeparamref name="T"/> using the provided graphics device and content type descriptor.
    /// </summary>
    /// <typeparam name="T">The type of the texture resource to load.</typeparam>
    /// <param name="graphicsDevice">The graphics device used to create and manage the texture resource.</param>
    /// <param name="type">The content type descriptor, containing information such as the texture path and configuration.</param>
    /// <returns>The loaded texture resource as an object.</returns>
    public object Load<T>(GraphicsDevice graphicsDevice, IContentType<T> type) {
        TextureContent contentType = (TextureContent) type;
        return new Texture2D(graphicsDevice, contentType.Path, contentType.Mipmap, contentType.UseSrgbFormat);
    }

    /// <summary>
    /// Unloads and disposes of a previously loaded texture resource.
    /// </summary>
    /// <param name="item">The texture resource to unload, typically an instance of <see cref="Texture2D"/>.</param>
    public void Unload(object item) {
        ((Texture2D) item).Dispose();
    }
}