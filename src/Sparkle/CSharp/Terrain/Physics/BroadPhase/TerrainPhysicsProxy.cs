using Jitter2.Collision;
using Jitter2.LinearMath;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.Terrain.Chunks;

namespace Sparkle.CSharp.Terrain.Physics.BroadPhase;

public class TerrainPhysicsProxy<T> : IDynamicTreeProxy, IRayCastable where T : class, IChunk<T> {
    
    /// <summary>
    /// Gets or sets the dynamic-tree node pointer used by Jitter.
    /// </summary>
    public int NodePtr { get; set; }
    
    /// <summary>
    /// Gets the velocity of the proxy. Terrain is static, so this is always zero.
    /// </summary>
    public JVector Velocity => JVector.Zero;
    
    /// <summary>
    /// Gets the world-space broadphase bounds of the terrain.
    /// </summary>
    public JBoundingBox WorldBoundingBox {
        get {
            if (this._owner.UseInfiniteProxyBounds) {
                return new JBoundingBox(new JVector(-1000000.0F), new JVector(1000000.0F));
            }
            
            return this._owner.Provider.GetWorldBounds(this._owner);
        }
    }
    
    /// <summary>
    /// Gets or sets the dynamic-tree set index used by Jitter.
    /// </summary>
    public int SetIndex { get; set; }
    
    /// <summary>
    /// The owning terrain physics component.
    /// </summary>
    private readonly TerrainPhysics3D<T> _owner;
    
    /// <summary>
    /// Initializes a new terrain broadphase proxy.
    /// </summary>
    /// <param name="owner">The owning terrain physics component.</param>
    public TerrainPhysicsProxy(TerrainPhysics3D<T> owner) {
        this._owner = owner;
        this.SetIndex = -1;
    }
    
    /// <summary>
    /// Performs a raycast against the terrain proxy.
    /// Terrain gameplay raycasts should be handled by the terrain implementation itself.
    /// </summary>
    /// <param name="origin">The ray origin.</param>
    /// <param name="direction">The ray direction.</param>
    /// <param name="normal">The output hit normal.</param>
    /// <param name="lambda">The output hit fraction.</param>
    /// <returns>Always <c>false</c>.</returns>
    public bool RayCast(in JVector origin, in JVector direction, out JVector normal, out float lambda) {
        normal = JVector.Zero;
        lambda = 0.0F;
        return false;
    }
}