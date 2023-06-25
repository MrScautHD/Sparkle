using System.Numerics;
using Raylib_cs;
using Sparkle.csharp.content;
using Sparkle.csharp.entity.components;
using Sparkle.csharp.graphics;
using Sparkle.csharp.window;

namespace Sparkle.csharp.entity; 

public abstract class Entity : IDisposable {
    
    protected Window Window => Game.Instance.Window;
    
    protected Graphics Graphics => Game.Instance.Graphics;

    protected ContentManager Content => Game.Instance.Content;
    
    public int Id { get; internal set; }

    public string? Tag;

    public Vector3 Position;
    public Vector3 Scale;
    public Quaternion Rotation;

    private readonly Dictionary<Type, Component> _components;

    public Entity(Vector3 position) {
        this.Position = position;
        this.Scale = Vector3.One;
        this.Rotation = Quaternion.Identity;
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
        this.Rotation *= Quaternion.CreateFromAxisAngle(axis, angle * Raylib.DEG2RAD);
    }

    public void Dispose() {
        foreach (Component component in this._components.Values) {
            component.Dispose();
        }
    }
}