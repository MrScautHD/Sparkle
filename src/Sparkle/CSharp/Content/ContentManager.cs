using System.Diagnostics.CodeAnalysis;
using Bliss.CSharp;
using Bliss.CSharp.Fonts;
using Bliss.CSharp.Geometry;
using Bliss.CSharp.Logging;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Textures.Cubemaps;
using MiniAudioEx.Core.StandardAPI;
using Sparkle.CSharp.Content.Processors;
using Sparkle.CSharp.Content.Types;
using Veldrid;

namespace Sparkle.CSharp.Content;

public class ContentManager : Disposable {
    
    /// <summary>
    /// The graphics device used for loading graphical assets.
    /// </summary>
    public GraphicsDevice GraphicsDevice { get; private set; }
    
    /// <summary>
    /// Stores loaded content items.
    /// </summary>
    private Dictionary<string, object> _content;
    
    /// <summary>
    /// List of collections that have been loaded.
    /// </summary>
    private Dictionary<string, ContentCollection> _collections;
    
    /// <summary>
    /// Maps content types to their respective content processors.
    /// </summary>
    private Dictionary<Type, IContentProcessor> _processors;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentManager"/> class.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used for loading assets.</param>
    public ContentManager(GraphicsDevice graphicsDevice) {
        this.GraphicsDevice = graphicsDevice;
        this._content = new Dictionary<string, object>();
        this._collections = new Dictionary<string, ContentCollection>();
        this._processors = new Dictionary<Type, IContentProcessor>();
        
        // Add default processors.
        this.AddProcessors(typeof(Font), new FontProcessor());
        this.AddProcessors(typeof(Texture2D), new TextureProcessor());
        this.AddProcessors(typeof(Cubemap), new CubemapProcessor());
        this.AddProcessors(typeof(Model), new ModelProcessor());
        this.AddProcessors(typeof(AudioClip), new AudioClipProcessor());
    }
    
    /// <summary>
    /// Retrieves all content processors currently registered in the content manager.
    /// </summary>
    /// <returns>An enumerable collection of the registered content processors.</returns>
    public IEnumerable<IContentProcessor> GetProcessors() {
        return this._processors.Values;
    }
    
    /// <summary>
    /// Determines whether a content processor exists for the specified type.
    /// </summary>
    /// <param name="type">The type to check for an associated content processor.</param>
    /// <returns>True if a processor exists for the specified type; otherwise, false.</returns>
    public bool HasProcessor(Type type) {
        return this._processors.ContainsKey(type);
    }
    
    /// <summary>
    /// Retrieves a content processor for the specified type, if one exists.
    /// </summary>
    /// <param name="type">The type of content for which the processor is requested.</param>
    /// <returns>The content processor for the specified type, or null if no processor is registered.</returns>
    public IContentProcessor? GetProcessor(Type type) {
        if (!this.TryGetProcessor(type, out IContentProcessor? processor)) {
            return null;
        }

        return processor;
    }
    
    /// <summary>
    /// Attempts to retrieve a content processor for a specified type.
    /// </summary>
    /// <param name="type">The type of content.</param>
    /// <param name="processor">The found content processor, or null if not found.</param>
    /// <returns>True if a processor was found, otherwise false.</returns>
    public bool TryGetProcessor(Type type, [NotNullWhen(true)] out IContentProcessor? processor) {
        return this._processors.TryGetValue(type, out processor);
    }
    
    /// <summary>
    /// Adds a new content processor for a specific content type.
    /// </summary>
    /// <param name="type">The type of content the processor will handle.</param>
    /// <param name="processor">The content processor to associate with the specified type.</param>
    public void AddProcessors(Type type, IContentProcessor processor) {
        if (!this.TryAddProcessors(type, processor)) {
            throw new Exception($"ContentProcessor for type: [{type}] already exists.");
        }
    }
    
    /// <summary>
    /// Attempts to add a content processor for a specific type.
    /// </summary>
    /// <param name="type">The type the processor handles.</param>
    /// <param name="processor">The content processor instance.</param>
    /// <returns>True if successfully added, otherwise false.</returns>
    public bool TryAddProcessors(Type type, IContentProcessor processor) {
        return this._processors.TryAdd(type, processor);
    }
    
    /// <summary>
    /// Loads content using an appropriate processor.
    /// </summary>
    /// <typeparam name="T">The item of content to load.</typeparam>
    /// <param name="item">The content item to load.</param>
    /// <returns>The loaded content.</returns>
    public T Load<T>(IContentType<T> item) {
        if (this._content.TryGetValue(item.Path, out object? cachedItem)) {
            return (T) cachedItem;
        }
        
        if (!this.TryGetProcessor(typeof(T), out IContentProcessor? processor)) {
            throw new Exception($"Failed to find a valid ContentProcessor for item: [{typeof(T)}]");
        }
        
        T loadedItem = (T) processor.Load(this.GraphicsDevice, item);
        
        if (loadedItem == null) {
            throw new Exception($"Failed to load content of type [{typeof(T).Name}] from path: {item.Path}");
        }
        
        item.OnLoaded?.Invoke(loadedItem);
        
        this._content.Add(item.Path, loadedItem);
        return loadedItem;
    }
    
