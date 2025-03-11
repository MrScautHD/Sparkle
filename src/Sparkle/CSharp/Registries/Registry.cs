using Bliss.CSharp;
using Sparkle.CSharp.Content;

namespace Sparkle.CSharp.Registries;

public abstract class Registry : Disposable {
    
    /// <summary>
    /// Loads content using the specified <see cref="ContentManager"/>.
    /// </summary>
    /// <param name="content">The content manager used to load assets.</param>
    protected internal virtual void Load(ContentManager content) { }
    
    /// <summary>
    /// Initializes the registry after content has been loaded.
    /// </summary>
    protected internal virtual void Init() { }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            RegistryManager.Registries.Remove(this.GetType());
        }
    }
}