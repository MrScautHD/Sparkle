using System.Diagnostics.CodeAnalysis;
using System.Numerics;
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
    /// The parent of this entity.
    /// </summary>
    public Entity? Parent { get; private set; }
    
    /// <summary>
    /// A tag used to categorize or identify the entity.
    /// </summary>
    public string Tag;
    
    /// <summary>
    /// The local transformation of the entity within its parent's coordinate space.
    /// </summary>
    public Transform LocalTransform;
    
    /// <summary>
    /// The global transformation of the entity, combining its local transformation with the transformations of all its ancestors in the hierarchy.
    /// </summary>
    public Transform GlobalTransform {
        get {
            if (this.Parent == null) {
                return this.LocalTransform;
            }
            
            Transform parentTransform = this.Parent.GlobalTransform;
            
            return new Transform() {
                Translation = parentTransform.Translation + Vector3.Transform(this.LocalTransform.Translation * parentTransform.Scale, parentTransform.Rotation),
                Rotation = parentTransform.Rotation * this.LocalTransform.Rotation,
                Scale = parentTransform.Scale * this.LocalTransform.Scale
            };
        }
    }
    
    /// <summary>
    /// A dictionary storing all components attached to this entity.
    /// </summary>
    internal Dictionary<Type, Component> Components;
    
    /// <summary>
    /// A list of children attached to this entity.
    /// </summary>
    private Dictionary<uint, Entity> _children;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Entity"/> class.
    /// </summary>
    /// <param name="transform">The transform defining the position, rotation, and scale of the entity.</param>
    /// <param name="tag">An optional tag used to categorize or identify the entity.</param>
    public Entity(Transform transform, string? tag = null) {
        this.LocalTransform = transform;
        this.Tag = tag ?? string.Empty;
        this.Components = new Dictionary<Type, Component>();
        this._children = new Dictionary<uint, Entity>();
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
    /// Retrieves all child entities of the current entity.
    /// </summary>
    /// <returns>A read-only collection containing the child entities.</returns>
    public IReadOnlyCollection<Entity> GetChildren() {
        return this._children.Values;
    }
    
    /// <summary>
    /// Determines whether this entity contains a child entity with the specified ID.
    /// </summary>
    /// <param name="id">The unique identifier of the child entity to check for.</param>
    /// <returns>true if a child entity with the specified ID exists; otherwise, false.</returns>
    public bool HasChild(uint id) {
        return this._children.ContainsKey(id);
    }
    
    /// <summary>
    /// Retrieves a child entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the child entity to retrieve.</param>
    /// <returns>The child entity with the specified identifier, or <c>null</c> if no such child exists.</returns>
    public Entity? GetChild(uint id) {
        if (!this.TryGetChild(id, out Entity? result)) {
            return null;
        }
        
        return result;
    }
    
    /// <summary>
    /// Attempts to retrieve a child entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the child entity to retrieve.</param>
    /// <param name="child">When this method returns, contains the child entity associated with the specified identifier, if the operation succeeded; otherwise, null.</param>
    /// <returns><c>true</c> if a child entity with the specified identifier was found; otherwise, <c>false</c>.</returns>
    public bool TryGetChild(uint id, [NotNullWhen(true)] out Entity? child) {
        return this._children.TryGetValue(id, out child);
    }
    
    /// <summary>
    /// Adds a child entity to the current entity.
    /// </summary>
    /// <param name="child">The entity to be added as a child. The child must not already have a parent entity.</param>
    /// <exception cref="Exception">Thrown if the specified child entity is already a child of another entity or this entity.</exception>
    public void AddChild(Entity child) {
        if (!this.TryAddChild(child)) {
            throw new Exception($"The entity: [{child.Id}] is already a child of another entity or this entity.");
        }
    }
    
    /// <summary>
    /// Attempts to add the specified entity as a child of this entity.
    /// </summary>
    /// <param name="child">The entity to be added as a child.</param>
    /// <returns>True if the child was successfully added; otherwise, false.</returns>
    public bool TryAddChild(Entity child) {
        if (child.Parent != null) {
            return false;
        }
        
        if (!this._children.TryAdd(child.Id, child)) {
            return false;
        }
        
        child.Parent = this;
        return true;
    }
    
    /// <summary>
    /// Removes the specified child entity from the current entity's list of children.
    /// </summary>
    /// <param name="child">The child entity to be removed.</param>
    /// <exception cref="Exception">Thrown when the specified entity is not a child of the current entity.</exception>
    public void RemoveChild(Entity child) {
        if (!this.TryRemoveChild(child)) {
            throw new Exception($"The entity: [{child.Id}] is not a child of this entity: [{this.Id}].");
        }
    }
    
    /// <summary>
    /// Attempts to remove the specified child entity from the current entity's collection of children.
    /// </summary>
    /// <param name="child">The child entity to remove from the current entity.</param>
    /// <returns>True if the child entity was successfully removed; otherwise, false.</returns>
    public bool TryRemoveChild(Entity child) {
        if (!this._children.ContainsKey(child.Id)) {
            return false;
        }
        
        child.Parent = null;
        return this._children.Remove(child.Id);
    }
    
    /// <summary>
    /// Removes a child entity with the specified ID from the current entity's hierarchy.
    /// </summary>
    /// <param name="id">The unique identifier of the child entity to be removed.</param>
    /// <exception cref="Exception">Thrown if the entity with the specified ID is not a child of the current entity.</exception>
    public void RemoveChild(uint id) {
        if (!this.TryRemoveChild(id)) {
            throw new Exception($"The entity: [{id}] is not a child of this entity: [{this.Id}].");
        }
    }
    
    /// <summary>
    /// Attempts to remove a child entity with the specified identifier from this entity's list of children.
    /// </summary>
    /// <param name="id">The unique identifier of the child entity to remove.</param>
    /// <return><c>true</c> if the child entity was successfully removed; otherwise, <c>false</c>.</return>
    public bool TryRemoveChild(uint id) {
        if (!this._children.TryGetValue(id, out Entity? child)) {
            return false;
        }
        
        child.Parent = null;
        return this._children.Remove(id);
    }
    
    /// <summary>
    /// Retrieves all components attached to the entity.
    /// </summary>
    /// <returns>An enumerable collection of components associated with the entity.</returns>
    public IReadOnlyCollection<Component> GetComponents() {
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
            
            // Detach children.
            foreach (Entity child in this._children.Values) {
                child.Parent = null;
            }
            
            this._children.Clear();
            
            // Dispose components.
            var enumerator = this.Components.GetEnumerator();
            
            while (enumerator.MoveNext()) {
                Component component = enumerator.Current.Value;
                component.Dispose();
                
                // Ensure the component is removed, even if `Dispose` was overridden incorrectly.
                if (this.Components.ContainsKey(component.GetType())) {
                    this.Components.Remove(component.GetType());
                }
            }
            
            // Remove entity from scene.
            this.Scene.Entities.Remove(this.Id);
        }
    }
}