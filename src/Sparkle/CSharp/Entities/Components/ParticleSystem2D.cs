using System.Numerics;
using System.Runtime.InteropServices;
using Bliss.CSharp.Colors;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.Graphics.Particles.Dim2;
using Sparkle.CSharp.Graphics.Particles.Dim2.Collisions;
using Sparkle.CSharp.Graphics.Particles.Dim2.Collisions.Providers;
using Veldrid;

namespace Sparkle.CSharp.Entities.Components;

public class ParticleSystem2D : InterpolatedComponent {
    
    /// <summary>
    /// The particle behavior and rendering settings used by this system.
    /// </summary>
    public ParticleDefinition2D Definition { get; private set; }
    
    /// <summary>
    /// Determines whether the particle system is currently emitting particles.
    /// </summary>
    public bool IsPlaying { get; private set; }
    
    /// <summary>
    /// The collection of currently active particles.
    /// </summary>
    private List<Particle2D> _particles;
    
    /// <summary>
    /// The random generator used for particle spawning and variation.
    /// </summary>
    private Random _random;
    
    /// <summary>
    /// The elapsed simulation time for the current emission cycle.
    /// </summary>
    private float _time;
    
    /// <summary>
    /// Accumulates fractional particle emission over time.
    /// </summary>
    private float _emissionAccumulator;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ParticleSystem2D"/> class.
    /// </summary>
    /// <param name="definition">The particle behavior and rendering settings used by this system.</param>
    /// <param name="offsetPosition">The local offset position of the particle system component.</param>
    /// <param name="isPlaying">Whether the particle system should start in the playing state.</param>
    public ParticleSystem2D(ParticleDefinition2D definition, Vector3 offsetPosition, bool isPlaying = true) : base(offsetPosition) {
        this.Definition = definition;
        this.IsPlaying = isPlaying;
        this._particles = new List<Particle2D>();
        this._random = new Random();
    }
    
    /// <summary>
    /// Starts particle emission and optionally clears existing particles.
    /// </summary>
    /// <param name="clearExisting">Whether to remove all currently active particles before playing.</param>
    public void Play(bool clearExisting = false) {
        if (clearExisting) {
            this._particles.Clear();
        }
        
        this.IsPlaying = true;
        this._time = 0.0F;
        this._emissionAccumulator = 0.0F;
    }
    
    /// <summary>
    /// Stops particle emission and optionally clears existing particles.
    /// </summary>
    /// <param name="clearExisting">Whether to remove all currently active particles after stopping.</param>
    public void Stop(bool clearExisting = false) {
        this.IsPlaying = false;
        
        if (clearExisting) {
            this._particles.Clear();
        }
    }
    
    /// <summary>
    /// Immediately emits a specific number of particles.
    /// </summary>
    /// <param name="count">The number of particles to emit.</param>
    public void Emit(int count) {
        Vector2 emitterPosition = new Vector2(this.GlobalPosition.X, this.GlobalPosition.Y);
        float emitterRotation = this.GetEmitterRotation();
        Vector2 emitterScale = new Vector2(this.Entity.GlobalTransform.Scale.X, this.Entity.GlobalTransform.Scale.Y);
        
        for (int i = 0; i < count && this._particles.Count < this.Definition.MaxParticles; i++) {
            Particle2D particle2D = this.CreateParticle(emitterPosition, emitterRotation, emitterScale);
            this._particles.Add(particle2D);
        }
    }
    
