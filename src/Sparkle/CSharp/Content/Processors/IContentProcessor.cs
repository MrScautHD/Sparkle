using Sparkle.CSharp.Content.Types;
using Veldrid;

namespace Sparkle.CSharp.Content.Processors;

public interface IContentProcessor {
    
    /// <summary>
    /// Loads the specified content of type <typeparamref name="T"/> using the appropriate content processor.
    /// </summary>
    /// <typeparam name="T">The type of the content to load.</typeparam>
    /// <param name="graphicsDevice">The graphics device used for loading content.</param>
    /// <param name="type">The content type descriptor that includes information about the path and content.</param>
    /// <returns>The loaded content item of type <typeparamref name="T"/>.</returns>
    object Load<T>(GraphicsDevice graphicsDevice, IContentType<T> type);

    /// <summary>
    /// Unloads the specified content item and releases any resources associated with it.
    /// </summary>
    /// <param name="item">The content item to unload.</param>
    void Unload(object item);
}