using System.Numerics;

namespace Sparkle.CSharp.Graphics.Particles.Collisions.Providers;

public interface IParticleCollisionProvider {
    
    /// <summary>
    /// Attempts to ray cast from a start position to an end position and returns collision information when a hit occurs.
    /// </summary>
    /// <param name="start">The world-space start position of the ray.</param>
    /// <param name="end">The world-space end position of the ray.</param>
    /// <param name="hit">When this method returns <see langword="true"/>, contains information about the detected collision.</param>
    /// <returns><see langword="true"/> if the ray hits a surface; otherwise, <see langword="false"/>.</returns>
    bool TryRayCast(Vector3 start, Vector3 end, out ParticleCollisionHit hit);
}