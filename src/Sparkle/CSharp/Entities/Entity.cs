using Bliss.CSharp;
using Bliss.CSharp.Logging;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.Graphics;

namespace Sparkle.CSharp.Entities;

public class Entity : Disposable {
    
    public uint Id { get; internal set; }
    public bool HasInitialized { get; private set; }
    
    public Transform Transform;
    public string Tag;

    private Dictionary<Type, Component> _components;

    public Entity(Transform transform, string? tag = null) {
        this.Transform = transform;
        this.Tag = tag ?? string.Empty;
        this._components = new Dictionary<Type, Component>();
    }

    protected internal virtual void Init() {
        
    }

    protected internal virtual void Update() {
        foreach (Component component in this._components.Values) {
            component.Update();
        }
    }
    
    protected internal virtual void AfterUpdate() {
        foreach (Component component in this._components.Values) {
            component.AfterUpdate();
        }
    }
    
    protected internal virtual void FixedUpdate() {
        foreach (Component component in this._components.Values) {
            component.FixedUpdate();
        }
    }
    
    protected internal virtual void Draw(GraphicsContext context) {
        foreach (Component component in this._components.Values) {
            component.Draw(context);
        }
    }

    public Component[] GetComponents() {
        return this._components.Values.ToArray();
    }

    public bool TryGetComponent<T>(out T? component) where T : Component {
        if (!this._components.TryGetValue(typeof(T), out Component? result)) {
            Logger.Error($"Unable to locate Component for type [{typeof(T)}]!");
            component = null;
            return false;
        }

        component = (T) result;
        return true;
    }

    public bool TryAddComponent(Component component) {
        if (this._components.Any(comp => comp.GetType() == component.GetType())) {
            Logger.Warn($"The component type [{component.GetType().Name}] is already present in the Entity[{this.Id}]!");
            return false;
        }

        if (component.Entity != null!) {
            Logger.Warn($"This component has already been added to a different Entity[{component.Entity.Id}]. Please create a new component to add it to this Entity[{this.Id}].");
            return false;
        }

        component.Entity = this;
        this._components.Add(component.GetType(), component);
        return true;
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            
        }
    }
}