    /// <summary>
    /// Updates particle emission, simulation, collision, and lifetime state.
    /// </summary>
    /// <param name="delta">The frame time in seconds.</param>
    protected internal override void Update(double delta) {
        base.Update(delta);
        
        float dt = (float) delta * this.Definition.SimulationSpeed;
        
        if (dt <= 0.0F) {
            return;
        }
        
        if (this.IsPlaying) {
            this._time += dt;
            
            if (this._time <= this.Definition.Duration) {
                this._emissionAccumulator += this.Definition.EmissionRate * dt;
                
                // While the system is active, keep emitting particles over time.
                if (this._emissionAccumulator >= 1.0F) {
                    Vector2 emitterPosition = new Vector2(this.GlobalPosition.X, this.GlobalPosition.Y);
                    float emitterRotation = this.GetEmitterRotation();
                    Vector2 emitterScale = new Vector2(this.Entity.GlobalTransform.Scale.X, this.Entity.GlobalTransform.Scale.Y);
                    
                    while (this._emissionAccumulator >= 1.0F && this._particles.Count < this.Definition.MaxParticles) {
                        Particle2D particle2D = this.CreateParticle(emitterPosition, emitterRotation, emitterScale);
                        
                        this._particles.Add(particle2D);
                        this._emissionAccumulator -= 1.0F;
                    }
                }
            }
            else if (this.Definition.Looping) {
                this._time = 0.0F;
                this._emissionAccumulator = 0.0F;
            }
            else {
                this.IsPlaying = false;
            }
        }
        
        Span<Particle2D> particles = CollectionsMarshal.AsSpan(this._particles);
        
        // Update all living particles.
        for (int i = 0; i < particles.Length; i++) {
            ref Particle2D particle2D = ref particles[i];
            
            particle2D.PreviousPosition = particle2D.Position;
            particle2D.Age += dt;
            
            // Skip further updates if the particle3D has expired.
            if (particle2D.Age >= particle2D.Lifetime) {
                continue;
            }
            
            float life = particle2D.Age / particle2D.Lifetime;
            float speed = particle2D.Velocity.Length();
            
            Vector2 force = Vector2.Zero;
            force += this.Definition.Acceleration;
            force += this.Definition.Gravity;
            force += this.Definition.ForceOverLifetime * life;
            
            // Add procedural noise movement if enabled.
            if (this.Definition.UseNoise && this.Definition.NoiseStrength > 0.0F) {
                force += this.SampleNoise(particle2D.NoiseSeed, particle2D.Age) * this.Definition.NoiseStrength;
            }
            
            // Move the particle using velocity and forces.
            particle2D.Velocity += this.Definition.VelocityOverLifetime * life * dt;
            particle2D.Velocity += force * dt;
            particle2D.Position += particle2D.Velocity * dt;
            
            float angularVelocity = float.Lerp(this.Definition.StartAngularVelocity, this.Definition.EndAngularVelocity, life);
            angularVelocity += this.Definition.AngularVelocity;
            
            if (this.Definition.RotationBySpeed != 0.0F && speed > 0.0F) {
                angularVelocity += speed * this.Definition.RotationBySpeed;
            }
            
            particle2D.Spin += angularVelocity * dt;
            
            // Scale particle over a lifetime, optionally affected by speed.
            Vector2 baseScale = Vector2.Lerp(this.Definition.StartScale, this.Definition.EndScale, life);
            float speedScaleFactor = 1.0F + speed * this.Definition.ScaleBySpeed;
            
            if (speedScaleFactor < 0.0F) {
                speedScaleFactor = 0.0F;
            }
            
            particle2D.Scale = baseScale * speedScaleFactor;
            
            // Handle collisions.
            this.ResolveCollision(ref particle2D);
        }
        
        // Remove dead particles after the update pass.
        for (int i = this._particles.Count - 1; i >= 0; i--) {
            if (this._particles[i].Age >= this._particles[i].Lifetime) {
                this._particles.RemoveAt(i);
            }
        }
    }
    
    /// <summary>
    /// Draws all currently active particles as queued sprites.
    /// </summary>
    /// <param name="context">The graphics context used for rendering.</param>
    /// <param name="framebuffer">The target framebuffer.</param>
    protected internal override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        base.Draw(context, framebuffer);
        
        int aliveCount = this._particles.Count;
        
        if (aliveCount == 0) {
            return;
        }
        
