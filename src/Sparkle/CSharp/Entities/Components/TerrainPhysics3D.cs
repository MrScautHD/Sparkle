using System.Numerics;
using Bliss.CSharp.Transformations;
using Jitter2;
using Jitter2.Collision;
using Sparkle.CSharp.Physics.Dim3;
using Sparkle.CSharp.Scenes;
using Sparkle.CSharp.Terrain.Chunks;
using Sparkle.CSharp.Terrain.Physics;
using Sparkle.CSharp.Terrain.Physics.BroadPhase;

namespace Sparkle.CSharp.Entities.Components;

public class TerrainPhysics3D<T> : Component where T : class, IChunk<T> {
    
    /// <summary>
    /// Gets a list of component types that are incompatible with this component.
    /// </summary>
    public override IReadOnlyList<Type> InCompatibleTypes => [
        typeof(RigidBody2D),
        typeof(RigidBody3D),
        typeof(SoftBody3D)
    ];
    
    /// <summary>
    /// Gets the current 3D simulation instance used by the scene.
    /// Throws an <see cref="InvalidOperationException"/> if the simulation is not of type <see cref="Simulation3D"/>.
    /// </summary>
    public Simulation3D Simulation => SceneManager.Simulation as Simulation3D ?? throw new InvalidOperationException("The current simulation must be of type Simulation3D.");
    
    /// <summary>
    /// Gets the physics world from the current 3D simulation.
    /// </summary>
    public World World => this.Simulation.World;
    
    /// <summary>
    /// Gets the terrain renderer/data component.
    /// </summary>
    public Terrain3D<T> Terrain3D => this._terrain3D ?? throw new InvalidOperationException("TerrainPhysics3D is not initialized.");
    
    /// <summary>
    /// Gets the provider that generates terrain physics contacts.
    /// </summary>
    public ITerrainPhysicsProvider<T> Provider { get; private set; }
    
    /// <summary>
    /// Gets or sets the extra terrain sample margin used by providers that expand body AABBs.
    /// </summary>
    public int ContactSampleMargin;
    
    /// <summary>
    /// Gets or sets the maximum number of terrain cells or elements tested per body shape.
    /// </summary>
    public int MaxCellsPerShape;
    
    /// <summary>
    /// Gets or sets whether providers may filter internal/shared terrain edges.
    /// </summary>
    public bool FilterInternalEdges;
    
    /// <summary>
    /// Gets or sets the dot threshold used by providers for internal edge filtering.
    /// </summary>
    public float InternalEdgeNormalThreshold;
    
    /// <summary>
    /// Gets or sets the edge distance threshold used by providers for internal edge filtering.
    /// </summary>
    public float InternalEdgeDistanceThreshold;
    
    /// <summary>
    /// Gets or sets whether the terrain proxy should use huge bounds.
    /// </summary>
    public bool UseInfiniteProxyBounds;
    
    /// <summary>
    /// The terrain component on the same entity.
    /// </summary>
    private Terrain3D<T>? _terrain3D;
    
    /// <summary>
    /// The terrain broadphase proxy registered in Jitter.
    /// </summary>
    private TerrainPhysicsProxy<T>? _terrainProxy;
    
    /// <summary>
    /// The broadphase filter that delegates terrain contacts to the provider.
    /// </summary>
    private TerrainPhysicsFilter<T>? _terrainFilter;
    
    /// <summary>
    /// The previous broadphase filter restored when this component is disposed.
    /// </summary>
    private IBroadPhaseFilter? _previousBroadPhaseFilter;
    
