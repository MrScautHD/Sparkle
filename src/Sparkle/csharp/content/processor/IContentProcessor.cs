namespace Sparkle.csharp.content.processor; 

public interface IContentProcessor {
    
    /// <summary>
    /// Loads a content item from the specified path.
    /// </summary>
    /// <param name="path">The path to the content item.</param>
    /// <returns>The loaded content item as an object.</returns>
    object Load(string path);
    
    /// <summary>
    /// Unloads a content item.
    /// </summary>
    /// <param name="item">The content item to unload.</param>
    void Unload(object item);
}