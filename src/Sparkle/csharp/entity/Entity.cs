using System.Numerics;
using Raylib_cs;
using Sparkle.csharp.entity.components;

namespace Sparkle.csharp.entity; 

public abstract class Entity : IDisposable {
    
    public int Id { get; internal set; }

    public string? Tag;

    public Vector3 Position;
    public Vector3 Scale;
    public Quaternion Rotation;

    private readonly Dictionary<Type, Component> _components;

    public Entity(Vector3 position) {
        this.Position = position;
        this.Scale = Vector3.One;
        this._components = new Dictionary<Type, Component>();
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
            Logger.Error($"Unable to locate Component for type [{typeof(T)}]!");
        }

        return (T) component!;
    }

    public void RotateAxisAngle(Vector3 axis, float angle) {
        this.Rotation = Raymath.QuaternionFromAxisAngle(axis, angle);
    }
    
    public void RotateEulerAngles(float pitch, float yaw, float roll) {
        this.Rotation = Raymath.QuaternionFromEuler(pitch, yaw, roll);
    }

    public void Dispose() {
        foreach (Component component in this._components.Values) {
            component.Dispose();
        }
    }
}