using System.Diagnostics.CodeAnalysis;
using Bliss.CSharp;
using Bliss.CSharp.Effects;
using Bliss.CSharp.Graphics.Rendering.Renderers.Forward;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Content;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.Graphics.Rendering;
using Sparkle.CSharp.Graphics.Rendering.Sprites;
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
    public SceneType SceneType { get; private set; }
    
    /// <summary>
    /// Gets the physics simulation associated with the scene.
    /// </summary>
    public Simulation Simulation { get; private set; }
    
    /// <summary>
    /// Gets the forward renderer responsible for rendering 3D content in the scene.
    /// </summary>
    public IRenderer Renderer { get; private set; }
    
    /// <summary>
    /// Provides tools for visual debugging of 3D physics simulations.
    /// </summary>
    public Physics3DDebugDrawer Physics3DDebugDrawer { get; private set; }
    
    /// <summary>
    /// Manages and draw sprites for the entity sprite component.
    /// </summary>
    public SpriteRenderer SpriteRenderer { get; private set; }
    
    /// <summary>
    /// The filter effect used in the scene.
    /// </summary>
    public Effect? FilterEffect;
    
    /// <summary>
    /// The skybox used in the scene.
    /// </summary>
    public SkyBox? SkyBox;
    
    /// <summary>
    /// Stores all entities within the scene.
    /// </summary>
    internal Dictionary<uint, Entity> Entities;
    
    /// <summary>
    /// Counter for generating unique entity IDs.
    /// </summary>
    private uint _entityIds;
    
    /// <summary>
    /// A collection of renderers responsible for handling multi-instance rendering within the scene.
    /// </summary>
    private List<MultiInstanceRenderer> _multiInstanceRenderers;
    
    /// <summary>
    /// Factory function used to create the renderer instance for this scene.
    /// </summary>
    private Func<GraphicsDevice, IRenderer>? _rendererFactory;
    
    /// <summary>
    /// Factory function used to create the physics simulation instance for this scene.
    /// </summary>
    private Func<Simulation>? _simulationFactory;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Scene"/> class.
    /// </summary>
    /// <param name="name">The name of the scene.</param>
    /// <param name="sceneType">The type of the scene (2D or 3D).</param>
    /// <param name="rendererFactory">Optional factory used to create a custom renderer for the scene.</param>
    /// <param name="simulationFactory">Optional factory used to create a custom physics simulation.</param>
    protected Scene(string name, SceneType sceneType, Func<GraphicsDevice, IRenderer>? rendererFactory = null, Func<Simulation>? simulationFactory = null) {
        this.Name = name;
        this.SceneType = sceneType;
        this._rendererFactory = rendererFactory;
        this._simulationFactory = simulationFactory;
        this.Entities = new Dictionary<uint, Entity>();
        this._multiInstanceRenderers = new List<MultiInstanceRenderer>();
    }
    
    /// <summary>
    /// Loads the necessary content for the scene using the content manager.
    /// </summary>
    /// <param name="content">The content manager used to load assets for the scene.</param>
    protected internal virtual void Load(ContentManager content) { }
    
    /// <summary>
    /// Initializes the scene. Can be overridden in derived classes.
    /// </summary>
    protected internal virtual void Init() {
        this.Renderer = this._rendererFactory?.Invoke(GlobalGraphicsAssets.GraphicsDevice) ?? new BasicForwardRenderer(GlobalGraphicsAssets.GraphicsDevice);
        this.Physics3DDebugDrawer = new Physics3DDebugDrawer(GlobalGraphicsAssets.GraphicsDevice, GlobalGraphicsAssets.Window);
        this.SpriteRenderer = new SpriteRenderer();
        this.Simulation = this._simulationFactory?.Invoke() ?? (this.SceneType == SceneType.Scene2D ? new Simulation2D(new PhysicsSettings2D()) : new Simulation3D(new PhysicsSettings3D()));
    }
    
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
    /// Performs a fixed update for the current scene, updating the simulation and invoking the fixed update for all entities.
    /// </summary>
    /// <param name="fixedStep">The fixed time interval to update the scene and entities.</param>
    protected internal virtual void FixedUpdate(double fixedStep) {
        this.Simulation.Step(fixedStep);
        
        foreach (Entity entity in this.Entities.Values) {
            entity.FixedUpdate(fixedStep);
        }
    }

    /// <summary>
    /// Draws all entities in the scene.
    /// </summary>
    /// <param name="context">The graphics context.</param>
    /// <param name="framebuffer">The framebuffer to render into.</param>
    protected internal virtual void Draw(GraphicsContext context, Framebuffer framebuffer) {
        
        // Draw skybox.
        this.SkyBox?.Draw(context.CommandList, framebuffer.OutputDescription);
        
        // Draw physics debug drawer.
        this.Physics3DDebugDrawer.Begin(context.CommandList, framebuffer.OutputDescription);
        
        foreach (Entity entity in this.Entities.Values) {
            if (entity.TryGetComponent(out RigidBody3D? rigidBody)) {
                if (rigidBody.DrawDebug) {
                    this.Physics3DDebugDrawer.PushColor(rigidBody.DebugDrawColor);
                    rigidBody.DebugDraw(this.Physics3DDebugDrawer);
                    this.Physics3DDebugDrawer.PopColor();
                }
            }
            else if (entity.TryGetComponent(out SoftBody3D? softBody)) {
                if (softBody.DrawDebug) {
                    this.Physics3DDebugDrawer.PushColor(softBody.DebugDrawColor);
                    softBody.DebugDraw(this.Physics3DDebugDrawer);
                    this.Physics3DDebugDrawer.PopColor();
                }
            }
        }
        
        this.Physics3DDebugDrawer.End();
        
        // Draw entities (Renderables linked to the renderer).
        foreach (Entity entity in this.Entities.Values) {
            entity.Draw(context, framebuffer);
        }
        
        // Draw multi instance renderers (Renderables linked to the renderer).
        foreach (MultiInstanceRenderer multiInstanceRenderer in this._multiInstanceRenderers) {
            multiInstanceRenderer.Draw(context, framebuffer);
        }
        
        // Draw 3D renderer.
        this.Renderer.Draw(context.CommandList, framebuffer.OutputDescription);
        
        // Draw sprite renderer.
        this.SpriteRenderer.Draw(context, framebuffer);
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
    public bool TryGetEntity(uint id, [NotNullWhen(true)] out Entity? entity) {
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
        
        entity.Scene = this;
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
    /// Adds a multi-instance renderer to the scene.
    /// </summary>
    /// <param name="renderer">The multi-instance renderer to be added to the scene.</param>
    internal void AddMultiInstanceRenderer(MultiInstanceRenderer renderer) {
        
        // Check if its already added and if yes just ignore it.
        foreach (Entity entity in this.Entities.Values) {
            if (entity.TryGetComponent(out InstancedRenderProxy? renderProxy)) {
                if (this._multiInstanceRenderers.Contains(renderProxy.MultiInstanceRenderer)) {
                    return;
                }
            }
        }
        
        // Add multi-instance renderer.
        this._multiInstanceRenderers.Add(renderer);
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
            this.Renderer.Dispose();
            this.Physics3DDebugDrawer.Dispose();
            Game.Instance?.Content.UnloadSceneContent();
        }
    }
}