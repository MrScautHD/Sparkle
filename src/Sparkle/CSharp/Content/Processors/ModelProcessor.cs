using Bliss.CSharp.Geometry;
using Sparkle.CSharp.Content.Types;
using Veldrid;

namespace Sparkle.CSharp.Content.Processors;

public class ModelProcessor : IContentProcessor {
    
    /// <summary>
    /// Loads a model using the given graphics device and content type descriptor.
    /// </summary>
    /// <typeparam name="T">The type of the content to be loaded.</typeparam>
    /// <param name="graphicsDevice">The graphics device used to load the model.</param>
    /// <param name="type">The content type descriptor that contains information such as the path, material loading option, and UV flip configuration.</param>
    /// <returns>A loaded model instance.</returns>
    public object Load<T>(GraphicsDevice graphicsDevice, IContentType<T> type) {
        ModelContent contentType = (ModelContent) type;
        return Model.Load(graphicsDevice, contentType.Path, contentType.LoadMaterial, contentType.FlipUv);
    }

    /// <summary>
    /// Unloads the specified model and releases any associated resources.
    /// </summary>
    /// <param name="item">The model instance to unload.</param>
    public void Unload(object item) {
        ((Model) item).Dispose();
    }
}