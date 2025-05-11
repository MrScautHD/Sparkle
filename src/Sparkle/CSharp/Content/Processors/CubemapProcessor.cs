using Bliss.CSharp.Textures.Cubemaps;
using Sparkle.CSharp.Content.Types;
using Veldrid;

namespace Sparkle.CSharp.Content.Processors;

public class CubemapProcessor : IContentProcessor {
    
    /// <summary>
    /// Loads a cubemap resource into memory using the provided graphics device and content type.
    /// </summary>
    /// <typeparam name="T">The type of content that will be loaded.</typeparam>
    /// <param name="graphicsDevice">The graphics device used for creating GPU resources.</param>
    /// <param name="type">The content type containing metadata about the resource to be loaded.</param>
    /// <returns>An object representing the loaded cubemap resource.</returns>
    public object Load<T>(GraphicsDevice graphicsDevice, IContentType<T> type) {
        CubemapContent contentType = (CubemapContent) type;
        return new Cubemap(graphicsDevice, contentType.Path, contentType.Layout, contentType.Mipmap, contentType.UseSrgbFormat);
    }

    /// <summary>
    /// Unloads a previously loaded cubemap resource and releases its associated GPU resources.
    /// </summary>
    /// <param name="item">The cubemap resource to be unloaded.</param>
    public void Unload(object item) {
        ((Cubemap) item).Dispose();
    }
}