using Sparkle.csharp.content;
using Sparkle.csharp.graphics;
using Sparkle.csharp.window;

namespace Sparkle.csharp.overlay; 

public abstract class Overlay : IDisposable {
    
    protected Window Window => Game.Instance.Window;
    
    protected Graphics Graphics => Game.Instance.Graphics;

    protected ContentManager Content => Game.Instance.Content;
    
    public readonly string Name;

    public bool Enabled;

    public static readonly List<Overlay> Overlays = new();

    public Overlay(string name) {
        this.Name = name;
        
        Overlays.Add(this);
    }
    
    protected internal virtual void Init() {

    }
    
    protected internal virtual void Update() {

    }

    protected internal virtual void FixedUpdate() {

    }

    protected internal virtual void Draw() {

    }

    public virtual void Dispose() {
        Overlays.Remove(this);
    }
}