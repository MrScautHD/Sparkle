using System.Numerics;
using Jitter2.LinearMath;
using Sparkle.CSharp.Physics.Dim3;

namespace Sparkle.CSharp.Graphics.Particles.Dim3.Collisions.Providers;

public class ParticleCollisionProvider3D : IParticleCollisionProvider3D {
    
    /// <summary>
    /// The 3D simulation used to perform collision ray casts.
    /// </summary>
    private Simulation3D _simulation;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ParticleCollisionProvider3D"/> class.
    /// </summary>
    /// <param name="simulation">The 3D physics simulation used for collision queries.</param>
    public ParticleCollisionProvider3D(Simulation3D simulation) {
        this._simulation = simulation;
    }
    
    /// <summary>
    /// Attempts to ray cast from a start position to an end position and returns collision information when a hit occurs.
    /// </summary>
    /// <param name="start">The world-space start position of the ray.</param>
    /// <param name="end">The world-space end position of the ray.</param>
    /// <param name="hit">When this method returns <see langword="true"/>, contains information about the detected collision.</param>
    /// <returns><see langword="true"/> if the ray hits a surface; otherwise, <see langword="false"/>.</returns>
    public bool TryRayCast(Vector3 start, Vector3 end, out ParticleCollisionHit3D hit) {
        Vector3 translation = end - start;
        
        // Ignore zero-length rays.
        if (translation.LengthSquared() <= float.Epsilon) {
            hit = default;
            return false;
        }
        
        bool hasHit = this._simulation.World.DynamicTree.RayCast(start, translation, null, null, out _, out JVector normal, out float fraction);
        
        // Ignore invalid hits and anything outside the segment.
        if (!hasHit || fraction < 0.0F || fraction > 1.0F) {
            hit = default;
            return false;
        }
        
        Vector3 point = start + translation * fraction;
        
        hit = new ParticleCollisionHit3D(point, normal);
        return true;
    }
}