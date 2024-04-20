using Raylib_cs;
using Sparkle.CSharp.Content.Types;

namespace Sparkle.CSharp.Effects;

public class Effect : Disposable {
    
    public Shader Shader { get; private set; }
    public bool HasInitialized { get; private set; }
    
    /// <summary>
    /// Constructor for creating an Effect object.
    /// </summary>
    /// <param name="vertPath">Path to the vertex shader file.</param>
    /// <param name="fragPath">Path to the fragment shader file.</param>
    public Effect(string vertPath, string fragPath) {
        this.Shader = Game.Instance.Content.Load(new ShaderContent(vertPath, fragPath));
    }
    
    /// <summary>
    /// Used for Initializes objects.
    /// </summary>
    protected internal virtual void Init() {
        this.HasInitialized = true;
    }
    
    /// <summary>
    /// Is invoked during each tick and is used for updating dynamic elements and game logic.
    /// </summary>
    protected internal virtual void Update() { }
    
    /// <summary>
    /// Called after the Update method on each tick to further update dynamic elements and game logic.
    /// </summary>
    protected internal virtual void AfterUpdate() { }
    
    /// <summary>
    /// Is invoked at a fixed rate of every <see cref="GameSettings.FixedTimeStep"/> frames following the <see cref="AfterUpdate"/> method.
    /// It is used for handling physics and other fixed-time operations.
    /// </summary>
    protected internal virtual void FixedUpdate() { }
    
    /// <summary>
    /// Is called every tick, used for rendering stuff.
    /// </summary>
    protected internal virtual void Draw() { }

    /// <summary>
    /// Updates the shader parameters for the materials, called from the ModelRenderer.
    /// </summary>
    /// <param name="materials">Array of materials to be updated.</param>
    protected internal virtual void UpdateMaterialParameters(Material[] materials) { }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            EffectManager.Effects.Remove(this);
        }
    }
}