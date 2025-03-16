using Bliss.CSharp;
using Bliss.CSharp.Effects;
using Bliss.CSharp.Fonts;
using Bliss.CSharp.Geometry;
using Bliss.CSharp.Logging;
using Bliss.CSharp.Textures;
using MiniAudioEx;
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
    private List<object> _content;
    
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
        this._content = new List<object>();
        this._processors = new Dictionary<Type, IContentProcessor>();
        
        // Add default processors.
        this.AddProcessors(typeof(Font), new FontProcessor());
        this.AddProcessors(typeof(Texture2D), new TextureProcessor());
        this.AddProcessors(typeof(Effect), new EffectProcessor());
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
    public bool TryGetProcessor(Type type, out IContentProcessor? processor) {
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
    /// Adds an unmanaged item to the content manager.
    /// </summary>
    /// <typeparam name="T">The type of content.</typeparam>
    /// <param name="item">The item to add.</param>
    public void AddUnmanagedItem<T>(T item) {
        if (!this._content.Contains(item!)) {
            if (!this._processors.ContainsKey(typeof(T))) {
                Logger.Warn($"This item is of an unsupported type: [{typeof(T)}]");
            }
            else {
                this._content.Add(item!);
            }
        }
        else {
            Logger.Warn($"The item is already present in the content manager for the specified type: [{typeof(T)}]");
        }
    }

    /// <summary>
    /// Loads content using an appropriate processor.
    /// </summary>
    /// <typeparam name="T">The type of content to load.</typeparam>
    /// <param name="type">The content type to load.</param>
    /// <returns>The loaded content.</returns>
    public T Load<T>(IContentType<T> type) {
        if (!this.TryGetProcessor(typeof(T), out IContentProcessor? processor)) {
            Logger.Fatal($"Failed to find a valid ContentProcessor for type: [{typeof(T)}]");
        }

        T item = (T) processor!.Load(this.GraphicsDevice, type);
        
        this._content.Add(item!);
        return item;
    }

    /// <summary>
    /// Unloads a content item, freeing associated resources.
    /// </summary>
    /// <typeparam name="T">The type of content to unload.</typeparam>
    /// <param name="item">The item to unload.</param>
    public void Unload<T>(T item) {
        if (this._content.Contains(item!)) {
            if (!this.TryGetProcessor(typeof(T), out IContentProcessor? processor)) {
                Logger.Error($"Failed to find a valid ContentProcessor for type: [{typeof(T)}]");
            }
            else {
                processor!.Unload(item!);
                this._content.Remove(item!);
            }
        }
        else {
           Logger.Warn($"Attempted to unload an item of type: [{typeof(T)}] that was not loaded in the ContentManager.");
        }
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            foreach (object item in this._content) {
                this.GetProcessor(item.GetType())!.Unload(item);
            }
            
            this._content.Clear();
        }
    }
}