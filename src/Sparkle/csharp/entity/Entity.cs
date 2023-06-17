using System.Numerics;
using Raylib_cs;

namespace Sparkle.csharp.entity; 

public abstract class Entity : IDisposable {
    
    public int Id { get; internal set; }

    public string? Tag;
    
    public Transform Transform;
    
    public Entity(Transform transform) {
        this.Transform = transform;
    }
    
    public Vector3 Position {
        get => this.Transform.translation;
        set => this.Transform.translation = value;
    }

    public Quaternion Rotation { // TODO ISSUE: https://github.com/ChrisDill/Raylib-cs/pull/158
        get => new Quaternion(this.Transform.rotation.X, this.Transform.rotation.Y, this.Transform.rotation.Z, this.Transform.rotation.W);
        set => this.Transform.rotation = new Vector4(value.X, value.Y, value.Z, value.W);
    }
    
    public Vector3 Scale {
        get => this.Transform.scale;
        set => this.Transform.scale = value;
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