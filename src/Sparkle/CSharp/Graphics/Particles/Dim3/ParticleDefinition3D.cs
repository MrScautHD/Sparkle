using System.Numerics;
using Sparkle.CSharp.Graphics.Particles.Dim3.Collisions.Providers;

namespace Sparkle.CSharp.Graphics.Particles.Dim3;

public class ParticleDefinition3D {
    
    /// <summary>
    /// Whether particles should face the active 3D camera.
    /// </summary>
    public bool Billboard;
    
    /// <summary>
    /// Whether particles remain in world space after spawning.
    /// </summary>
    public bool SimulateInWorldSpace;
    
    /// <summary>
    /// Global multiplier applied to simulation time.
    /// </summary>
    public float SimulationSpeed;
    
    /// <summary>
    /// Whether the emitter loops after its duration ends.
    /// </summary>
    public bool Looping;
    
    /// <summary>
    /// Duration of one emission cycle in seconds.
    /// </summary>
    public float Duration;
    
    /// <summary>
    /// Number of particles emitted per second.
    /// </summary>
    public float EmissionRate;
    
    /// <summary>
    /// Maximum number of alive particles at the same time.
    /// </summary>
    public int MaxParticles;
    
    /// <summary>
    /// Base lifetime of each particle in seconds.
    /// </summary>
    public float StartLifetime;
    
    /// <summary>
    /// Random lifetime variation added and subtracted from the base lifetime.
    /// </summary>
    public float LifetimeRandomness;
    
    /// <summary>
    /// Local spawn volume around the emitter.
    /// </summary>
    public Vector3 SpawnBox;
    
    /// <summary>
    /// Preferred emission direction. If zero, emission is fully random.
    /// </summary>
    public Vector3 Direction;
    
    /// <summary>
    /// Random spread amount added to the preferred direction.
    /// </summary>
    public float Spread;
    
    /// <summary>
    /// Base starting speed of emitted particles.
    /// </summary>
    public float StartSpeed;
    
    /// <summary>
    /// Random speed variation added and subtracted from the base speed.
    /// </summary>
    public float SpeedRandomness;
    
    /// <summary>
    /// Constant acceleration applied every update step.
    /// </summary>
    public Vector3 Acceleration;
    
    /// <summary>
    /// Gravity applied every update step.
    /// </summary>
    public Vector3 Gravity;
    
    /// <summary>
    /// Extra velocity contribution over lifetime.
    /// </summary>
    public Vector3 VelocityOverLifetime;
    
    /// <summary>
    /// Extra force contribution over lifetime.
    /// </summary>
    public Vector3 ForceOverLifetime;
    
    /// <summary>
    /// Rotation at the start of life, expressed in radians as Euler angles.
    /// </summary>
    public Vector3 StartRotation;
    
    /// <summary>
    /// Rotation at the end of life, expressed in radians as Euler angles.
    /// </summary>
    public Vector3 EndRotation;
    
    /// <summary>
    /// Random rotation variation applied when particles spawn, expressed in radians as Euler angles.
    /// </summary>
    public Vector3 RotationRandomness;
    
    /// <summary>
    /// Constant angular velocity applied every frame.
    /// </summary>
    public Vector3 AngularVelocity;
    
    /// <summary>
    /// Angular velocity at the start of life.
    /// </summary>
    public Vector3 StartAngularVelocity;
    
    /// <summary>
    /// Angular velocity at the end of life.
    /// </summary>
    public Vector3 EndAngularVelocity;
    
    /// <summary>
    /// Additional angular motion based on particle speed.
    /// </summary>
    public float RotationBySpeed;
    
    /// <summary>
    /// Particle3D scale at the start of life.
    /// </summary>
    public Vector3 StartScale;
    
    /// <summary>
    /// Particle3D scale at the end of life.
    /// </summary>
    public Vector3 EndScale;
    
