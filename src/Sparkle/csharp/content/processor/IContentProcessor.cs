using Sparkle.csharp.content.type;

namespace Sparkle.csharp.content.processor; 

public interface IContentProcessor {
    
    /// <summary>
    /// Loads content of the specified type from the given directory and returns the loaded content as an object.
    /// </summary>
    /// <param name="type">The content type to load.</param>
    /// <param name="directory">The directory from which to load the content.</param>
    /// <returns>The loaded content as an object.</returns>
    object Load(IContentType type, string directory);
    
    /// <summary>
    /// Unloads a content item.
    /// </summary>
    /// <param name="item">The content item to unload.</param>
    void Unload(object item);
}