        for (int i = 0; i < aliveCount; i++) {
            Particle2D particle2D = this._particles[i];
            float life = particle2D.Age / particle2D.Lifetime;
            
            float lifetimeRotation = float.Lerp(this.Definition.StartRotation, this.Definition.EndRotation, life);
            float baseRotation = this.Definition.SimulateInWorldSpace ? particle2D.Rotation : this.GetEmitterRotation();
            float finalRotation = baseRotation + lifetimeRotation + particle2D.Spin;
            
            Vector2 position = this.Definition.SimulateInWorldSpace ? particle2D.Position : new Vector2(this.LerpedGlobalPosition.X, this.LerpedGlobalPosition.Y) + particle2D.Position;
            Vector2 scale = this.Definition.SimulateInWorldSpace ? particle2D.Scale * particle2D.SpawnScale : particle2D.Scale * new Vector2(this.LerpedScale.X, this.LerpedScale.Y);
            
            this.Entity.Scene.SpriteRenderer.DrawSprite(
                this.Definition.Texture,
                this.Definition.Sampler,
                position,
                this.Definition.LayerDepth,
                this.Definition.SourceRect,
                scale,
                this.Definition.Origin,
                finalRotation,
                this.GetParticleColor(life),
                this.Definition.Flip,
                this.Definition.Effect,
                this.Definition.BlendState,
                this.Definition.DepthStencilState,
                this.Definition.RasterizerState,
                this.Definition.ScissorRect
            );
        }
    }
    
    /// <summary>
    /// Creates a new particle using the current emitter transform and particle definition.
    /// </summary>
    /// <param name="emitterPosition">The world-space position of the emitter.</param>
    /// <param name="emitterRotation">The world-space rotation of the emitter in radians.</param>
    /// <param name="emitterScale">The world-space scale of the emitter.</param>
    /// <returns>A newly initialized <see cref="Particle2D"/> instance.</returns>
    private Particle2D CreateParticle(Vector2 emitterPosition, float emitterRotation, Vector2 emitterScale) {
        Vector2 baseDirection;
        
        // Choose a random direction when no base direction is set, otherwise use the configured direction with spread.
        if (this.Definition.Direction == Vector2.Zero) {
            baseDirection = this.RandomUnitVector();
        }
        else {
            baseDirection = Vector2.Normalize(this.Definition.Direction + this.RandomInsideUnitCircle() * this.Definition.Spread);
        }
        
        // Randomize the particle speed and lifetime within their configured ranges.
        float speed = MathF.Max(0.0F, this.Definition.StartSpeed + this.RandomRange(-this.Definition.SpeedRandomness, this.Definition.SpeedRandomness));
        float lifetime = MathF.Max(0.01F, this.Definition.StartLifetime + this.RandomRange(-this.Definition.LifetimeRandomness, this.Definition.LifetimeRandomness));
        
        // Pick a random spawn position inside the configured spawn box.
        Vector2 spawnOffset = new Vector2(
            this.RandomRange(-this.Definition.SpawnBox.X * 0.5F, this.Definition.SpawnBox.X * 0.5F),
            this.RandomRange(-this.Definition.SpawnBox.Y * 0.5F, this.Definition.SpawnBox.Y * 0.5F)
        );
        
        float spawnRotation = this.Definition.StartRotation + this.RandomRange(-this.Definition.RotationRandomness, this.Definition.RotationRandomness);
        
        // Convert the spawn position and movement direction into world space when needed.
        if (this.Definition.SimulateInWorldSpace) {
            Vector2 worldSpawnOffset = Vector2.Transform(spawnOffset * emitterScale, Matrix3x2.CreateRotation(emitterRotation));
            Vector2 worldDirection = Vector2.TransformNormal(baseDirection, Matrix3x2.CreateRotation(emitterRotation));
            
            return new Particle2D() {
                Position = emitterPosition + worldSpawnOffset,
                PreviousPosition = emitterPosition + worldSpawnOffset,
                Velocity = worldDirection * speed,
                Rotation = emitterRotation + spawnRotation,
                Spin = 0.0F,
                Scale = this.Definition.StartScale,
                SpawnScale = emitterScale,
                Age = 0.0F,
                Lifetime = lifetime,
                NoiseSeed = this.RandomRange(0.0F, 10000.0F)
            };
        }
        
        return new Particle2D() {
            Position = spawnOffset,
            PreviousPosition = spawnOffset,
            Velocity = baseDirection * speed,
            Rotation = spawnRotation,
            Spin = 0.0F,
            Scale = this.Definition.StartScale,
            SpawnScale = Vector2.One,
            Age = 0.0F,
            Lifetime = lifetime,
            NoiseSeed = this.RandomRange(0.0F, 10000.0F)
        };
    }
    
    /// <summary>
    /// Resolves particle collisions along the particle movement segment.
    /// </summary>
    /// <param name="particle">The particle to test and update on collision.</param>
    private void ResolveCollision(ref Particle2D particle) {
        if (particle.Position == particle.PreviousPosition) {
            return;
        }
        
        IParticleCollisionProvider2D? collisionProvider = this.Definition.CollisionProvider;
        
        if (collisionProvider == null) {
            return;
        }
        
        if (!collisionProvider.TryRayCast(particle.PreviousPosition, particle.Position, out ParticleCollisionHit2D hit)) {
            return;
        }
        
        particle.Position = hit.Point + hit.Normal * this.Definition.CollisionSurfaceOffset;
        particle.Velocity = Vector2.Reflect(particle.Velocity, hit.Normal) * this.Definition.Bounciness;
        particle.Velocity *= Math.Clamp(1.0F - this.Definition.CollisionDamping, 0.0F, 1.0F);
    }
    
    /// <summary>
    /// Samples a normalized procedural noise direction for a particle at a specific age.
    /// </summary>
    /// <param name="seed">The random seed associated with the particle.</param>
    /// <param name="age">The current age of the particle in seconds.</param>
    /// <returns>A normalized noise vector, or <see cref="Vector2.Zero"/> if no valid direction can be produced.</returns>
    private Vector2 SampleNoise(float seed, float age) {
        float t = age * this.Definition.NoiseScrollSpeed * this.Definition.NoiseFrequency + seed;
        
        float x = MathF.Sin(t * 1.13F) + MathF.Cos(t * 0.73F);
        float y = MathF.Cos(t * 1.57F) + MathF.Sin(t * 0.91F);
        
        Vector2 noise = new Vector2(x, y);
        
        if (noise == Vector2.Zero) {
            return Vector2.Zero;
        }
        
        return Vector2.Normalize(noise);
    }
    
    /// <summary>
    /// Generates the particle color for the current normalized lifetime.
    /// </summary>
    /// <param name="life">Normalized particle lifetime in the range [0, 1].</param>
    /// <returns>The interpolated particle color.</returns>
    private Color GetParticleColor(float life) {
        life = Math.Clamp(life, 0.0F, 1.0F);
        
        byte r = (byte) (this.Definition.StartColor.R + (this.Definition.EndColor.R - this.Definition.StartColor.R) * life);
        byte g = (byte) (this.Definition.StartColor.G + (this.Definition.EndColor.G - this.Definition.StartColor.G) * life);
        byte b = (byte) (this.Definition.StartColor.B + (this.Definition.EndColor.B - this.Definition.StartColor.B) * life);
        byte a = (byte) (this.Definition.StartColor.A + (this.Definition.EndColor.A - this.Definition.StartColor.A) * life);
        
        return new Color(r, g, b, a);
    }
    
    /// <summary>
    /// Gets the emitter rotation around the Z axis in radians.
    /// </summary>
    /// <returns>The emitter rotation in radians.</returns>
    private float GetEmitterRotation() {
        Vector3 forward = Vector3.Transform(Vector3.UnitX, this.Entity.GlobalTransform.Rotation);
        return MathF.Atan2(forward.Y, forward.X);
    }
    
    /// <summary>
    /// Generates a random normalized direction vector.
    /// </summary>
    /// <returns>A random unit vector.</returns>
    private Vector2 RandomUnitVector() {
        Vector2 value = this.RandomInsideUnitCircle();
        
        if (value == Vector2.Zero) {
            return Vector2.UnitY;
        }
        
        return Vector2.Normalize(value);
    }
    
    /// <summary>
    /// Generates a random point inside a unit circle.
    /// </summary>
    /// <returns>A random vector located inside the unit circle.</returns>
    private Vector2 RandomInsideUnitCircle() {
        while (true) {
            Vector2 value = new Vector2(
                this.RandomRange(-1.0F, 1.0F),
                this.RandomRange(-1.0F, 1.0F)
            );
            
            if (value.LengthSquared() <= 1.0F) {
                return value;
            }
        }
    }
    
    /// <summary>
    /// Generates a random floating-point value within the specified range.
    /// </summary>
    /// <param name="min">The inclusive minimum value.</param>
    /// <param name="max">The exclusive maximum value.</param>
    /// <returns>A random floating-point value between <paramref name="min"/> and <paramref name="max"/>.</returns>
    private float RandomRange(float min, float max) {
        return min + (float) this._random.NextDouble() * (max - min);
    }
}