    /// <summary>
    /// Additional scale multiplier based on particle speed.
    /// </summary>
    public float ScaleBySpeed;
    
    /// <summary>
    /// Whether procedural noise motion is enabled.
    /// </summary>
    public bool UseNoise;
    
    /// <summary>
    /// Strength of the procedural noise motion.
    /// </summary>
    public float NoiseStrength;
    
    /// <summary>
    /// Frequency of the procedural noise sampling.
    /// </summary>
    public float NoiseFrequency;
    
    /// <summary>
    /// Scroll speed of the procedural noise over time.
    /// </summary>
    public float NoiseScrollSpeed;
    
    /// <summary>
    /// Provides collision behavior for particles (Enables collisions).
    /// </summary>
    public IParticleCollisionProvider3D? CollisionProvider;
    
    /// <summary>
    /// Energy preserved after collision.
    /// </summary>
    public float Bounciness;
    
    /// <summary>
    /// Extra damping applied after collision.
    /// </summary>
    public float CollisionDamping;
    
    /// <summary>
    /// Small offset used to push particles away from a collision surface.
    /// </summary>
    public float CollisionSurfaceOffset;
    
    /// <summary>
    /// Initializes a new particle definition3D with a required mesh and optional defaults.
    /// </summary>
    /// <param name="billboard">Whether particles should face the active 3D camera.</param>
    /// <param name="simulateInWorldSpace">Whether particles remain in world space after spawning.</param>
    /// <param name="simulationSpeed">Global multiplier applied to simulation time.</param>
    /// <param name="looping">Whether the emitter loops after its duration ends.</param>
    /// <param name="duration">Duration of one emission cycle in seconds.</param>
    /// <param name="emissionRate">Number of particles emitted per second.</param>
    /// <param name="maxParticles">Maximum number of alive particles at the same time.</param>
    /// <param name="startLifetime">Base lifetime of each particle in seconds.</param>
    /// <param name="lifetimeRandomness">Random lifetime variation added and subtracted from the base lifetime.</param>
    /// <param name="spawnBox">Local spawn volume around the emitter.</param>
    /// <param name="direction">Preferred emission direction. If zero, emission is fully random.</param>
    /// <param name="spread">Random spread amount added to the preferred direction.</param>
    /// <param name="startSpeed">Base starting speed of emitted particles.</param>
    /// <param name="speedRandomness">Random speed variation added and subtracted from the base speed.</param>
    /// <param name="acceleration">Constant acceleration applied every update step.</param>
    /// <param name="gravity">Gravity applied every update step.</param>
    /// <param name="velocityOverLifetime">Extra velocity contribution over lifetime.</param>
    /// <param name="forceOverLifetime">Extra force contribution over lifetime.</param>
    /// <param name="startRotation">Rotation at the start of life, expressed in radians as Euler angles.</param>
    /// <param name="endRotation">Rotation at the end of life, expressed in radians as Euler angles.</param>
    /// <param name="rotationRandomness">Random rotation variation applied when particles spawn, expressed in radians as Euler angles.</param>
    /// <param name="angularVelocity">Constant angular velocity applied every frame.</param>
    /// <param name="startAngularVelocity">Angular velocity at the start of life.</param>
    /// <param name="endAngularVelocity">Angular velocity at the end of life.</param>
    /// <param name="rotationBySpeed">Additional angular motion based on particle speed.</param>
    /// <param name="startScale">Particle3D scale at the start of life.</param>
    /// <param name="endScale">Particle3D scale at the end of life.</param>
    /// <param name="scaleBySpeed">Additional scale multiplier based on particle speed.</param>
    /// <param name="useNoise">Whether procedural noise motion is enabled.</param>
    /// <param name="noiseStrength">Strength of the procedural noise motion.</param>
    /// <param name="noiseFrequency">Frequency of the procedural noise sampling.</param>
    /// <param name="noiseScrollSpeed">Scroll speed of the procedural noise over time.</param>
    /// <param name="collisionProvider">Provides collision behavior for particles and enables collisions</param>
    /// <param name="bounciness">Energy preserved after collision.</param>
    /// <param name="collisionDamping">Extra damping applied after collision.</param>
    /// <param name="collisionSurfaceOffset">Small offset used to push particles away from a collision surface.</param>
    public ParticleDefinition3D(
        bool billboard = false,
        bool simulateInWorldSpace = true,
        float simulationSpeed = 1.0F,
        bool looping = true,
        float duration = 5.0F,
        float emissionRate = 20.0F,
        int maxParticles = 256,
        float startLifetime = 1.5F,
        float lifetimeRandomness = 0.25F,
        Vector3? spawnBox = null,
        Vector3? direction = null,
        float spread = 1.0F,
        float startSpeed = 2.0F,
        float speedRandomness = 0.5F,
        Vector3? acceleration = null,
        Vector3? gravity = null,
        Vector3? velocityOverLifetime = null,
        Vector3? forceOverLifetime = null,
        Vector3? startRotation = null,
        Vector3? endRotation = null,
        Vector3? rotationRandomness = null,
        Vector3? angularVelocity = null,
        Vector3? startAngularVelocity = null,
        Vector3? endAngularVelocity = null,
        float rotationBySpeed = 0.0F,
        Vector3? startScale = null,
        Vector3? endScale = null,
        float scaleBySpeed = 0.0F,
        bool useNoise = false,
        float noiseStrength = 0.0F,
        float noiseFrequency = 1.0F,
        float noiseScrollSpeed = 1.0F,
        IParticleCollisionProvider3D? collisionProvider = null,
        float bounciness = 0.5F,
        float collisionDamping = 0.1F,
        float collisionSurfaceOffset = 0.01F
    ) {
        this.Billboard = billboard;
        this.SimulateInWorldSpace = simulateInWorldSpace;
        this.SimulationSpeed = simulationSpeed;
        this.Looping = looping;
        this.Duration = duration;
        this.EmissionRate = emissionRate;
        this.MaxParticles = maxParticles;
        this.StartLifetime = startLifetime;
        this.LifetimeRandomness = lifetimeRandomness;
        this.SpawnBox = spawnBox ?? Vector3.Zero;
        this.Direction = direction ?? Vector3.UnitY;
        this.Spread = spread;
        this.StartSpeed = startSpeed;
        this.SpeedRandomness = speedRandomness;
        this.Acceleration = acceleration ?? Vector3.Zero;
        this.Gravity = gravity ?? new Vector3(0, -9.81F, 0);
        this.VelocityOverLifetime = velocityOverLifetime ?? Vector3.Zero;
        this.ForceOverLifetime = forceOverLifetime ?? Vector3.Zero;
        this.StartRotation = startRotation ?? Vector3.Zero;
        this.EndRotation = endRotation ?? Vector3.Zero;
        this.RotationRandomness = rotationRandomness ?? Vector3.Zero;
        this.AngularVelocity = angularVelocity ?? Vector3.Zero;
        this.StartAngularVelocity = startAngularVelocity ?? Vector3.Zero;
        this.EndAngularVelocity = endAngularVelocity ?? Vector3.Zero;
        this.RotationBySpeed = rotationBySpeed;
        this.StartScale = startScale ?? Vector3.One * 0.15F;
        this.EndScale = endScale ?? Vector3.Zero;
        this.ScaleBySpeed = scaleBySpeed;
        this.UseNoise = useNoise;
        this.NoiseStrength = noiseStrength;
        this.NoiseFrequency = noiseFrequency;
        this.NoiseScrollSpeed = noiseScrollSpeed;
        this.CollisionProvider = collisionProvider;
        this.Bounciness = bounciness;
        this.CollisionDamping = collisionDamping;
        this.CollisionSurfaceOffset = collisionSurfaceOffset;
    }
}