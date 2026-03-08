using System.Numerics;

namespace Sparkle.CSharp.Graphics.Particles;

public struct Particle {
    
    /// <summary>
    /// The current position of the particle.
    /// </summary>
    public Vector3 Position;
    
    /// <summary>
    /// The position of the particle during the previous update step.
    /// </summary>
    public Vector3 PreviousPosition;
    
    /// <summary>
    /// The current movement velocity of the particle.
    /// </summary>
    public Vector3 Velocity;
    
    /// <summary>
    /// The base rotation space used when simulating the particle.
    /// </summary>
    public Quaternion RotationSpace;
    
    /// <summary>
    /// The accumulated spin rotation of the particle, expressed as Euler angles in radians.
    /// </summary>
    public Vector3 SpinEuler;
    
    /// <summary>
    /// The current scale of the particle.
    /// </summary>
    public Vector3 Scale;
    
    /// <summary>
    /// The scale captured when the particle was spawned.
    /// </summary>
    public Vector3 SpawnScale;
    
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
    /// Initializes a new instance of the <see cref="Particle"/> struct.
    /// </summary>
    /// <param name="position">The current position of the particle.</param>
    /// <param name="previousPosition">The position of the particle during the previous update step.</param>
    /// <param name="velocity">The current movement velocity of the particle.</param>
    /// <param name="rotationSpace">The base rotation space used when simulating the particle.</param>
    /// <param name="spinEuler">The accumulated spin rotation of the particle, expressed as Euler angles in radians.</param>
    /// <param name="scale">The current scale of the particle.</param>
    /// <param name="spawnScale">The scale captured when the particle was spawned.</param>
    /// <param name="age">The current age of the particle in seconds.</param>
    /// <param name="lifetime">The total lifetime of the particle in seconds.</param>
    /// <param name="noiseSeed">A random seed value used for procedural noise sampling.</param>
    public Particle(Vector3 position, Vector3 previousPosition, Vector3 velocity, Quaternion rotationSpace, Vector3 spinEuler, Vector3 scale, Vector3 spawnScale, float age, float lifetime, float noiseSeed) {
        this.Position = position;
        this.PreviousPosition = previousPosition;
        this.Velocity = velocity;
        this.RotationSpace = rotationSpace;
        this.SpinEuler = spinEuler;
        this.Scale = scale;
        this.SpawnScale = spawnScale;
        this.Age = age;
        this.Lifetime = lifetime;
        this.NoiseSeed = noiseSeed;
    }
}