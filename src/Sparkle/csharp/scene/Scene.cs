using Sparkle.csharp.audio;
using Sparkle.csharp.content;
using Sparkle.csharp.entity;
using Sparkle.csharp.graphics;
using Sparkle.csharp.window;

namespace Sparkle.csharp.scene; 

public abstract class Scene : IDisposable {
    
    protected Window Window => Game.Instance.Window;
    protected Graphics Graphics => Game.Instance.Graphics;
    protected ContentManager Content => Game.Instance.Content;
    protected AudioDevice AudioDevice => Game.Instance.AudioDevice;

    public readonly string Name;
    
    private readonly Dictionary<int, Entity> _entities;

    private int _entityIds;

    public Scene(string name) {
        this.Name = name;
        this._entities = new Dictionary<int, Entity>();
    }
    
    protected internal virtual void Init() {
        
    }
    
    protected internal virtual void Update() {
        foreach (Entity entity in this._entities.Values) {
            entity.Update();
        }
    }
    
    protected internal virtual void FixedUpdate() {
        foreach (Entity entity in this._entities.Values) {
            entity.FixedUpdate();
        }
    }
    
    protected internal virtual void Draw() {
        foreach (Entity entity in this._entities.Values) {
            entity.Draw();
        }
    }

    public void AddEntity(Entity entity) {
        entity.Id = this._entityIds++;
        entity.Init();
        
        this._entities.Add(entity.Id, entity);
    }
    
    public void RemoveEntity(int id) {
        this._entities[id].Dispose();
        this._entities.Remove(id);
    }
    
    public void RemoveEntity(Entity entity) {
        this.RemoveEntity(entity.Id);
    }

    public Entity GetEntity(int id) {
        return this._entities[id];
    }

    public Entity[] GetEntities() {
        return this._entities.Values.ToArray();
    }
    
    public IEnumerable<Entity> GetEntitiesWithTag(string tag) {
        foreach (Entity entity in this._entities.Values) {
            if (entity.Tag == tag) {
                yield return entity;
            }
        }
    }

    public virtual void Dispose() {
        foreach (Entity entity in this._entities.Values) {
            entity.Dispose();
        }
    }
}