using Raylib_cs;
using Sparkle.csharp.content.processor;
using Sparkle.csharp.content.type;

namespace Sparkle.csharp.content; 

public class ContentManager : Disposable {

    private readonly string _contentDirectory;

    private readonly List<object> _content;
    private readonly Dictionary<Type, IContentProcessor> _processors;
    private readonly Dictionary<Type, Type> _types;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentManager"/>, setting the content directory and initializing internal collections and processors.
    /// </summary>
    /// <param name="directory">The directory where content will be located.</param>
    public ContentManager(string directory) {
        this._contentDirectory = $"{directory}/";
        this._content = new List<object>();
        
        this._processors = new Dictionary<Type, IContentProcessor>();
        this.AddProcessors(typeof(Font), new FontProcessor());
        this.AddProcessors(typeof(Image), new ImageProcessor());
        this.AddProcessors(typeof(Texture2D), new TextureProcessor());
        this.AddProcessors(typeof(ModelAnimation[]), new ModelAnimationProcessor());
        this.AddProcessors(typeof(Model), new ModelProcessor());
        this.AddProcessors(typeof(Shader), new ShaderProcessor());
        this.AddProcessors(typeof(Sound), new SoundProcessor());
        this.AddProcessors(typeof(Wave), new WaveProcessor());
        this.AddProcessors(typeof(Music), new MusicProcessor());

        this._types = new Dictionary<Type, Type>();
        this.AddType(typeof(Font), typeof(FontContent));
        this.AddType(typeof(Image), typeof(ImageContent));
        this.AddType(typeof(Texture2D), typeof(TextureContent));
        this.AddType(typeof(ModelAnimation[]), typeof(ModelAnimationContent));
        this.AddType(typeof(Model), typeof(ModelContent));
        this.AddType(typeof(Shader), typeof(ShaderContent));
        this.AddType(typeof(Sound), typeof(SoundContent));
        this.AddType(typeof(Wave), typeof(WaveContent));
        this.AddType(typeof(Music), typeof(MusicContent));
    }
    
    /// <summary>
    /// Tries to retrieve a content processor for the specified content type.
    /// </summary>
    /// <param name="type">The type of content for which the processor is sought.</param>
    /// <returns>
    /// The content processor associated with the specified content type,
    /// or null if a matching processor is not found.
    /// </returns>
    public IContentProcessor TryGetProcessor(Type type) {
        if (!this._processors.TryGetValue(type, out IContentProcessor? processor)) {
            Logger.Error($"Unable to locate ContentProcessor for type [{type}]!");
        }

        return processor!;
    }
    
    /// <summary>
    /// Adds a content processor to the collection for a specified content type.
    /// </summary>
    /// <param name="type">The type of content for which the processor will be used.</param>
    /// <param name="processor">The content processor to add.</param>
    public void AddProcessors(Type type, IContentProcessor processor) {
        this._processors.Add(type, processor);
    }
    
    /// <summary>
    /// Checks if the specified content type is valid for a given generic type.
    /// </summary>
    /// <typeparam name="T">The generic type.</typeparam>
    /// <param name="contentType">The content type to check.</param>
    /// <returns>True if the content type is valid for the generic type; otherwise, false.</returns>
    private bool IsValidType<T>(IContentType contentType) {
        if (this._types[typeof(T)] == contentType.GetType()) {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Adds a mapping between a type and its corresponding content type.
    /// </summary>
    /// <param name="type">The type to map.</param>
    /// <param name="contentType">The corresponding content type.</param>
    public void AddType(Type type, Type contentType) {
        this._types.Add(type, contentType);
    }
    
    /// <summary>
    /// Loads content of the specified type and adds it to the content manager, returning the loaded item.
    /// </summary>
    /// <typeparam name="T">The type of content to load.</typeparam>
    /// <param name="contentType">The content type interface for loading.</param>
    /// <returns>The loaded content item of type T.</returns>
    public T Load<T>(IContentType contentType) {
        if (!this.IsValidType<T>(contentType)) {
            Logger.Fatal($"Invalid contentType for type {typeof(T).Name}");
        }
        
        T item = (T) this.TryGetProcessor(typeof(T)).Load(contentType, this._contentDirectory);

        this._content.Add(item!);
        return item;
    }
    
    /// <summary>
    /// Unloads the specified content item.
    /// </summary>
    /// <typeparam name="T">The type of content item to unload.</typeparam>
    /// <param name="item">The content item to unload.</param>
    public void Unload<T>(T item) {
        if (this._content.Contains(item!)) {
            this.TryGetProcessor(typeof(T)).Unload(item!);
            this._content.Remove(item!);
        }
        else {
            Logger.Warn($"Unable to unload content for the specified type {typeof(T)}!");
        }
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            foreach (object item in this._content) {
                this.TryGetProcessor(item.GetType()).Unload(item);
            }
            this._content.Clear();
        }
    }
}