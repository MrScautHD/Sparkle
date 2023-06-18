using System.Numerics;
using Raylib_cs;
using Sparkle.csharp.entity.components;

namespace Sparkle.csharp.entity; 

public abstract class Entity : IDisposable {
    
    public int Id { get; internal set; }

    public string? Tag;
    
    public Transform Transform;

    private readonly Dictionary<Type, Component> _components;

    public Entity(Transform transform) {
        this.Transform = transform;
        this._components = new Dictionary<Type, Component>();
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
        foreach (Component component in this._components.Values) {
            component.Init();
        }
    }

    protected internal virtual void Update() {
        foreach (Component component in this._components.Values) {
            component.Update();
        }
    }
    
    protected internal virtual void FixedUpdate() {
        foreach (Component component in this._components.Values) {
            component.FixedUpdate();
        }
    }
    
    protected internal virtual void Draw() {
        foreach (Component component in this._components.Values) {
            component.Draw();
        }
    }

    public void AddComponent(Component component) {
        this._components.Add(component.GetType(), component);
    }
    
    public void RemoveComponent(Component component) {
        this._components.Remove(component.GetType());
        component.Dispose();
    }

    public T GetComponent<T>() where T : Component {
        if (!this._components.TryGetValue(typeof(T), out Component? component)) {
            Logger.Error($"Unable to locate Component for type {typeof(T)}!");
        }

        return (T) component!;
    }
    
    public void Dispose() {
        foreach (Component component in this._components.Values) {
            component.Dispose();
        }
    }
}