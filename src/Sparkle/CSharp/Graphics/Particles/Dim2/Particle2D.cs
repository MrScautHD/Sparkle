using System.Numerics;

namespace Sparkle.CSharp.Graphics.Particles.Dim2;

public struct Particle2D {
    
    /// <summary>
    /// The current position of the particle.
    /// </summary>
    public Vector2 Position;
    
    /// <summary>
    /// The position of the particle during the previous update step.
    /// </summary>
    public Vector2 PreviousPosition;
    
    /// <summary>
    /// The current movement velocity of the particle.
    /// </summary>
    public Vector2 Velocity;
    
    /// <summary>
    /// The base rotation of the particle in radians.
    /// </summary>
    public float Rotation;
    
    /// <summary>
    /// The accumulated spin rotation of the particle in radians.
    /// </summary>
    public float Spin;
    
    /// <summary>
    /// The current scale of the particle.
    /// </summary>
    public Vector2 Scale;
    
    /// <summary>
    /// The scale captured when the particle was spawned.
    /// </summary>
    public Vector2 SpawnScale;
    
    /// <summary>
    /// The current age of the particle in seconds.
    /// </summary>
    public float Age;
    
    /// <summary>
    /// The total lifetime of the particle in seconds.
    /// </summary>
    public float Lifetime;
    
    /// <summary>
    /// A random seed value used for procedural noise sampling.
    /// </summary>
    public float NoiseSeed;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Particle2D"/> struct.
    /// </summary>
    /// <param name="position">The current position of the particle.</param>
    /// <param name="previousPosition">The position of the particle during the previous update step.</param>
    /// <param name="velocity">The current movement velocity of the particle.</param>
    /// <param name="rotation">The base rotation of the particle in radians.</param>
    /// <param name="spin">The accumulated spin rotation of the particle in radians.</param>
    /// <param name="scale">The current scale of the particle.</param>
    /// <param name="spawnScale">The scale captured when the particle was spawned.</param>
    /// <param name="age">The current age of the particle in seconds.</param>
    /// <param name="lifetime">The total lifetime of the particle in seconds.</param>
    /// <param name="noiseSeed">A random seed value used for procedural noise sampling.</param>
    public Particle2D(Vector2 position, Vector2 previousPosition, Vector2 velocity, float rotation, float spin, Vector2 scale, Vector2 spawnScale, float age, float lifetime, float noiseSeed) {
        this.Position = position;
        this.PreviousPosition = previousPosition;
        this.Velocity = velocity;
        this.Rotation = rotation;
        this.Spin = spin;
        this.Scale = scale;
        this.SpawnScale = spawnScale;
        this.Age = age;
        this.Lifetime = lifetime;
        this.NoiseSeed = noiseSeed;
    }
}