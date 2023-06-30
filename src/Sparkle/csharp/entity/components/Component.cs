using Sparkle.csharp.content;
using Sparkle.csharp.graphics;
using Sparkle.csharp.window;

namespace Sparkle.csharp.entity.components; 

public abstract class Component : IDisposable {

    protected Window Window => Game.Instance.Window;
    
    protected Graphics Graphics => Game.Instance.Graphics;

    protected ContentManager Content => Game.Instance.Content;

    protected internal Entity Entity { get; internal set; }

    protected internal virtual void Init() {
        
    }

    protected internal virtual void Update() {
        
    }
    
    protected internal virtual void FixedUpdate() {
        
    }
    
    protected internal virtual void Draw() {
        
    }
    
    public virtual void Dispose() {
        
    }
}