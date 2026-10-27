using Jitter2;
using Jitter2.Collision;
using Jitter2.Collision.Shapes;
using Jitter2.Dynamics;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.Terrain.Chunks;

namespace Sparkle.CSharp.Terrain.Physics.BroadPhase;

public class TerrainPhysicsFilter<T> : IBroadPhaseFilter where T : class, IChunk<T> {
    
    /// <summary>
    /// The number of contact ids reserved for terrain-generated contacts.
    /// </summary>
    private const ulong _terrainContactIdCount = 8000000;
    
    /// <summary>
    /// The physics world.
    /// </summary>
    private readonly World _world;
    
    /// <summary>
    /// The terrain broadphase proxy.
    /// </summary>
    private readonly TerrainPhysicsProxy<T> _terrainProxy;
    
    /// <summary>
    /// The owning terrain physics component.
    /// </summary>
    private readonly TerrainPhysics3D<T> _owner;
    
    /// <summary>
    /// The previous broadphase filter, if any.
    /// </summary>
    private readonly IBroadPhaseFilter? _previousFilter;
    
    /// <summary>
    /// The first reserved contact id for generated terrain contacts.
    /// </summary>
    private readonly ulong _terrainContactIdBase;
    
    /// <summary>
    /// Initializes a new terrain physics broadphase filter.
    /// </summary>
    /// <param name="world">The physics world.</param>
    /// <param name="terrainProxy">The terrain broadphase proxy.</param>
    /// <param name="owner">The owning terrain physics component.</param>
    /// <param name="previousFilter">The previous broadphase filter.</param>
    public TerrainPhysicsFilter(World world, TerrainPhysicsProxy<T> terrainProxy, TerrainPhysics3D<T> owner, IBroadPhaseFilter? previousFilter) {
        this._world = world;
        this._terrainProxy = terrainProxy;
        this._owner = owner;
        this._previousFilter = previousFilter;
        (this._terrainContactIdBase, _) = World.RequestId((int) _terrainContactIdCount);
    }
    
    /// <summary>
    /// Filters a broadphase pair and generates contacts for pairs involving the terrain proxy.
    /// </summary>
    /// <param name="shapeA">The first broadphase proxy.</param>
    /// <param name="shapeB">The second broadphase proxy.</param>
    /// <returns><c>true</c> when Jitter should process the pair normally; <c>false</c> when this filter handled or rejected the pair.</returns>
    public bool Filter(IDynamicTreeProxy shapeA, IDynamicTreeProxy shapeB) {
        bool involvesTerrain = shapeA == this._terrainProxy || shapeB == this._terrainProxy;
        
        if (!involvesTerrain) {
            return this._previousFilter?.Filter(shapeA, shapeB) ?? true;
        }
        
        IDynamicTreeProxy otherProxy = shapeA == this._terrainProxy ? shapeB : shapeA;
        
        if (otherProxy is not RigidBodyShape bodyShape) {
            return false;
        }
        
        RigidBody body = bodyShape.RigidBody;
        
        if (!body.IsActive || body.MotionType == MotionType.Static) {
            return false;
        }
        
        this._owner.Provider.GenerateContacts(this._owner, this._world, bodyShape, this._terrainContactIdBase, _terrainContactIdCount);
        return false;
    }
}