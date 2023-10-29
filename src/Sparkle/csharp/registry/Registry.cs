using Sparkle.csharp.content;

namespace Sparkle.csharp.registry; 

public abstract class Registry : Disposable {
    
    protected ContentManager Content => Game.Instance.Content;
    public bool HasInitialized { get; private set; }
    
    protected readonly Dictionary<string, object> Items;
    
    /// <summary>
    /// Initializes a new instance of the Registry class, creating an empty registry for storing items.
    /// </summary>
    public Registry() {
        this.Items = new Dictionary<string, object>();
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
    protected T? Register<T>(string name, Func<T> item) {
        if (this.Items.TryAdd(name, item)) {
            Logger.Info($"Registration complete for: {name}");
            return item.Invoke();
        }
        else {
            Logger.Error($"Unable to register: {name}");
            return default;
        }
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            this.Items.Clear();
            RegistryManager.RegisterTypes.Remove(this);
        }
    }
}