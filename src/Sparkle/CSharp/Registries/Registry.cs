using Sparkle.CSharp.Content;

namespace Sparkle.CSharp.Registries;

public abstract class Registry : Disposable {
    
    public bool HasInitialized { get; private set; }
    
    /// <summary>
    /// Used for loading resources.
    /// </summary>
    protected internal virtual void Load(ContentManager content) { }
    
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