using System.Diagnostics.CodeAnalysis;
using Bliss.CSharp;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.Scenes;
using Veldrid;

namespace Sparkle.CSharp.Entities;

public class Entity : Disposable {
    
    /// <summary>
    /// The scene that the entity belongs to.
    /// </summary>
    public Scene Scene { get; internal set; }
    
    /// <summary>
    /// The unique identifier for this entity.
    /// </summary>
    public uint Id { get; internal set; }
    
    /// <summary>
    /// A tag used to categorize or identify the entity.
    /// </summary>
    public string Tag;
    
    /// <summary>
    /// The transform representing the position, rotation, and scale of the entity.
    /// </summary>
    public Transform Transform;

    /// <summary>
    /// A dictionary storing all components attached to this entity.
    /// </summary>
    internal Dictionary<Type, Component> Components;

    /// <summary>
    /// Initializes a new instance of the <see cref="Entity"/> class.
    /// </summary>
    /// <param name="transform">The transform defining the position, rotation, and scale of the entity.</param>
    /// <param name="tag">An optional tag used to categorize or identify the entity.</param>
    public Entity(Transform transform, string? tag = null) {
        this.Transform = transform;
        this.Tag = tag ?? string.Empty;
        this.Components = new Dictionary<Type, Component>();
    }

    /// <summary>
    /// Called once when the entity is initialized.
    /// </summary>
    protected internal virtual void Init() {
        foreach (Component component in this.Components.Values) {
            component.Init();
        }
    }

    /// <summary>
    /// Called every tick to update the entity's logic.  
    /// </summary>
    /// <param name="delta">The time delta since the last update.</param>
    protected internal virtual void Update(double delta) {
        foreach (Component component in this.Components.Values) {
            component.Update(delta);
        }
    }
    
    /// <summary>
    /// Called after the main update phase.  
    /// </summary>
    /// <param name="delta">The time delta since the last update.</param>
    protected internal virtual void AfterUpdate(double delta) {
        foreach (Component component in this.Components.Values) {
            component.AfterUpdate(delta);
        }
    }

    /// <summary>
    /// Updates the entity and its components at fixed time intervals.
    /// </summary>
    /// <param name="fixedStep">The fixed time step interval at which the update occurs.</param>
    protected internal virtual void FixedUpdate(double fixedStep) {
        foreach (Component component in this.Components.Values) {
            component.FixedUpdate(fixedStep);
        }
    }

    /// <summary>
    /// Renders the entity and its components using the provided graphics context and framebuffer.
    /// </summary>
    /// <param name="context">The graphics context that provides rendering utilities.</param>
    /// <param name="framebuffer">The framebuffer to which the entity will be rendered.</param>
    protected internal virtual void Draw(GraphicsContext context, Framebuffer framebuffer) {
        foreach (Component component in this.Components.Values) {
            component.Draw(context, framebuffer);
        }
    }

    /// <summary>
    /// Called when the screen or viewport size changes.  
    /// </summary>
    /// <param name="rectangle">The new screen or viewport dimensions.</param>
    protected internal virtual void Resize(Rectangle rectangle) {
        foreach (Component component in this.Components.Values) {
            component.Resize(rectangle);
        }
    }

    /// <summary>
    /// Retrieves all components attached to the entity.
    /// </summary>
    /// <returns>An enumerable collection of components associated with the entity.</returns>
    public IEnumerable<Component> GetComponents() {
        return this.Components.Values;
    }

    /// <summary>
    /// Checks if the entity has a specific type of component.
    /// </summary>
    /// <typeparam name="T">The type of component to check for.</typeparam>
    /// <returns>True if the entity has the component, otherwise false.</returns>
    public bool HasComponent<T>() where T : Component {
        return this.Components.ContainsKey(typeof(T));
    }
    
    /// <summary>
    /// Retrieves a specific component attached to this entity.
    /// </summary>
    /// <typeparam name="T">The type of component to retrieve.</typeparam>
    /// <returns>The component if found, otherwise null.</returns>
    public T? GetComponent<T>() where T : Component {
        if (!this.TryGetComponent(out T? result)) {
            return null;
        }
        
        return result;
    }
    
