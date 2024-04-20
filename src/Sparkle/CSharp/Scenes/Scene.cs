using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Physics;
using Sparkle.CSharp.Rendering.Renderers;

namespace Sparkle.CSharp.Scenes;

public abstract class Scene : Disposable {
    
    public readonly string Name;
    
    public readonly SceneType Type;
    public readonly Simulation Simulation;

    public Skybox? Skybox { get; private set; }
    
    private readonly Dictionary<int, Entity> _entities;
    private int _entityIds;
    
    public bool HasInitialized { get; private set; }
    
    /// <summary>
    /// Represents an abstract scene in the game.
    /// </summary>
    /// <param name="name">The scene name.</param>
    /// <param name="type">The scene type (3D or 2D).</param>
    /// <param name="settings">The physics settings.</param>
    protected Scene(string name, SceneType type, PhysicsSettings? settings = default) {
        this.Name = name;
        this.Type = type;
        this.Simulation = new Simulation(settings ?? new PhysicsSettings());
        this._entities = new Dictionary<int, Entity>();
    }
    
    /// <summary>
    /// Used for Initializes objects.
    /// </summary>
    protected internal virtual void Init() {
        this.HasInitialized = true;
    }
    
    /// <summary>
    /// Is invoked during each tick and is used for updating dynamic elements and game logic.
    /// </summary>
    protected internal virtual void Update() {
        foreach (Entity entity in this._entities.Values) {
            entity.Update();
        }
    }
    
    /// <summary>
    /// Called after the Update method on each tick to further update dynamic elements and game logic.
    /// </summary>
    protected internal virtual void AfterUpdate() {
        foreach (Entity entity in this._entities.Values) {
            entity.AfterUpdate();
        }
    }
    
    /// <summary>
    /// Is invoked at a fixed rate of every <see cref="GameSettings.FixedTimeStep"/> frames following the <see cref="AfterUpdate"/> method.
    /// It is used for handling physics and other fixed-time operations.
    /// </summary>
    protected internal virtual void FixedUpdate() {
        this.Simulation.Step(1.0F / Game.Instance.Settings.FixedTimeStep);
        
        foreach (Entity entity in this._entities.Values) {
            entity.FixedUpdate();
        }
    }
    
    /// <summary>
    /// Is called every tick, used for rendering stuff.
    /// </summary>
    protected internal virtual void Draw() {
        this.Skybox?.Draw();
        
        foreach (Entity entity in this._entities.Values) {
            entity.Draw();
        }
    }
    
    /// <summary>
    /// Adds an entity to the collection and initializes it.
    /// </summary>
    /// <param name="entity">The entity to be added.</param>
    public void AddEntity(Entity entity) {
        if (this._entities.ContainsValue(entity)) {
            Logger.Warn($"The entity with the id: [{entity.Id}] is already present in the Scene!");
            return;
        }
        
        entity.Id = this._entityIds++;
        entity.Init();
        
        this._entities.Add(entity.Id, entity);
    }
    
    /// <summary>
    /// Removes an entity from the collection and disposes of it.
    /// </summary>
    /// <param name="id">The ID of the entity to be removed.</param>
    public void RemoveEntity(int id) {
        if (!this._entities.ContainsKey(id)) {
            Logger.Warn($"The entity with the id: [{id}] is already removed from the Scene!");
            return;
        }
        
        this._entities[id].Dispose();
        this._entities.Remove(id);
    }
    
    /// <summary>
    /// Removes an entity from the collection and disposes of it.
    /// </summary>
    /// <param name="entity">The entity to be removed.</param>
    public void RemoveEntity(Entity entity) {
        this.RemoveEntity(entity.Id);
    }

    /// <summary>
    /// Retrieves an entity from the collection by its ID.
    /// </summary>
    /// <param name="id">The ID of the entity to be retrieved.</param>
    /// <returns>The entity associated with the specified ID.</returns>
    public Entity GetEntity(int id) {
        return this._entities[id];
    }

    /// <summary>
    /// Retrieves an array of all entities currently in the collection.
    /// </summary>
    /// <returns>An array containing all entities in the collection.</returns>
    public Entity[] GetEntities() {
        return this._entities.Values.ToArray();
    }

    /// <summary>
    /// Returns an array of entities with the specified tag.
    /// </summary>
    /// <param name="tag">The tag to filter entities by.</param>
    /// <returns>An IEnumerable of Entity objects with the specified tag.</returns>
    public IEnumerable<Entity> GetEntitiesWithTag(string tag) {
        foreach (Entity entity in this._entities.Values) {
            if (entity.Tag == tag) {
                yield return entity;
            }
        }
    }
    
    /// <summary>
    /// Sets the skybox for the 3D camera.
    /// </summary>
    /// <param name="skybox">The skybox to set.</param>
    public void SetSkybox(Skybox? skybox) {
        this.Skybox?.Dispose();
        this.Skybox = skybox;
        
        if (this.Skybox != null && !this.Skybox.HasInitialized) {
            this.Skybox.Init();
        }
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            foreach (Entity entity in this._entities.Values) {
                entity.Dispose();
            }
            
            this._entities.Clear();
            this._entityIds = 0;
            
            this.Skybox?.Dispose();
            this.Simulation.Dispose();
        }
    }
}