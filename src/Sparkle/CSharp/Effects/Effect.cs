using Raylib_CSharp.Materials;
using Raylib_CSharp.Shaders;

namespace Sparkle.CSharp.Effects;

public class Effect : Disposable {
    
    public Shader Shader { get; private set; }
    public bool HasInitialized { get; private set; }
    
    /// <summary>
    /// Constructor for creating an Effect object.
    /// </summary>
    /// <param name="shader">The shader to be used by the effect.</param>
    public Effect(Shader shader) {
        this.Shader = shader;
    }
    
    /// <summary>
    /// Used for Initializes objects.
    /// </summary>
    protected internal virtual void Init() {
        this.HasInitialized = true;
    }

    /// <summary>
    /// Apply the state effect immediately before rendering it.
    /// </summary>
    public virtual void Apply(Material? material = default) { }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            EffectManager.Effects.Remove(this);
        }
    }
}