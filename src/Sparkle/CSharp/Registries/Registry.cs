﻿using Sparkle.CSharp.Content;

namespace Sparkle.CSharp.Registries; 

public abstract class Registry : Disposable {
    
    public bool HasInitialized { get; private set; }
    
    private readonly Dictionary<string, object> _items;
    
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
    protected internal virtual void Load(ContentManager content) { }
    
    /// <summary>
    /// Registers an object with a specified name in the registry.
    /// </summary>
    /// <typeparam name="T">The type of the object to register.</typeparam>
    /// <param name="name">The name associated with the object.</param>
    /// <param name="item">The object to register.</param>
    /// <returns>The registered object of type T.</returns>
    protected T? Register<T>(string name, Func<T> item) {
        if (this._items.TryAdd(name, item)) {
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
            this._items.Clear();
            RegistryManager.RegisterTypes.Remove(this);
        }
    }
}