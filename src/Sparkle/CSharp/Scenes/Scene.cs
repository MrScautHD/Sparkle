using Bliss.CSharp;
using Bliss.CSharp.Logging;
using Sparkle.CSharp.Effects;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Physics;
using Sparkle.CSharp.Physics.Dim2;
using Sparkle.CSharp.Physics.Dim3;
using Sparkle.CSharp.Rendering.Renderers;

namespace Sparkle.CSharp.Scenes;

public abstract class Scene : Disposable {
    
    public readonly string Name;
    public readonly SceneType Type;
    
    public Simulation Simulation { get; private set; }
    public Skybox? Skybox { get; private set; }
    public Effect? FilterEffect { get; private set; }

    internal readonly Dictionary<uint, Entity> Entities;
    private uint _entityIds;
    
    public bool HasInitialized { get; private set; }
    
    /// <summary>
    /// Initializes a new instance of the Scene class with the specified parameters.
    /// </summary>
    /// <param name="name">The name of the scene.</param>
    /// <param name="type">The type of the scene.</param>
    /// <param name="simulation">Optional simulation for the scene. If not provided, a default simulation is created based on the scene type.</param>
    protected Scene(string name, SceneType type, Simulation? simulation = default) {
        this.Name = name;
        this.Type = type;
        this.Simulation = simulation ?? (type == SceneType.Scene3D ? new Simulation3D(new PhysicsSettings3D()) : new Simulation2D(new PhysicsSettings2D()));
        this.Entities = new Dictionary<uint, Entity>();
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
        foreach (Entity entity in this.Entities.Values) {
            entity.Update();
        }
    }
    
    /// <summary>
    /// Called after the Update method on each tick to further update dynamic elements and game logic.
    /// </summary>
    protected internal virtual void AfterUpdate() {
        foreach (Entity entity in this.Entities.Values) {
            entity.AfterUpdate();
        }
    }
    
    /// <summary>
    /// Is invoked at a fixed rate of every <see cref="GameSettings.FixedTimeStep"/> frames following the <see cref="AfterUpdate"/> method.
    /// It is used for handling physics and other fixed-time operations.
    /// </summary>
    protected internal virtual void FixedUpdate() {
        this.Simulation.Step(1.0F / Game.Instance.Settings.FixedTimeStep);
        
        foreach (Entity entity in this.Entities.Values) {
            entity.FixedUpdate();
        }
    }
    
    /// <summary>
    /// Is called every tick, used for rendering stuff.
    /// </summary>
    protected internal virtual void Draw() {
        this.Skybox?.Draw();
        
        foreach (Entity entity in this.Entities.Values) {
            entity.Draw();
        }
    }
    
    /// <summary>
    /// Retrieves an entity from the collection by its ID.
    /// </summary>
    /// <param name="id">The ID of the entity to be retrieved.</param>
    /// <returns>The entity associated with the specified ID.</returns>
    public Entity GetEntity(uint id) {
        return this.Entities[id];
    }

    /// <summary>
    /// Retrieves an array of all entities currently in the collection.
    /// </summary>
    /// <returns>An array containing all entities in the collection.</returns>
    public Entity[] GetEntities() {
        return this.Entities.Values.ToArray();
    }

    /// <summary>
    /// Returns an array of entities with the specified tag.
    /// </summary>
    /// <param name="tag">The tag to filter entities by.</param>
    /// <returns>An IEnumerable of Entity objects with the specified tag.</returns>
    public IEnumerable<Entity> GetEntitiesWithTag(string tag) {
        foreach (Entity entity in this.Entities.Values) {
            if (entity.Tag == tag) {
                yield return entity;
            }
        }
    }
    
    /// <summary>
    /// Adds an entity to the collection and initializes it.
    /// </summary>
    /// <param name="entity">The entity to be added.</param>
    public void AddEntity(Entity entity) {
        if (this.Entities.ContainsValue(entity)) {
            Logger.Warn($"The entity with the id: [{entity.Id}] is already present in the Scene!");
            return;
        }

        entity.Id = ++this._entityIds;
        entity.Init();
        
        this.Entities.Add(entity.Id, entity);
    }
    
    /// <summary>
    /// Removes an entity from the collection and disposes of it.
    /// </summary>
    /// <param name="id">The ID of the entity to be removed.</param>
    public void RemoveEntity(uint id) {
        this.Entities[id].Dispose();
    }
    
    /// <summary>
    /// Removes an entity from the collection and disposes of it.
    /// </summary>
    /// <param name="entity">The entity to be removed.</param>
    public void RemoveEntity(Entity entity) {
        this.RemoveEntity(entity.Id);
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

    /// <summary>
    /// Sets the filter effect for the scene.
    /// </summary>
    /// <param name="effect">The filter effect to apply to the scene. Pass null to remove the filter effect.</param>
    public void SetFilterEffect(Effect? effect) {
        this.FilterEffect = effect;
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            foreach (Entity entity in this.Entities.Values) {
                entity.Dispose();
            }
            
            this._entityIds = 0;
            
            this.Skybox?.Dispose();
            this.Simulation.Dispose();
        }
    }
}