    /// <summary>
    /// Attempts to retrieve a specific component attached to this entity.
    /// </summary>
    /// <typeparam name="T">The type of component to retrieve.</typeparam>
    /// <param name="component">The retrieved component if found.</param>
    /// <returns>True if the component was found, otherwise false.</returns>
    public bool TryGetComponent<T>([NotNullWhen(true)] out T? component) where T : Component {
        if (!this.Components.TryGetValue(typeof(T), out Component? result)) {
            component = null;
            return false;
        }

        component = (T) result;
        return true;
    }

    /// <summary>
    /// Adds a component to this entity.
    /// </summary>
    /// <param name="component">The component to add.</param>
    /// <exception cref="Exception">Thrown if the component is already attached to the entity.</exception>
    public void AddComponent(Component component) {
        if (!this.TryAddComponent(component)) {
            throw new Exception($"The component type: [{nameof(component)}] is already present in the entity: [{this.Id}] or the component has already set to an entity.");
        }
    }
    
    /// <summary>
    /// Attempts to add a component to this entity.
    /// </summary>
    /// <param name="component">The component to add.</param>
    /// <returns>True if the component was added, otherwise false.</returns>
    public bool TryAddComponent(Component component) {
        if (this.Components.ContainsKey(component.GetType())) {
            return false;
        }

        foreach (Component comp in this.Components.Values) {
            if (component.ConflictsWith(comp)) {
                return false;
            }
        }

        if (component.Entity != null!) {
            return false;
        }

        component.Entity = this;
        
        if (this.Id != 0) {
            component.Init();
        }

        this.Components.Add(component.GetType(), component);
        return true;
    }

    /// <summary>
    /// Removes a component from this entity.
    /// </summary>
    /// <param name="component">The component to remove.</param>
    /// <exception cref="Exception">Thrown if the component does not exist on this entity.</exception>
    public void RemoveComponent(Component component) {
        if (!this.TryRemoveComponent(component)) {
            throw new Exception($"Failed to Remove/Dispose the component type: [{nameof(component)}] from the entity: [{this.Id}].");
        }
    }

    /// <summary>
    /// Attempts to remove a component from this entity.
    /// </summary>
    /// <param name="component">The component to remove.</param>
    /// <returns>True if the component was removed, otherwise false.</returns>
    public bool TryRemoveComponent(Component component) {
        if (!this.Components.ContainsKey(component.GetType())) {
            return false;
        }
        
        component.Dispose();
        
        // Ensure the component is removed, even if `Dispose` was overridden incorrectly.
        if (this.Components.ContainsKey(component.GetType())) {
            this.Components.Remove(component.GetType());
        }

        return true;
    }
    
    /// <summary>
    /// Removes a component of a specific type from this entity.
    /// </summary>
    /// <typeparam name="T">The type of component to remove.</typeparam>
    /// <exception cref="Exception">Thrown if the component type does not exist on this entity.</exception>
    public void RemoveComponent<T>() where T : Component {
        if (!this.TryRemoveComponent<T>()) {
            throw new Exception($"Failed to Remove/Dispose the component type: [{nameof(T)}] from the entity: [{this.Id}].");
        }
    }

    /// <summary>
    /// Attempts to remove a component of a specific type from this entity.
    /// </summary>
    /// <typeparam name="T">The type of component to remove.</typeparam>
    /// <returns>True if the component was removed, otherwise false.</returns>
    public bool TryRemoveComponent<T>() where T : Component {
        if (!this.Components.TryGetValue(typeof(T), out Component? component)) {
            return false;
        }
        
        component.Dispose();
        
        // Ensure the component is removed, even if `Dispose` was overridden incorrectly.
        if (this.Components.ContainsKey(component.GetType())) {
            this.Components.Remove(component.GetType());
        }

        return true;
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            var enumerator = this.Components.GetEnumerator();
            
            while (enumerator.MoveNext()) {
                Component component = enumerator.Current.Value;
                component.Dispose();
                
                // Ensure the component is removed, even if `Dispose` was overridden incorrectly.
                if (this.Components.ContainsKey(component.GetType())) {
                    this.Components.Remove(component.GetType());
                }
            }

            this.Scene.Entities.Remove(this.Id);
        }
    }
}