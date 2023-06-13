using Raylib_cs;
using Sparkle.csharp.content.processor;

namespace Sparkle.csharp.content; 

public class ContentManager : IDisposable {

    private readonly string _contentDirectory;

    private readonly List<object> _content;
    private readonly Dictionary<Type, IContentProcessor> _processors;

    public ContentManager(string directory) {
        this._contentDirectory = $"{directory}/";
        this._content = new List<object>();
        
        this._processors = new Dictionary<Type, IContentProcessor>();
        this.AddProcessors(typeof(Font), new FontProcessor());
        this.AddProcessors(typeof(Image), new ImageProcessor());
        this.AddProcessors(typeof(Model), new ModelProcessor());
        this.AddProcessors(typeof(Sound), new SoundProcessor());
        this.AddProcessors(typeof(Texture2D), new TextureProcessor());
    }

    public void AddProcessors(Type type, IContentProcessor processor) {
        this._processors.Add(type, processor);
    }

    public IContentProcessor TryGetProcessor<T>() {
        if (!this._processors.TryGetValue(typeof(T), out IContentProcessor? processor)) {
            Logger.Error($"Unable to locate ContentProcessor for type {typeof(T)}!");
        }

        return processor!;
    }

    public T Load<T>(string path) {
        T content = (T) this.TryGetProcessor<T>().Load(this._contentDirectory + path);
        Logger.Info($"Loading [{path}]");
        
        this._content.Add(content!);
        return content;
    }
    
    public void UnLoad<T>(T content) { // FIX UNLOADER + ADD LOGGER
        if (this._content.Contains(content!)) {
            this.TryGetProcessor<T>().UnLoad(content!);
            this._content.Remove(content!);
        }
    }
    
    public void Dispose() {
        foreach (var content in this._content) {
            this.UnLoad(content);
        }
    }
}