    /// <summary>
    /// Unloads a content item by its path, freeing associated resources.
    /// </summary>
    /// <typeparam name="T">The type of content to unload.</typeparam>
    /// <param name="path">The path of the item to unload.</param>
    public void Unload<T>(string path) {
        if (this._content.TryGetValue(path, out object? item)) {
            if (!this.TryGetProcessor(typeof(T), out IContentProcessor? processor)) {
                Logger.Error($"Failed to find a valid ContentProcessor for type: [{typeof(T)}]");
            }
            else {
                processor.Unload(item);
                this._content.Remove(path);
            }
        }
        else {
            Logger.Warn($"Attempted to unload an item at path: [{path}] that was not loaded in the ContentManager.");
        }
    }
    
    /// <summary>
    /// Unloads a content item by its instance, freeing associated resources.
    /// </summary>
    /// <typeparam name="T">The type of content to unload.</typeparam>
    /// <param name="item">The item instance to unload.</param>
    public void Unload<T>(T item) {
        foreach (KeyValuePair<string, object> content in this._content) {
            if (ReferenceEquals(content.Value, item)) {
                if (this.TryGetProcessor(typeof(T), out IContentProcessor? processor)) {
                    processor.Unload(item);
                    this._content.Remove(content.Key);
                }
                else {
                    Logger.Error($"Failed to find a valid ContentProcessor for type: [{typeof(T)}]");
                }
                
                return;
            }
        }
        
        Logger.Warn($"Attempted to unload an item of type: [{typeof(T)}] that was not found in the ContentManager.");
    }
    
    /// <summary>
    /// Creates and registers a new <see cref="ContentCollection"/> with the specified key and items.
    /// </summary>
    /// <param name="key">The unique key used to identify the collection.</param>
    /// <param name="items">The array of content items to include in the collection.</param>
    /// <returns>The newly created <see cref="ContentCollection"/>.</returns>
    /// <exception cref="Exception">Thrown when a collection with the provided key already exists.</exception>
    public ContentCollection DefineCollection(string key, IContentType[] items) {
        if (this._collections.ContainsKey(key)) {
            throw new Exception($"A collection with the key [{key}] already exists.");
        }
        
        ContentCollection collection = new ContentCollection(key, items);
        this._collections.Add(key, collection);
        
        return collection;
    }
    
    /// <summary>
    /// Removes a content collection associated with the specified key from the manager.
    /// </summary>
    /// <param name="key">The key of the content collection to be removed.</param>
    public void UndefineCollection(string key) {
        if (!this._collections.Remove(key)) {
            Logger.Warn($"Failed to undefine collection. A collection with the key [{key}] was not found.");
        }
    }
    
    /// <summary>
    /// Loads the specified content collection by key, ensuring all contained items are processed and loaded.
    /// </summary>
    /// <param name="key">The unique identifier for the content collection to load.</param>
    /// <exception cref="Exception">Thrown if the specified collection is not defined, a content processor for an item is not found, or an item fails to load.</exception>
    internal void LoadCollection(string key) {
        if (!this._collections.TryGetValue(key, out ContentCollection? collection)) {
            throw new Exception($"Failed to load collection: [{key}]. It has not been defined.");
        }
        
        // Check if the collection is already loaded.
        if (collection.IsLoaded) {
            return;
        }
        
        // Mark the collection as loaded.
        collection.IsLoaded = true;
        
        foreach (IContentType item in collection.Items) {
            if (!this.TryGetProcessor(item.ContentType, out IContentProcessor? processor)) {
                throw new Exception($"Failed to find a valid ContentProcessor for item: [{item.ContentType}] in collection: [{key}]");
            }
            
            object loadedItem = processor.Load(this.GraphicsDevice, (dynamic) item);
            
            if (loadedItem == null) {
                throw new Exception($"Failed to load content for item: [{item.ContentType}] in collection: [{key}] from path: {item.Path}");
            }
            
            item.OnLoaded?.Invoke(loadedItem);
            collection.AddContent(item.Path, loadedItem);
        }
    }
    
    /// <summary>
    /// Unloads a content collection and releases its associated resources.
    /// </summary>
    /// <param name="key">The unique identifier of the content collection to unload.</param>
    internal void UnloadCollection(string key) {
        if (!this._collections.TryGetValue(key, out ContentCollection? collection)) {
            Logger.Warn($"Attempted to unload collection: [{key}] that was not found.");
            return;
        }
        
        // Check if the collection is already unloaded.
        if (!collection.IsLoaded) {
            return;
        }
        
        // Mark the collection as unloaded.
        collection.IsLoaded = false;
        
        foreach (IContentType item in collection.Items) {
            if (collection.TryGet(item.Path, out object? loadedItem)) {
                if (this.TryGetProcessor(item.ContentType, out IContentProcessor? processor)) {
                    processor.Unload(loadedItem);
                }
            }
        }
        
        collection.ClearContent();
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            foreach (object item in this._content.Values) {
                if (this.TryGetProcessor(item.GetType(), out IContentProcessor? processor)) {
                    processor.Unload(item);
                }
            }
            
            this._content.Clear();
        }
    }
}