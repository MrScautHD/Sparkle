using System.Diagnostics.CodeAnalysis;
using Sparkle.CSharp.Content.Types;

namespace Sparkle.CSharp.Content;

public class ContentCollection {
    
    /// <summary>
    /// Gets the unique key that identifies this content collection.
    /// </summary>
    public string Key { get; private set; }
    
    /// <summary>
    /// Gets the content type definitions associated with this collection.
    /// </summary>
    public IContentType[] Items { get; private set; }
    
    /// <summary>
    /// Gets a value indicating whether this collection is currently loaded.
    /// </summary>
    public bool IsLoaded { get; internal set; }
    
    /// <summary>
    /// Stores the loaded content instances mapped by their asset path.
    /// </summary>
    private Dictionary<string, object> _content;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentCollection"/> class.
    /// </summary>
    /// <param name="key">The unique identifier used to reference this collection.</param>
    /// <param name="items">The content definitions that belong to this collection.</param>
    internal ContentCollection(string key, IContentType[] items) {
        this.Key = key;
        this.Items = items;
        this._content = new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Adds a loaded content item to the collection.
    /// </summary>
    /// <param name="key"> The asset path used to reference the content.</param>
    /// <param name="item"> The loaded content instance.</param>
    internal void AddContent(string key, object item) {
        this._content.Add(key, item);
    }
    
    /// <summary>
    /// Removes all loaded content from the collection.
    /// </summary>
    internal void ClearContent() {
        this._content.Clear();
    }
    
    /// <summary>
    /// Retrieves a content item of the specified type from the collection using the given path.
    /// </summary>
    /// <typeparam name="T">The type of the content item to retrieve.</typeparam>
    /// <param name="path">The unique path used to identify the content item within the collection.</param>
    /// <returns>The content item of type <typeparamref name="T"/> if it exists in the collection; otherwise, an exception is thrown.</returns>
    /// <exception cref="Exception">Thrown when the content item cannot be retrieved, either because the collection is not loaded or the item does not exist.</exception>
    public T Get<T>(string path) {
        if (!this.TryGet(path, out T? storedItem)) {
            throw new Exception($"Failed to retrieve content at path: [{path}]. The collection might not be loaded or the item doesn't exist.");
        }
        
        return storedItem;
    }
    
    /// <summary>
    /// Tries to retrieve a content item from the collection by its asset path.
    /// </summary>
    /// <typeparam name="T">The type of the content being retrieved.</typeparam>
    /// <param name="path">The asset path used to identify the content item.</param>
    /// <param name="item">When this method returns, contains the content item if found; otherwise, the default value of <typeparamref name="T"/>.</param>
    /// <returns><c>true</c> if the content item was found; otherwise, <c>false</c>.</returns>
    public bool TryGet<T>(string path, [NotNullWhen(true)] out T? item) {
        if (this._content.TryGetValue(path, out object? storedItem)) {
            item = (T) storedItem;
            return true;
        }
        else {
            item = default;
            return false;
        }
    }
}