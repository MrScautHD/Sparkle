using System.Numerics;

namespace Sparkle.CSharp.Graphics.Particles.Dim2.Collisions;

public struct ParticleCollisionHit2D {
    
    /// <summary>
    /// The point in world space where the collision occurred.
    /// </summary>
    public Vector2 Point { get; private set; }
    
    /// <summary>
    /// The normalized surface normal at the collision point.
    /// </summary>
    public Vector2 Normal { get; private set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ParticleCollisionHit2D"/> struct.
    /// </summary>
    /// <param name="point">The point in world space where the collision occurred.</param>
    /// <param name="normal">The surface normal at the collision point. If zero, a default upward normal is used.</param>
    public ParticleCollisionHit2D(Vector2 point, Vector2 normal) {
        this.Point = point;
        this.Normal = normal == Vector2.Zero ? Vector2.UnitY : Vector2.Normalize(normal);
    }
}