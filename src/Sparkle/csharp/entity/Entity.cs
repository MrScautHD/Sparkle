using System.Numerics;
using Sparkle.csharp.entity.component;

namespace Sparkle.csharp.entity; 

public abstract class Entity : IDisposable {
    
    public int Id { get; internal set; }

    public string? Tag;

    // TODO https://github.com/ChrisDill/Raylib-cs/issues/163
    public Vector3 Position;
    public Vector3 Scale;
    public Quaternion Rotation;
    
    private readonly Dictionary<Type, Component> _components;

    public bool HasInitialized { get; private set; }
    public bool HasDisposed { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Entity"/>, setting its position in 3D space.
    /// Also initializes the entity's scale to 1,1,1 (Vector3.One) and rotation to identity (Quaternion.Identity).
    /// Additionally, initializes an empty dictionary to hold components associated with the entity.
    /// </summary>
    /// <param name="position">Initial position of the entity in 3D space, specified as a Vector3.</param>
    public Entity(Vector3 position) {
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
            component.Init();
        }

        this.HasInitialized = true;
    }

    /// <summary>
    /// Is invoked during each tick and is used for updating dynamic elements and game logic.
    /// </summary>
    protected internal virtual void Update() {
        foreach (Component component in this._components.Values) {
            component.Update();
        }
    }
    
    /// <summary>
    /// Is invoked at a fixed rate of every <see cref="GameSettings.FixedTimeStep"/> frames following the <see cref="Update"/> method.
    /// It is used for handling physics and other fixed-time operations.
    /// </summary>
    protected internal virtual void FixedUpdate() {
        foreach (Component component in this._components.Values) {
            component.FixedUpdate();
        }
    }
    
    /// <summary>
    /// Is called every tick, used for rendering stuff.
    /// </summary>
    protected internal virtual void Draw() {
        foreach (Component component in this._components.Values) {
            component.Draw();
        }
    }
    
    /// <summary>
    /// Adds a component to the entity and initializes it if the entity has been initialized.
    /// </summary>
    /// <param name="component">The component to be added.</param>
    public void AddComponent(Component component) {
        this.ThrowIfDisposed();
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
        this.ThrowIfDisposed();
        this._components.Remove(component.GetType());
        component.Dispose();
    }

    /// <summary>
    /// Retrieves a component of the specified type from the entity.
    /// </summary>
    /// <typeparam name="T">The type of component to retrieve.</typeparam>
    /// <returns>The component of the specified type.</returns>
    public T GetComponent<T>() where T : Component {
        this.ThrowIfDisposed();
        if (!this._components.TryGetValue(typeof(T), out Component? component)) {
            Logger.Error($"Unable to locate Component for type [{typeof(T)}]!");
        }

        return (T) component!;
    }
    
    public virtual void Dispose() {
        if (this.HasDisposed) return;
        
        this.Dispose(true);
        GC.SuppressFinalize(this);
        this.HasDisposed = true;
    }
    
    protected virtual void Dispose(bool disposing) {
        if (disposing) {
            foreach (Component component in this._components.Values) {
                component.Dispose();
            }
        }
    }
    
    public void ThrowIfDisposed() {
        if (this.HasDisposed) {
            throw new ObjectDisposedException(this.GetType().Name);
        }
    }
}