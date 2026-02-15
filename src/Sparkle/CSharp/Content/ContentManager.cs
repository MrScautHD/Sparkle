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
    /// A collection used to store content that persists across scenes or levels.
    /// </summary>
    private Dictionary<string, object> _persistentContent;

    /// <summary>
    /// Contains content that is specific to individual scenes, allowing for efficient management and unloading when the scene is no longer needed.
    /// </summary>
    private Dictionary<string, object> _sceneContent;
    
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
        this._persistentContent = new Dictionary<string, object>();
        this._sceneContent = new Dictionary<string, object>();
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
    /// <param name="persistent">Whether the content should be stored in the global cache or the scene cache.</param>
    /// <returns>The loaded content.</returns>
    public T Load<T>(IContentType<T> item, bool persistent = true) {
        Dictionary<string, object> cache = persistent ? this._persistentContent : this._sceneContent;
        
        if (cache.TryGetValue(item.Path, out object? cachedItem)) {
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
        
        cache.Add(item.Path, loadedItem);
        return loadedItem;
    }
    
    /// <summary>
    /// Unloads a content item by its path, freeing associated resources.
    /// </summary>
    /// <typeparam name="T">The type of content to unload.</typeparam>
    /// <param name="path">The path of the item to unload.</param>
    public void Unload<T>(string path) {
        if (this._persistentContent.TryGetValue(path, out object? persistentItem)) {
            if (this.TryGetProcessor(typeof(T), out IContentProcessor? processor)) {
                processor.Unload(persistentItem);
                this._persistentContent.Remove(path);
            }
            else {
                Logger.Error($"Failed to find a valid ContentProcessor for type: [{typeof(T)}]");
            }
        }
        else if (this._sceneContent.TryGetValue(path, out object? sceneItem)) {
            if (this.TryGetProcessor(typeof(T), out IContentProcessor? processor)) {
                processor.Unload(sceneItem);
                this._sceneContent.Remove(path);
            }
            else {
                Logger.Error($"Failed to find a valid ContentProcessor for type: [{typeof(T)}]");
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
        foreach (KeyValuePair<string, object> content in this._persistentContent) {
            if (ReferenceEquals(content.Value, item)) {
                if (this.TryGetProcessor(typeof(T), out IContentProcessor? processor)) {
                    processor.Unload(item);
                    this._persistentContent.Remove(content.Key);
                }
                else {
                    Logger.Error($"Failed to find a valid ContentProcessor for type: [{typeof(T)}]");
                }
                
                return;
            }
        }
        
        foreach (KeyValuePair<string, object> content in this._sceneContent) {
            if (ReferenceEquals(content.Value, item)) {
                if (this.TryGetProcessor(typeof(T), out IContentProcessor? processor)) {
                    processor.Unload(item);
                    this._sceneContent.Remove(content.Key);
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
    /// Clears all content stored in the scene cache.
    /// </summary>
    public void UnloadSceneContent() {
        foreach (object item in this._sceneContent.Values) {
            if (this.TryGetProcessor(item.GetType(), out IContentProcessor? processor)) {
                processor.Unload(item);
            }
        }
        
        this._sceneContent.Clear();
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            
            // Unload persistent content.
            foreach (object item in this._persistentContent.Values) {
                if (this.TryGetProcessor(item.GetType(), out IContentProcessor? processor)) {
                    processor.Unload(item);
                }
            }
            
            this._persistentContent.Clear();
            
            // Unload scene content.
            this.UnloadSceneContent();
        }
    }
}