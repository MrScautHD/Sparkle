using Bliss.CSharp;
using Bliss.CSharp.Effects;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.Physics;
using Sparkle.CSharp.Physics.Dim2;
using Sparkle.CSharp.Physics.Dim3;
using Veldrid;

namespace Sparkle.CSharp.Scenes;

public abstract class Scene : Disposable {
    
    /// <summary>
    /// Gets the name of the scene.
    /// </summary>
    public string Name { get; private set; }
    
    /// <summary>
    /// Gets the type of the scene (2D or 3D).
    /// </summary>
    public SceneType Type { get; private set; }
    
    /// <summary>
    /// Gets the physics simulation associated with the scene.
    /// </summary>
    public Simulation Simulation { get; private set; }
    
    /// <summary>
    /// Gets or sets the optional filter effect applied to the scene.
    /// </summary>
    public Effect? FilterEffect { get; private set; }
    
    // TODO: Add Skybox
    
    /// <summary>
    /// Stores all entities within the scene.
    /// </summary>
    internal Dictionary<uint, Entity> Entities;
    
    /// <summary>
    /// Counter for generating unique entity IDs.
    /// </summary>
    private uint _entityIds;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Scene"/> class.
    /// </summary>
    /// <param name="name">The name of the scene.</param>
    /// <param name="type">The type of the scene (2D or 3D).</param>
    /// <param name="simulation">The optional physics simulation. If not provided, a default one is created.</param>
    protected Scene(string name, SceneType type, Simulation? simulation = null) {
        this.Name = name;
        this.Type = type;
        this.Simulation = simulation ?? (type == SceneType.Scene2D ? new Simulation2D(new PhysicsSettings2D()) : new Simulation3D(new PhysicsSettings3D()));
        this.Entities = new Dictionary<uint, Entity>();
    }

    /// <summary>
    /// Initializes the scene. Can be overridden in derived classes.
    /// </summary>
    protected internal virtual void Init() { }

    /// <summary>
    /// Updates all entities in the scene.
    /// </summary>
    /// <param name="delta">The time elapsed since the last update.</param>
    protected internal virtual void Update(double delta) {
        foreach (Entity entity in this.Entities.Values) {
            entity.Update(delta);
        }
    }

    /// <summary>
    /// Called after the main update loop. Allows for post-update logic.
    /// </summary>
    /// <param name="delta">The time elapsed since the last update.</param>
    protected internal virtual void AfterUpdate(double delta) {
        foreach (Entity entity in this.Entities.Values) {
            entity.AfterUpdate(delta);
        }
    }

    /// <summary>
    /// Performs physics and other fixed timeStep updates.
    /// </summary>
    /// <param name="timeStep">The fixed timeStep duration.</param>
    protected internal virtual void FixedUpdate(double timeStep) {
        this.Simulation.Step(timeStep);
        
        foreach (Entity entity in this.Entities.Values) {
            entity.FixedUpdate(timeStep);
        }
    }

    /// <summary>
    /// Draws all entities in the scene.
    /// </summary>
    /// <param name="context">The graphics context.</param>
    /// <param name="framebuffer">The framebuffer to render into.</param>
    protected internal virtual void Draw(GraphicsContext context, Framebuffer framebuffer) {
        foreach (Entity entity in this.Entities.Values) {
            entity.Draw(context, framebuffer);
        }
    }

    /// <summary>
    /// Handles window resizing events and updates entities accordingly.
    /// </summary>
    /// <param name="rectangle">The new window dimensions.</param>
    protected internal virtual void Resize(Rectangle rectangle) {
        foreach (Entity entity in this.Entities.Values) {
            entity.Resize(rectangle);
        }
    }

    /// <summary>
    /// Retrieves all entities in the scene.
    /// </summary>
    public IEnumerable<Entity> GetEntities() {
        return this.Entities.Values;
    }
    
    /// <summary>
    /// Retrieves all entities with a specific tag.
    /// </summary>
    /// <param name="tag">The tag to filter entities.</param>
    public IEnumerable<Entity> GetEntitiesWithTag(string tag) {
        return this.Entities.Values.Where(entity => entity.Tag == tag);
    }

    /// <summary>
    /// Checks if an entity exists in the scene.
    /// </summary>
    /// <param name="id">The entity ID.</param>
    public bool HasEntity(uint id) {
        return this.Entities.ContainsKey(id);
    }

