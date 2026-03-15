using System.Numerics;
using Box2D;
using Sparkle.CSharp.Physics.Dim2;

namespace Sparkle.CSharp.Graphics.Particles.Dim2.Collisions.Providers;

public class ParticleCollisionProvider2D : IParticleCollisionProvider2D {
    
    /// <summary>
    /// The 2D simulation used to perform collision ray casts.
    /// </summary>
    private Simulation2D _simulation;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ParticleCollisionProvider2D"/> class.
    /// </summary>
    /// <param name="simulation">The 2D physics simulation used for collision queries.</param>
    public ParticleCollisionProvider2D(Simulation2D simulation) {
        this._simulation = simulation;
    }
    
    /// <summary>
    /// Attempts to ray cast from a start position to an end position and returns collision information when a hit occurs.
    /// </summary>
    /// <param name="start">The world-space start position of the ray.</param>
    /// <param name="end">The world-space end position of the ray.</param>
    /// <param name="hit">When this method returns <see langword="true"/>, contains information about the detected collision.</param>
    /// <returns><see langword="true"/> if the ray hits a surface; otherwise, <see langword="false"/>.</returns>
    public bool TryRayCast(Vector2 start, Vector2 end, out ParticleCollisionHit2D hit) {
        Vector2 translation = end - start;
        
        // Ignore zero-length rays.
        if (translation.LengthSquared() <= float.Epsilon) {
            hit = default;
            return false;
        }
        
        QueryFilter filter = new QueryFilter();
        RayResult result = this._simulation.World.CastRayClosest(start, translation, filter);
        
        // Ignore invalid hits and anything outside the segment.
        if (!result.Shape.Valid || result.Fraction < 0.0F || result.Fraction > 1.0F) {
            hit = default;
            return false;
        }
        
        hit = new ParticleCollisionHit2D(result.Point, result.Normal);
        return true;
    }
}