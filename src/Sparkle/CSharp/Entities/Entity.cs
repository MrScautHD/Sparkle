using System.Numerics;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.Scenes;

namespace Sparkle.CSharp.Entities;

public class Entity : Disposable {
    
    public int Id { get; internal set; }

    public string Tag;

    public Vector3 Position;
    public Vector3 Scale;
    public Quaternion Rotation;
    
    private readonly Dictionary<Type, Component> _components;

    public bool HasInitialized { get; private set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Entity"/>, setting its position in 3D space.
    /// Also initializes the entity's scale to 1,1,1 (Vector3.One) and rotation to identity (Quaternion.Identity).
    /// Additionally, initializes an empty dictionary to hold components associated with the entity.
    /// </summary>
    /// <param name="position">Initial position of the entity in 3D space, specified as a Vector3.</param>
    public Entity(Vector3 position) {
        this.Tag = string.Empty;
        this.Position = position;
        this.Scale = Vector3.One;
        this.Rotation = Quaternion.Identity;
        this._components = new Dictionary<Type, Component>();
    }

    /// <summary>
    /// Used for Initializes objects.
    /// </summary>
    protected internal virtual void Init() {
        foreach (Component component in this._components.Values) {
            if (!component.HasInitialized) {
                component.Init();
            }
        }

        this.HasInitialized = true;
    }

    /// <summary>
    /// Is invoked during each tick and is used for updating dynamic elements and game logic.
    /// </summary>
    protected internal virtual void Update() {
        foreach (Component component in this._components.Values) {
            if (component.HasInitialized) {
                component.Update();
            }
        }
    }

    /// <summary>
    /// Called after the Update method on each tick to further update dynamic elements and game logic.
    /// </summary>
    protected internal virtual void AfterUpdate() {
        foreach (Component component in this._components.Values) {
            if (component.HasInitialized) {
                component.AfterUpdate();
            }
        }
    }
    
    /// <summary>
    /// Is invoked at a fixed rate of every <see cref="GameSettings.FixedTimeStep"/> frames following the <see cref="AfterUpdate"/> method.
    /// It is used for handling physics and other fixed-time operations.
    /// </summary>
    protected internal virtual void FixedUpdate() {
        foreach (Component component in this._components.Values) {
            if (component.HasInitialized) {
                component.FixedUpdate();
            }
        }
    }
    
    /// <summary>
    /// Is called every tick, used for rendering stuff.
    /// </summary>
    protected internal virtual void Draw() {
        foreach (Component component in this._components.Values) {
            if (component.HasInitialized) {
                component.Draw();
            }
        }
    }
    
    /// <summary>
    /// Adds a component to the entity and initializes it if the entity has been initialized.
    /// </summary>
    /// <param name="component">The component to be added.</param>
    public void AddComponent(Component component) {
        component.Entity = this;

        if (this.HasInitialized) {
            component.Init();
        }
        
        this._components.Add(component.GetType(), component);
    }
    
    /// <summary>
    /// Removes a component from the entity and disposes of it.
    /// </summary>
    /// <param name="component">The component to be removed.</param>
    public void RemoveComponent(Component component) {
        this._components.Remove(component.GetType());
        component.Dispose();
    }

    /// <summary>
    /// Retrieves a component of the specified type from the entity.
    /// </summary>
    /// <typeparam name="T">The type of component to retrieve.</typeparam>
    /// <returns>The component of the specified type.</returns>
    public T GetComponent<T>() where T : Component {
        if (!this._components.TryGetValue(typeof(T), out Component? component)) {
            Logger.Error($"Unable to locate Component for type [{typeof(T)}]!");
        }

        return (T) component!;
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            foreach (Component component in this._components.Values) {
                component.Dispose();
            }
            
            this._components.Clear();
        }
    }
}