    /// <summary>
    /// Gets an entity by its ID.
    /// </summary>
    /// <param name="id">The entity ID.</param>
    public Entity? GetEntity(uint id) {
        if (!this.TryGetEntity(id, out Entity? result)) {
            return null;
        }

        return result;
    }

    /// <summary>
    /// Tries to retrieve an entity by its ID.
    /// </summary>
    /// <param name="id">The entity ID.</param>
    /// <param name="entity">The retrieved entity, if found.</param>
    public bool TryGetEntity(uint id, out Entity? entity) {
        return this.Entities.TryGetValue(id, out entity);
    }

    /// <summary>
    /// Adds an entity to the scene.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <exception cref="Exception">Thrown if the entity is already present in the scene or belongs to another scene.</exception>
    public void AddEntity(Entity entity) {
        if (!this.TryAddEntity(entity)) {
            throw new Exception($"The entity with the id: [{entity.Id}] is already present in the scene or has been added to another one!");
        }
    }

    /// <summary>
    /// Attempts to add an entity to the scene.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <returns>True if the entity was successfully added; otherwise, false.</returns>
    public bool TryAddEntity(Entity entity) {
        if (this.Entities.ContainsKey(entity.Id)) {
            return false;
        }

        if (entity.Id != 0) {
            return false;
        }

        entity.Id = ++this._entityIds;
        entity.Init();
        
        this.Entities.Add(entity.Id, entity);
        return true;
    }

    /// <summary>
    /// Removes an entity from the scene.
    /// </summary>
    /// <param name="entity">The entity to remove.</param>
    /// <exception cref="Exception">Thrown if the entity could not be removed.</exception>
    public void RemoveEntity(Entity entity) {
        if (!this.TryRemoveEntity(entity)) {
            throw new Exception($"Failed to Remove/Dispose the entity: [{entity.Id}] from the scene: [{this.Name}]");
        }
    }

    /// <summary>
    /// Attempts to remove an entity from the scene.
    /// </summary>
    /// <param name="entity">The entity to remove.</param>
    /// <returns>True if the entity was successfully removed; otherwise, false.</returns>
    public bool TryRemoveEntity(Entity entity) {
        if (!this.Entities.ContainsKey(entity.Id)) {
            return false;
        }
        
        entity.Dispose();
        
        // Ensure the component is removed, even if `Dispose` was overridden incorrectly.
        if (this.Entities.ContainsKey(entity.Id)) {
            this.Entities.Remove(entity.Id);
        }

        return true;
    }

    /// <summary>
    /// Removes an entity from the scene by its ID.
    /// </summary>
    /// <param name="id">The ID of the entity to remove.</param>
    /// <exception cref="Exception">Thrown if the entity could not be removed.</exception>
    public void RemoveEntity(uint id) {
        if (!this.TryRemoveEntity(id)) {
            throw new Exception($"Failed to Remove/Dispose the entity: [{id}] from the scene: [{this.Name}]");
        }
    }

    /// <summary>
    /// Attempts to remove an entity from the scene by its ID.
    /// </summary>
    /// <param name="id">The ID of the entity to remove.</param>
    /// <returns>True if the entity was successfully removed; otherwise, false.</returns>
    public bool TryRemoveEntity(uint id) {
        if (!this.Entities.TryGetValue(id, out Entity? entity)) {
            return false;
        }
        
        entity.Dispose();
        
        // Ensure the component is removed, even if `Dispose` was overridden incorrectly.
        if (this.Entities.ContainsKey(id)) {
            this.Entities.Remove(id);
        }

        return true;
    }

    /// <summary>
    /// Sets the filter effect to be applied to the scene during rendering.
    /// </summary>
    /// <param name="effect">The filter effect to apply. Pass null to remove the current filter effect.</param>
    public void SetFilterEffect(Effect? effect) {
        this.FilterEffect = effect;
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            var enumerator = this.Entities.GetEnumerator();
            
            while (enumerator.MoveNext()) {
                Entity entity = enumerator.Current.Value;
                entity.Dispose();
                
                // Ensure the component is removed, even if `Dispose` was overridden incorrectly.
                if (this.Entities.ContainsKey(entity.Id)) {
                    this.Entities.Remove(entity.Id);
                }
            }

            this._entityIds = 0;
            this.Simulation.Dispose();
        }
    }
}