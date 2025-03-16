using Bliss.CSharp;
using Bliss.CSharp.Effects;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.Physics;
using Sparkle.CSharp.Physics.Dim2;
using Sparkle.CSharp.Physics.Dim3;

namespace Sparkle.CSharp.Scenes;

public abstract class Scene : Disposable {
    
    public string Name { get; private set; }
    public SceneType Type { get; private set; }
    
    public Simulation Simulation { get; private set; }
    public Effect? FilterEffect { get; private set; }
    // TODO: Add Skybox
    // TODO: Add FilterEffect

    internal Dictionary<uint, Entity> Entities;
    private uint _entityIds;
    
    protected Scene(string name, SceneType type, Simulation? simulation = null) {
        this.Name = name;
        this.Type = type;
        this.Simulation = simulation ?? (type == SceneType.Scene2D ? new Simulation2D(new PhysicsSettings2D()) : new Simulation3D(new PhysicsSettings3D()));
        this.Entities = new Dictionary<uint, Entity>();
    }

    protected internal virtual void Init() { }

    protected internal virtual void Update(double delta) {
        foreach (Entity entity in this.Entities.Values) {
            entity.Update(delta);
        }
    }

    protected internal virtual void AfterUpdate(double delta) {
        foreach (Entity entity in this.Entities.Values) {
            entity.AfterUpdate(delta);
        }
    }

    protected internal virtual void FixedUpdate(double timeStep) {
        this.Simulation.Step(timeStep);
        
        foreach (Entity entity in this.Entities.Values) {
            entity.FixedUpdate(timeStep);
        }
    }

    protected internal virtual void Draw(GraphicsContext context) {
        foreach (Entity entity in this.Entities.Values) {
            entity.Draw(context);
        }
    }

    protected internal virtual void Resize(Rectangle rectangle) {
        foreach (Entity entity in this.Entities.Values) {
            entity.Resize(rectangle);
        }
    }

    public IEnumerable<Entity> GetEntities() {
        return this.Entities.Values;
    }
    
    public IEnumerable<Entity> GetEntitiesWithTag(string tag) {
        return this.Entities.Values.Where(entity => entity.Tag == tag);
    }

    public bool HasEntity(uint id) {
        return this.Entities.ContainsKey(id);
    }

    public Entity? GetEntity(uint id) {
        if (!this.TryGetEntity(id, out Entity? result)) {
            return null;
        }

        return result;
    }

    public bool TryGetEntity(uint id, out Entity? entity) {
        return this.Entities.TryGetValue(id, out entity);
    }

    public void AddEntity(Entity entity) {
        if (!this.TryAddEntity(entity)) {
            throw new Exception($"The entity with the id: [{entity.Id}] is already present in the scene or has been added to another one!");
        }
    }

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

    public void RemoveEntity(Entity entity) {
        if (!this.TryRemoveEntity(entity)) {
            throw new Exception($"Failed to Remove/Dispose the entity: [{entity.Id}] from the scene: [{this.Name}]");
        }
    }

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

    public void RemoveEntity(uint id) {
        if (!this.TryRemoveEntity(id)) {
            throw new Exception($"Failed to Remove/Dispose the entity: [{id}] from the scene: [{this.Name}]");
        }
    }

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