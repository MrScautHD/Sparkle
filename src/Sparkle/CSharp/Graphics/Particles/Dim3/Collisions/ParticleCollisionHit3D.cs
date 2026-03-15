using System.Numerics;

namespace Sparkle.CSharp.Graphics.Particles.Dim3.Collisions;

public struct ParticleCollisionHit3D {
    
    /// <summary>
    /// The point in world space where the collision occurred.
    /// </summary>
    public Vector3 Point { get; private set; }
    
    /// <summary>
    /// The normalized surface normal at the collision point.
    /// </summary>
    public Vector3 Normal { get; private set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ParticleCollisionHit3D"/> struct.
    /// </summary>
    /// <param name="point">The point in world space where the collision occurred.</param>
    /// <param name="normal">The surface normal at the collision point. If zero, a default upward normal is used.</param>
    public ParticleCollisionHit3D(Vector3 point, Vector3 normal) {
        this.Point = point;
        this.Normal = normal == Vector3.Zero ? Vector3.UnitY : Vector3.Normalize(normal);
    }
}