using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Effects;
using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Sprites;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics.Particles.Dim2.Collisions.Providers;
using Veldrid;

namespace Sparkle.CSharp.Graphics.Particles.Dim2;

public class ParticleDefinition2D {
    
    /// <summary>
    /// The texture used to draw each particle.
    /// </summary>
    public Texture2D Texture { get; private set; }
    
    /// <summary>
    /// Optional sampler used for particle rendering.
    /// </summary>
    public Sampler? Sampler;
    
    /// <summary>
    /// The source rectangle used when drawing the particle texture.
    /// </summary>
    public Rectangle SourceRect;
    
    /// <summary>
    /// The sprite origin used for rotation and scaling.
    /// </summary>
    public Vector2 Origin;
    
    /// <summary>
    /// The color of particles at the start of their lifetime.
    /// </summary>
    public Color StartColor;
    
    /// <summary>
    /// The color of particles at the end of their lifetime.
    /// </summary>
    public Color EndColor;
    
    /// <summary>
    /// The layer depth used for sprite sorting during rendering.
    /// </summary>
    public float LayerDepth;
    
    /// <summary>
    /// The sprite flip mode applied when rendering particles.
    /// </summary>
    public SpriteFlip Flip;
    
    /// <summary>
    /// The optional rendering effect applied to the particles.
    /// </summary>
    public Effect? Effect;
    
    /// <summary>
    /// The optional blend state used when rendering particles.
    /// </summary>
    public BlendStateDescription? BlendState;
    
    /// <summary>
    /// The optional depth-stencil state used when rendering particles.
    /// </summary>
    public DepthStencilStateDescription? DepthStencilState;
    
    /// <summary>
    /// The optional rasterizer state used when rendering particles.
    /// </summary>
    public RasterizerStateDescription? RasterizerState;
    
    /// <summary>
    /// The optional scissor rectangle used to limit particle rendering.
    /// </summary>
    public Rectangle? ScissorRect;
    
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
    /// Local spawn area around the emitter.
    /// </summary>
    public Vector2 SpawnBox;
    
    /// <summary>
    /// Preferred emission direction. If zero, emission is fully random.
    /// </summary>
    public Vector2 Direction;
    
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
    public Vector2 Acceleration;
    
    /// <summary>
    /// Gravity applied every update step.
    /// </summary>
    public Vector2 Gravity;
    
    /// <summary>
    /// Extra velocity contribution over lifetime.
    /// </summary>
    public Vector2 VelocityOverLifetime;
    
    /// <summary>
    /// Extra force contribution over lifetime.
    /// </summary>
    public Vector2 ForceOverLifetime;
    
    /// <summary>
    /// Rotation at the start of life, expressed in euler.
    /// </summary>
    public float StartRotation;
    
    /// <summary>
    /// Rotation at the end of life, expressed in euler.
    /// </summary>
    public float EndRotation;
    
    /// <summary>
    /// Random rotation variation applied when particles spawn.
    /// </summary>
    public float RotationRandomness;
    
    /// <summary>
    /// Constant angular velocity applied every frame.
    /// </summary>
    public float AngularVelocity;
    
    /// <summary>
    /// Angular velocity at the start of life.
    /// </summary>
    public float StartAngularVelocity;
    
    /// <summary>
    /// Angular velocity at the end of life.
    /// </summary>
    public float EndAngularVelocity;
    
    /// <summary>
    /// Additional angular motion based on particle speed.
    /// </summary>
    public float RotationBySpeed;
    
    /// <summary>
    /// Particle scale at the start of life.
    /// </summary>
    public Vector2 StartScale;
    
