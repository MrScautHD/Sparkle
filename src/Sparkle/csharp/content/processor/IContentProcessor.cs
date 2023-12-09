using Sparkle.csharp.content.type;

namespace Sparkle.csharp.content.processor; 

public interface IContentProcessor {
    
    /// <summary>
    /// Loads the content of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of content to load.</typeparam>
    /// <param name="type">The content type from which to retrieve the content.</param>
    /// <returns>The loaded content of the specified type.</returns>
    object Load<T>(IContentType<T> type);
    
    /// <summary>
    /// Unloads a content item.
    /// </summary>
    /// <param name="item">The content item to unload.</param>
    void Unload(object item);
}