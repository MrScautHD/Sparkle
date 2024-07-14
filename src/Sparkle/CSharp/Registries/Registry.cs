using Sparkle.CSharp.Content;

namespace Sparkle.CSharp.Registries;

public abstract class Registry : Disposable {
    
    public bool HasLoaded { get; private set; }
    public bool HasInitialized { get; private set; }

    /// <summary>
    /// Used for loading resources.
    /// </summary>
    protected internal virtual void Load(ContentManager content) {
        this.HasLoaded = true;
    }
    
    /// <summary>
    /// Used for Initializes objects.
    /// </summary>
    protected internal virtual void Init() {
        this.HasInitialized = true;
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            RegistryManager.RegisterTypes.Remove(this);
        }
    }
}