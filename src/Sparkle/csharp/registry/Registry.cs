using Sparkle.csharp.content;

namespace Sparkle.csharp.registry; 

public abstract class Registry : Disposable {
    
    protected ContentManager Content => Game.Instance.Content;
    public bool HasInitialized { get; private set; }
    
    private Dictionary<string, object> _items;
    
    /// <summary>
    /// Initializes a new instance of the Registry class, creating an empty registry for storing items.
    /// </summary>
    public Registry() {
        this._items = new Dictionary<string, object>();
    }

    /// <summary>
    /// Used for Initializes objects.
    /// </summary>
    protected internal virtual void Init() {
        this.HasInitialized = true;
    }
    
    /// <summary>
    /// Used for loading resources.
    /// </summary>
    protected internal virtual void Load() { }
    
    /// <summary>
    /// Registers an object with a specified name in the registry.
    /// </summary>
    /// <typeparam name="T">The type of the object to register.</typeparam>
    /// <param name="name">The name associated with the object.</param>
    /// <param name="item">The object to register.</param>
    /// <returns>The registered object of type T.</returns>
    protected T Register<T>(string name, T item) {
        Logger.Info($"Registration complete for: {name}");
        this._items.Add(name, item!);

        return item;
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            this._items.Clear();
            RegistryManager.GetTypes().Remove(this);
        }
    }
}