    /// <summary>
    /// Particle scale at the end of life.
    /// </summary>
    public Vector2 EndScale;
    
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
    /// Provides collision behavior for particles and enables collisions.
    /// </summary>
    public IParticleCollisionProvider2D? CollisionProvider;
    
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
    /// Initializes a new particle definition2D with a required texture and optional defaults.
    /// </summary>
    /// <param name="texture">The texture used to draw each particle.</param>
    /// <param name="sampler">Optional sampler used for particle rendering.</param>
    /// <param name="sourceRect">The source rectangle used when drawing the particle texture.</param>
    /// <param name="origin">The sprite origin used for rotation and scaling.</param>
    /// <param name="startColor">The color of particles at the start of their lifetime.</param>
    /// <param name="endColor">The color of particles at the end of their lifetime.</param>
    /// <param name="layerDepth">The layer depth used for sprite sorting during rendering.</param>
    /// <param name="flip">The sprite flip mode applied when rendering particles.</param>
    /// <param name="effect">The optional rendering effect applied to the particles.</param>
    /// <param name="blendState">The optional blend state used when rendering particles.</param>
    /// <param name="depthStencilState">The optional depth-stencil state used when rendering particles.</param>
    /// <param name="rasterizerState">The optional rasterizer state used when rendering particles.</param>
    /// <param name="scissorRect">The optional scissor rectangle used to limit particle rendering.</param>
    /// <param name="simulateInWorldSpace">Whether particles remain in world space after spawning.</param>
    /// <param name="simulationSpeed">Global multiplier applied to simulation time.</param>
    /// <param name="looping">Whether the emitter loops after its duration ends.</param>
    /// <param name="duration">Duration of one emission cycle in seconds.</param>
    /// <param name="emissionRate">Number of particles emitted per second.</param>
    /// <param name="maxParticles">Maximum number of alive particles at the same time.</param>
    /// <param name="startLifetime">Base lifetime of each particle in seconds.</param>
    /// <param name="lifetimeRandomness">Random lifetime variation added and subtracted from the base lifetime.</param>
    /// <param name="spawnBox">Local spawn area around the emitter.</param>
    /// <param name="direction">Preferred emission direction. If zero, emission is fully random.</param>
    /// <param name="spread">Random spread amount added to the preferred direction.</param>
    /// <param name="startSpeed">Base starting speed of emitted particles.</param>
    /// <param name="speedRandomness">Random speed variation added and subtracted from the base speed.</param>
    /// <param name="acceleration">Constant acceleration applied every update step.</param>
    /// <param name="gravity">Gravity applied every update step.</param>
    /// <param name="velocityOverLifetime">Extra velocity contribution over lifetime.</param>
    /// <param name="forceOverLifetime">Extra force contribution over lifetime.</param>
    /// <param name="startRotation">Rotation at the start of life, expressed in radians.</param>
    /// <param name="endRotation">Rotation at the end of life, expressed in radians.</param>
    /// <param name="rotationRandomness">Random rotation variation applied when particles spawn.</param>
    /// <param name="angularVelocity">Constant angular velocity applied every frame.</param>
    /// <param name="startAngularVelocity">Angular velocity at the start of life.</param>
    /// <param name="endAngularVelocity">Angular velocity at the end of life.</param>
    /// <param name="rotationBySpeed">Additional angular motion based on particle speed.</param>
    /// <param name="startScale">Particle scale at the start of life.</param>
    /// <param name="endScale">Particle scale at the end of life.</param>
    /// <param name="scaleBySpeed">Additional scale multiplier based on particle speed.</param>
    /// <param name="useNoise">Whether procedural noise motion is enabled.</param>
    /// <param name="noiseStrength">Strength of the procedural noise motion.</param>
    /// <param name="noiseFrequency">Frequency of the procedural noise sampling.</param>
    /// <param name="noiseScrollSpeed">Scroll speed of the procedural noise over time.</param>
    /// <param name="collisionProvider">Provides collision behavior for particles and enables collisions.</param>
    /// <param name="bounciness">Energy preserved after collision.</param>
    /// <param name="collisionDamping">Extra damping applied after collision.</param>
    /// <param name="collisionSurfaceOffset">Small offset used to push particles away from a collision surface.</param>
    public ParticleDefinition2D(
        Texture2D texture,
        Sampler? sampler = null,
        Rectangle? sourceRect = null,
        Vector2? origin = null,
        Color? startColor = null,
        Color? endColor = null,
        float layerDepth = 0.0F,
        SpriteFlip flip = SpriteFlip.None,
        Effect? effect = null,
        BlendStateDescription? blendState = null,
        DepthStencilStateDescription? depthStencilState = null,
        RasterizerStateDescription? rasterizerState = null,
        Rectangle? scissorRect = null,
        bool simulateInWorldSpace = true,
        float simulationSpeed = 1.0F,
        bool looping = true,
        float duration = 5.0F,
        float emissionRate = 20.0F,
        int maxParticles = 256,
        float startLifetime = 1.5F,
        float lifetimeRandomness = 0.25F,
        Vector2? spawnBox = null,
        Vector2? direction = null,
        float spread = 1.0F,
        float startSpeed = 2.0F,
        float speedRandomness = 0.5F,
        Vector2? acceleration = null,
        Vector2? gravity = null,
        Vector2? velocityOverLifetime = null,
        Vector2? forceOverLifetime = null,
        float startRotation = 0.0F,
        float endRotation = 0.0F,
        float rotationRandomness = 0.0F,
        float angularVelocity = 0.0F,
        float startAngularVelocity = 0.0F,
        float endAngularVelocity = 0.0F,
        float rotationBySpeed = 0.0F,
        Vector2? startScale = null,
        Vector2? endScale = null,
        float scaleBySpeed = 0.0F,
        bool useNoise = false,
        float noiseStrength = 0.0F,
        float noiseFrequency = 1.0F,
        float noiseScrollSpeed = 1.0F,
        IParticleCollisionProvider2D? collisionProvider = null,
        float bounciness = 0.5F,
        float collisionDamping = 0.1F,
        float collisionSurfaceOffset = 0.01F
    ) {
        this.Texture = texture;
        this.Sampler = sampler;
        this.SourceRect = sourceRect ?? new Rectangle(0, 0, (int) texture.Width, (int) texture.Height);
        this.Origin = origin ?? new Vector2(this.SourceRect.Width * 0.5F, this.SourceRect.Height * 0.5F);
        this.StartColor = startColor ?? Color.White;
        this.EndColor = endColor ?? Color.White;
        this.LayerDepth = layerDepth;
        this.Flip = flip;
        this.Effect = effect;
        this.BlendState = blendState;
        this.DepthStencilState = depthStencilState;
        this.RasterizerState = rasterizerState;
        this.ScissorRect = scissorRect;
        this.SimulateInWorldSpace = simulateInWorldSpace;
        this.SimulationSpeed = simulationSpeed;
        this.Looping = looping;
        this.Duration = duration;
        this.EmissionRate = emissionRate;
        this.MaxParticles = maxParticles;
        this.StartLifetime = startLifetime;
        this.LifetimeRandomness = lifetimeRandomness;
        this.SpawnBox = spawnBox ?? Vector2.Zero;
        this.Direction = direction ?? -Vector2.UnitY;
        this.Spread = spread;
        this.StartSpeed = startSpeed;
        this.SpeedRandomness = speedRandomness;
        this.Acceleration = acceleration ?? Vector2.Zero;
        this.Gravity = gravity ?? new Vector2(0, 9.81F);
        this.VelocityOverLifetime = velocityOverLifetime ?? Vector2.Zero;
        this.ForceOverLifetime = forceOverLifetime ?? Vector2.Zero;
        this.StartRotation = startRotation;
        this.EndRotation = endRotation;
        this.RotationRandomness = rotationRandomness;
        this.AngularVelocity = angularVelocity;
        this.StartAngularVelocity = startAngularVelocity;
        this.EndAngularVelocity = endAngularVelocity;
        this.RotationBySpeed = rotationBySpeed;
        this.StartScale = startScale ?? Vector2.One * 0.15F;
        this.EndScale = endScale ?? Vector2.Zero;
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