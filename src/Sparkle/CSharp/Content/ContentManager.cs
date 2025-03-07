using Bliss.CSharp;
using Bliss.CSharp.Effects;
using Bliss.CSharp.Fonts;
using Bliss.CSharp.Geometry;
using Bliss.CSharp.Images;
using Bliss.CSharp.Textures;
using MiniAudioEx;
using Sparkle.CSharp.Content.Processors;
using Sparkle.CSharp.Content.Types;
using Sparkle.CSharp.Logging;
using Sparkle.CSharp.Gifs;

namespace Sparkle.CSharp.Content;

public class ContentManager : Disposable {
    
    private readonly List<object> _content;
    private readonly Dictionary<Type, IContentProcessor> _processors;
    
    /// <summary>
    /// Manages the loading, unloading, and processing of content in a game.
    /// </summary>
    public ContentManager() {
        this._content = new List<object>();
        this._processors = new Dictionary<Type, IContentProcessor>();
        this.AddProcessors(typeof(Font), new FontProcessor());
        this.AddProcessors(typeof(Image), new ImageProcessor());
        this.AddProcessors(typeof(Texture2D), new TextureProcessor());
        this.AddProcessors(typeof(Effect), new ShaderProcessor());
        this.AddProcessors(typeof(Gif), new AnimatedImageProcessor());
        this.AddProcessors(typeof(Model), new ModelProcessor());
        this.AddProcessors(typeof(AudioClip), new SoundProcessor());
    }
    
    /// <summary>
    /// Tries to retrieve a content processor for the specified content type.
    /// </summary>
    /// <param name="type">The type of content for which the processor is sought.</param>
    /// <returns>
    /// The content processor associated with the specified content type, or null if a matching processor is not found.
    /// </returns>
    public IContentProcessor TryGetProcessor(Type type) {
        if (!this._processors.TryGetValue(type, out IContentProcessor? processor)) {
            Logger.Error($"Unable to locate ContentProcessor for type: [{type}]!");
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
    /// Loads an item of type T from the specified directory using the provided content type.
    /// </summary>
    /// <typeparam name="T">The type of item to load.</typeparam>
    /// <param name="type">The content type to be used.</param>
    /// <returns>The loaded item of type T.</returns>
    public T Load<T>(IContentType<T> type) {
        IContentProcessor processor = this.TryGetProcessor(typeof(T));
        T item = (T) processor.Load(type);
        
        this._content.Add(item!);
        return item;
    }
    
    // TODO: ADD "AddUnmanged" back for Gen methods like Mesh.GenCube();
    
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
            Logger.Warn($"Unable to unload content for the specified type: {typeof(T)}!");
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