using Raylib_cs;

namespace Sparkle.csharp.entity; 

public abstract class Entity : IDisposable {
    
    public int Id { get; internal set; }

    public string Tag;
    
    public Transform Transform;

    public Entity(Transform transform) {
        this.Transform = transform;
    }
    
    protected internal virtual void Init() {
        
    }

    protected internal virtual void Update() {
        
    }
    
    protected internal virtual void FixedUpdate() {
        
    }
    
    protected internal virtual void Draw() {
        
    }
    
    public void Dispose() {
        
    }
}