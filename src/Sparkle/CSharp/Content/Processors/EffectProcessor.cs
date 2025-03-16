using Bliss.CSharp.Effects;
using Sparkle.CSharp.Content.Types;
using Veldrid;

namespace Sparkle.CSharp.Content.Processors;

public class EffectProcessor : IContentProcessor {
    
    /// <summary>
    /// Loads an effect from the specified content type and initializes it using the provided graphics device.
    /// </summary>
    /// <typeparam name="T">The type of the content to load, expected to be of type <see cref="Effect"/>.</typeparam>
    /// <param name="graphicsDevice">The graphics device used for loading and initializing the effect.</param>
    /// <param name="type">The content type descriptor that includes the effect's vertex layout and shader paths.</param>
    /// <returns>An instance of <see cref="Effect"/> initialized with the provided vertex layout and shader paths.</returns>
    public object Load<T>(GraphicsDevice graphicsDevice, IContentType<T> type) {
        EffectContent contentType = (EffectContent) type;
        return new Effect(graphicsDevice, contentType.VertexLayout, contentType.Path, contentType.FragPath);
    }

    /// <summary>
    /// Unloads the specified content by releasing associated resources or performing necessary cleanup.
    /// </summary>
    /// <param name="item">The content item to unload, expected to implement resource disposal or cleanup logic.</param>
    public void Unload(object item) {
        ((Effect) item).Dispose();
    }
}