    /// <summary>
    /// Initializes a new <see cref="TerrainPhysics3D{T}"/> component.
    /// </summary>
    /// <param name="offsetPosition">The component offset.</param>
    /// <param name="provider">The terrain physics provider.</param>
    /// <param name="contactSampleMargin">The number of extra terrain cells that are sampled around a contact area. A larger margin finds contacts that lie slightly outside of the shape's bounds, at the cost of more samples per contact.</param>
    /// <param name="maxCellsPerShape">The maximum number of terrain cells that are turned into collision geometry for one shape. It limits the cost of large or fast-moving shapes, and cells beyond the limit are ignored.</param>
    /// <param name="filterInternalEdges">Specifies whether contacts at the inner edges between neighboring terrain triangles are filtered. This prevents shapes from catching on these edges and being pushed in a wrong direction (ghost collisions).</param>
    /// <param name="internalEdgeNormalThreshold">The threshold for the contact normal when internal edges are filtered. A contact that points along an inner edge counts as an edge contact, and the filter corrects it.</param>
    /// <param name="internalEdgeDistanceThreshold">The distance, in world units, from an inner edge within which a contact is treated as an edge contact when internal edges are filtered.</param>
    /// <param name="useInfiniteProxyBounds">Specifies whether the terrain uses infinite bounds in the broad phase of the physics world. The terrain is then always tested against every shape, which is useful for a terrain that is larger than the physics world, but it costs more than tight bounds.</param>
    public TerrainPhysics3D(
        Vector3 offsetPosition,
        ITerrainPhysicsProvider<T> provider,
        int contactSampleMargin = 2,
        int maxCellsPerShape = 4096,
        bool filterInternalEdges = true,
        float internalEdgeNormalThreshold = 0.5F,
        float internalEdgeDistanceThreshold = 0.01F,
        bool useInfiniteProxyBounds = false) : base(offsetPosition) {
        this.Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        this.ContactSampleMargin = contactSampleMargin;
        this.MaxCellsPerShape = maxCellsPerShape;
        this.FilterInternalEdges = filterInternalEdges;
        this.InternalEdgeNormalThreshold = internalEdgeNormalThreshold;
        this.InternalEdgeDistanceThreshold = internalEdgeDistanceThreshold;
        this.UseInfiniteProxyBounds = useInfiniteProxyBounds;
    }
    
    /// <summary>
    /// Initializes the component and registers the terrain broadphase proxy.
    /// </summary>
    protected internal override void Init() {
        base.Init();
        
        if (!this.Entity.TryGetComponent(out this._terrain3D)) {
            throw new InvalidOperationException($"The TerrainPhysics3D component requires a Terrain3D component to function properly. Please ensure a Terrain3D is attached to Entity with the id: [{this.Entity.Id}]");
        }
        
        // Add a new proxy to broadphase tree.
        this._terrainProxy = new TerrainPhysicsProxy<T>(this);
        this.World.DynamicTree.AddProxy(this._terrainProxy, false);
        
        // The terrain filter handles terrain pairs and forwards all others to the previous filter.
        this._previousBroadPhaseFilter = this.World.BroadPhaseFilter;
        this._terrainFilter = new TerrainPhysicsFilter<T>(this.World, this._terrainProxy, this, this._previousBroadPhaseFilter);
        this.World.BroadPhaseFilter = this._terrainFilter;
    }
    
    /// <summary>
    /// Converts a terrain-local position to world space using the terrain entity transform.
    /// </summary>
    /// <param name="terrainPosition">The terrain-local position.</param>
    /// <returns>The world-space position.</returns>
    public Vector3 TerrainToWorldPosition(Vector3 terrainPosition) {
        Transform transform = this.Entity.WorldTransform;
        Vector3 scaled = terrainPosition * transform.Scale;
        Vector3 rotated = Vector3.Transform(scaled, transform.Rotation);
        return transform.Translation + rotated;
    }
    
    /// <summary>
    /// Converts a world-space position to terrain-local space using the terrain entity transform.
    /// </summary>
    /// <param name="worldPosition">The world-space position.</param>
    /// <returns>The terrain-local position.</returns>
    public Vector3 WorldToTerrainPosition(Vector3 worldPosition) {
        Transform transform = this.Entity.WorldTransform;
        Quaternion inverseRotation = Quaternion.Inverse(transform.Rotation);
        Vector3 local = Vector3.Transform(worldPosition - transform.Translation, inverseRotation);
        return local / transform.Scale;
    }
    
    /// <summary>
    /// Unregisters the terrain proxy and restores the previous broadphase filter.
    /// </summary>
    /// <param name="disposing">Whether managed resources should be disposed.</param>
    protected override void Dispose(bool disposing) {
        if (disposing) {
            if (this._terrainProxy != null) {
                this.World.DynamicTree.RemoveProxy(this._terrainProxy);
                this._terrainProxy = null;
            }
            
            if (this._terrainFilter != null && this.World.BroadPhaseFilter == this._terrainFilter) {
                this.World.BroadPhaseFilter = this._previousBroadPhaseFilter;
            }
            
            this._terrainFilter = null;
            this._previousBroadPhaseFilter = null;
        }
    }
}