using Jitter2;
using Jitter2.Collision.Shapes;
using Jitter2.LinearMath;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.Terrain.Chunks;

namespace Sparkle.CSharp.Terrain.Physics;

public interface ITerrainPhysicsProvider<T> where T : class, IChunk<T> {
    
    /// <summary>
    /// Gets the terrain proxy bounds in world space.
    /// </summary>
    /// <param name="terrainPhysics">The owning terrain physics component.</param>
    /// <returns>The terrain broadphase bounds.</returns>
    JBoundingBox GetWorldBounds(TerrainPhysics3D<T> terrainPhysics);
    
    /// <summary>
    /// Generates contacts between the terrain and a rigid body shape.
    /// </summary>
    /// <param name="terrainPhysics">The owning terrain physics component.</param>
    /// <param name="world">The physics world.</param>
    /// <param name="bodyShape">The body shape overlapping the terrain proxy.</param>
    /// <param name="contactIdBase">Base id for generated terrain contacts.</param>
    /// <param name="contactIdCount">Number of reserved terrain contact ids.</param>
    void GenerateContacts(TerrainPhysics3D<T> terrainPhysics, World world, RigidBodyShape bodyShape, ulong contactIdBase, ulong contactIdCount);
}