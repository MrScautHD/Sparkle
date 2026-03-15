using System.Numerics;
using System.Runtime.InteropServices;
using Bliss.CSharp.Geometry;
using Bliss.CSharp.Graphics.Rendering.Renderers.Forward;
using Bliss.CSharp.Materials;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.Graphics.Particles.Dim3;
using Sparkle.CSharp.Graphics.Particles.Dim3.Collisions;
using Sparkle.CSharp.Graphics.Particles.Dim3.Collisions.Providers;
using Sparkle.CSharp.Scenes;
using Veldrid;

namespace Sparkle.CSharp.Entities.Components;

public class ParticleSystem3D : InterpolatedComponent {
    
    /// <summary>
    /// The mesh used to render each particle instance.
    /// </summary>
    public Mesh Mesh { get; private set; }
    
    /// <summary>
    /// The material used by the particle system renderable.
    /// </summary>
    public ref Material Material => ref this._renderable.Material;

    /// <summary>
    /// An array of transformation matrices representing bone animations for the particle instances.
    /// </summary>
    public Matrix4x4[]? BoneMatrics => this._renderable.BoneMatrices;
    
    /// <summary>
    /// The particle behavior and rendering settings used by this system.
    /// </summary>
    public ParticleDefinition3D Definition { get; private set; }
    
    /// <summary>
    /// Determines whether the particle system is currently emitting particles.
    /// </summary>
    public bool IsPlaying { get; private set; }
    
    /// <summary>
    /// The internal renderable used to draw particle instances.
    /// </summary>
    private Renderable _renderable;
    
    /// <summary>
    /// The collection of currently active particles.
    /// </summary>
    private List<Particle3D> _particles;
    
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
    /// Initializes a new instance of the <see cref="ParticleSystem3D"/> class using either the mesh material or a cloned copy.
    /// </summary>
    /// <param name="mesh">The mesh used to render each particle instance.</param>
    /// <param name="definition">The particle behavior and rendering settings used by this system.</param>
    /// <param name="offsetPosition">The local offset position of the particle system component.</param>
    /// <param name="copyMeshMaterial">Whether to clone the mesh material instead of using the original instance.</param>
    /// <param name="isPlaying">Whether the particle system should start in the playing state.</param>
    public ParticleSystem3D(Mesh mesh, ParticleDefinition3D definition, Vector3 offsetPosition, bool copyMeshMaterial = false, bool isPlaying = true) : this(mesh, definition, offsetPosition, copyMeshMaterial ? (Material) mesh.Material.Clone() : mesh.Material, isPlaying) { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ParticleSystem3D"/> class.
    /// </summary>
    /// <param name="mesh">The mesh used to render each particle instance.</param>
    /// <param name="definition">The particle behavior and rendering settings used by this system.</param>
    /// <param name="offsetPosition">The local offset position of the particle system component.</param>
    /// <param name="material">The material used to render the particle instances.</param>
    /// <param name="isPlaying">Whether the particle system should start in the playing state.</param>
    public ParticleSystem3D(Mesh mesh, ParticleDefinition3D definition, Vector3 offsetPosition, Material material, bool isPlaying = true) : base(offsetPosition) {
        this.Mesh = mesh;
        this.Definition = definition;
        this.IsPlaying = isPlaying;
        this._renderable = new Renderable(mesh, new Transform(), material);
        this._particles = new List<Particle3D>();
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
        Vector3 emitterPosition = this.GlobalPosition;
        Quaternion emitterRotation = this.Entity.GlobalTransform.Rotation;
        Vector3 emitterScale = this.Entity.GlobalTransform.Scale;
        
        for (int i = 0; i < count && this._particles.Count < this.Definition.MaxParticles; i++) {
            Particle3D particle = this.CreateParticle(emitterPosition, emitterRotation, emitterScale);
            this._particles.Add(particle);
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
                    Vector3 emitterPosition = this.GlobalPosition;
                    Quaternion emitterRotation = this.Entity.GlobalTransform.Rotation;
                    Vector3 emitterScale = this.Entity.GlobalTransform.Scale;
                    
                    while (this._emissionAccumulator >= 1.0F && this._particles.Count < this.Definition.MaxParticles) {
                        Particle3D particle = this.CreateParticle(emitterPosition, emitterRotation, emitterScale);
                        
                        this._particles.Add(particle);
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
        
        Span<Particle3D> particles = CollectionsMarshal.AsSpan(this._particles);
        
        // Update all living particles.
        for (int i = 0; i < particles.Length; i++) {
            ref Particle3D particle = ref particles[i];
            
            particle.PreviousPosition = particle.Position;
            particle.Age += dt;
            
            // Skip further updates if the particle has expired.
            if (particle.Age >= particle.Lifetime) {
                continue;
            }
            
            float life = particle.Age / particle.Lifetime;
            float speed = particle.Velocity.Length();
            
            Vector3 force = Vector3.Zero;
            force += this.Definition.Acceleration;
            force += this.Definition.Gravity;
            force += this.Definition.ForceOverLifetime * life;
            
            // Add procedural noise movement if enabled.
            if (this.Definition.UseNoise && this.Definition.NoiseStrength > 0.0F) {
                force += this.SampleNoise(particle.NoiseSeed, particle.Age) * this.Definition.NoiseStrength;
            }
            
            // Move the particle using velocity and forces.
            particle.Velocity += this.Definition.VelocityOverLifetime * life * dt;
            particle.Velocity += force * dt;
            particle.Position += particle.Velocity * dt;
            
            Vector3 angularVelocity = Vector3.Lerp(this.Definition.StartAngularVelocity, this.Definition.EndAngularVelocity, life);
            angularVelocity += this.Definition.AngularVelocity;
            
            if (this.Definition.RotationBySpeed != 0.0F && speed > 0.0F) {
                angularVelocity += Vector3.One * (speed * this.Definition.RotationBySpeed);
            }
            
            particle.Spin += angularVelocity * dt;
            
            // Scale particle over a lifetime, optionally affected by speed.
            Vector3 baseScale = Vector3.Lerp(this.Definition.StartScale, this.Definition.EndScale, life);
            float speedScaleFactor = 1.0F + speed * this.Definition.ScaleBySpeed;
            
            if (speedScaleFactor < 0.0F) {
                speedScaleFactor = 0.0F;
            }
            
            particle.Scale = baseScale * speedScaleFactor;
            
            // Handle collisions.
            this.ResolveCollision(ref particle);
        }
        
        // Remove dead particles after the update pass.
        for (int i = this._particles.Count - 1; i >= 0; i--) {
            if (this._particles[i].Age >= this._particles[i].Lifetime) {
                this._particles.RemoveAt(i);
            }
        }
    }
    
    /// <summary>
    /// Draws all currently active particles as instanced renderables.
    /// </summary>
    /// <param name="context">The graphics context used for rendering.</param>
    /// <param name="framebuffer">The target framebuffer.</param>
    protected internal override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        base.Draw(context, framebuffer);
        Camera3D? cam3D = SceneManager.ActiveCam3D;
        
        if (cam3D == null) {
            return;
        }
        
        int aliveCount = this._particles.Count;
        
        if (aliveCount == 0) {
            return;
        }
        
        // Make sure the instance array matches the particle count.
        this.AdjustInstanceStorage();
        
        for (int i = 0; i < aliveCount; i++) {
            Particle3D particle = this._particles[i];
            float life = particle.Age / particle.Lifetime;
            
            // Interpolate the configured rotation over the particle lifetime.
            float lerpedRotYaw = float.Lerp(this.Definition.StartRotation.Y, this.Definition.EndRotation.Y, life);
            float lerpedRotPitch = float.Lerp(this.Definition.StartRotation.X, this.Definition.EndRotation.X, life);
            float lerpedRotRoll = float.Lerp(this.Definition.StartRotation.Z, this.Definition.EndRotation.Z, life);
            
            Quaternion randomRotation = Quaternion.CreateFromYawPitchRoll(particle.RotationOffset.Y, particle.RotationOffset.X, particle.RotationOffset.Z);
            Quaternion lifetimeRotation = Quaternion.CreateFromYawPitchRoll(lerpedRotYaw, lerpedRotPitch, lerpedRotRoll);
            Quaternion spinRotation = Quaternion.CreateFromYawPitchRoll(particle.Spin.Y, particle.Spin.X, particle.Spin.Z);
            
            Quaternion baseRotation;
            
            if (this.Definition.Billboard && SceneManager.ActiveCam3D != null) {
                Camera3D camera = SceneManager.ActiveCam3D;
                
                Vector3 forward = Vector3.Normalize(camera.GlobalTransform.Translation - (camera.GlobalTransform.Translation + camera.GetForward()));
                Vector3 up = Vector3.Transform(Vector3.UnitY, camera.GlobalTransform.Rotation);
                Vector3 right = Vector3.Normalize(Vector3.Cross(up, forward));
                
                up = Vector3.Normalize(Vector3.Cross(forward, right));
                
                Matrix4x4 rotationMatrix = Matrix4x4.Identity with {
                    
                    // Right axis.
                    M11 = right.X,
                    M12 = right.Y,
                    M13 = right.Z,
                    
                    // Up axis.
                    M21 = up.X,
                    M22 = up.Y,
                    M23 = up.Z,
                    
                    // Forward axis.
                    M31 = forward.X,
                    M32 = forward.Y,
                    M33 = forward.Z
                };
                
                baseRotation = Quaternion.Normalize(Quaternion.CreateFromRotationMatrix(rotationMatrix) * Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.PI));
            }
            else {
                baseRotation = this.Definition.SimulateInWorldSpace ? particle.RotationSpace : this.LerpedRotation;
            }
            
            this._renderable.Transforms[i] = new Transform() {
                Translation = this.Definition.SimulateInWorldSpace ? particle.Position : this.LerpedGlobalPosition + particle.Position,
                Rotation = Quaternion.Normalize(baseRotation * randomRotation * lifetimeRotation * spinRotation),
                Scale = this.Definition.SimulateInWorldSpace ? particle.Scale * particle.SpawnScale : particle.Scale * this.LerpedScale
            };
        }
        
        this.Entity.Scene.Renderer.DrawRenderable(this._renderable);
    }
    
    /// <summary>
    /// Creates a new particle using the current emitter transform and particle definition3D.
    /// </summary>
    /// <param name="emitterPosition">The world-space position of the emitter.</param>
    /// <param name="emitterRotation">The world-space rotation of the emitter.</param>
    /// <param name="emitterScale">The world-space scale of the emitter.</param>
    /// <returns>A newly initialized <see cref="Particle3D"/> instance.</returns>
    private Particle3D CreateParticle(Vector3 emitterPosition, Quaternion emitterRotation, Vector3 emitterScale) {
        Vector3 baseDirection;
        
        // Choose a random direction when no base direction is set, otherwise use the configured direction with spread.
        if (this.Definition.Direction == Vector3.Zero) {
            baseDirection = this.RandomUnitVector();
        }
        else {
            baseDirection = Vector3.Normalize(this.Definition.Direction + this.RandomInsideUnitSphere() * this.Definition.Spread);
        }    
        
        // Randomize the particle speed and lifetime within their configured ranges.
        float speed = MathF.Max(0.0F, this.Definition.StartSpeed + this.RandomRange(-this.Definition.SpeedRandomness, this.Definition.SpeedRandomness));
        float lifetime = MathF.Max(0.01F, this.Definition.StartLifetime + this.RandomRange(-this.Definition.LifetimeRandomness, this.Definition.LifetimeRandomness));
        
        // Pick a random spawn position inside the configured spawn box.
        Vector3 spawnOffset = new Vector3() {
            X = this.RandomRange(-this.Definition.SpawnBox.X * 0.5F, this.Definition.SpawnBox.X * 0.5F),
            Y = this.RandomRange(-this.Definition.SpawnBox.Y * 0.5F, this.Definition.SpawnBox.Y * 0.5F),
            Z = this.RandomRange(-this.Definition.SpawnBox.Z * 0.5F, this.Definition.SpawnBox.Z * 0.5F)
        };
        
        // Pick a random spawn rotation.
        Vector3 rotationOffset = new Vector3(
            this.RandomRange(-this.Definition.RotationRandomness.X, this.Definition.RotationRandomness.X),
            this.RandomRange(-this.Definition.RotationRandomness.Y, this.Definition.RotationRandomness.Y),
            this.RandomRange(-this.Definition.RotationRandomness.Z, this.Definition.RotationRandomness.Z)
        );
        
        // Convert the spawn position and movement direction into world space when needed.
        if (this.Definition.SimulateInWorldSpace) {
            Vector3 worldSpawnOffset = Vector3.Transform(spawnOffset * emitterScale, emitterRotation);
            Vector3 worldDirection = Vector3.Transform(baseDirection, emitterRotation);
            
            return new Particle3D() {
                Position = emitterPosition + worldSpawnOffset,
                PreviousPosition = emitterPosition + worldSpawnOffset,
                Velocity = worldDirection * speed,
                RotationSpace = emitterRotation,
                RotationOffset = rotationOffset,
                Spin = Vector3.Zero,
                Scale = this.Definition.StartScale,
                SpawnScale = emitterScale,
                Age = 0.0F,
                Lifetime = lifetime,
                NoiseSeed = this.RandomRange(0.0F, 10000.0F)
            };
        }
        
        // Otherwise keep the particle data in local emitter space.
        return new Particle3D() {
            Position = spawnOffset,
            PreviousPosition = spawnOffset,
            Velocity = baseDirection * speed,
            RotationSpace = Quaternion.Identity,
            RotationOffset = rotationOffset,
            Spin = Vector3.Zero,
            Scale = this.Definition.StartScale,
            SpawnScale = Vector3.One,
            Age = 0.0F,
            Lifetime = lifetime,
            NoiseSeed = this.RandomRange(0.0F, 10000.0F)
        };
    }
    
    /// <summary>
    /// Resolves particle collisions along the particle movement segment.
    /// </summary>
    /// <param name="particle">The particle to test and update on collision.</param>
    private void ResolveCollision(ref Particle3D particle) {
        if (particle.Position == particle.PreviousPosition) {
            return;
        }
        
        IParticleCollisionProvider3D? collisionProvider = this.Definition.CollisionProvider;
        
        if (collisionProvider == null) {
            return;
        }
        
        if (!collisionProvider.TryRayCast(particle.PreviousPosition, particle.Position, out ParticleCollisionHit3D hit)) {
            return;
        }
        
        particle.Position = hit.Point + hit.Normal * this.Definition.CollisionSurfaceOffset;
        particle.Velocity = Vector3.Reflect(particle.Velocity, hit.Normal) * this.Definition.Bounciness;
        particle.Velocity *= Math.Clamp(1.0F - this.Definition.CollisionDamping, 0.0F, 1.0F);
    }
    
    /// <summary>
    /// Samples a normalized procedural noise direction for a particle at a specific age.
    /// </summary>
    /// <param name="seed">The random seed associated with the particle.</param>
    /// <param name="age">The current age of the particle in seconds.</param>
    /// <returns>A normalized noise vector, or <see cref="Vector3.Zero"/> if no valid direction can be produced.</returns>
    private Vector3 SampleNoise(float seed, float age) {
        float t = age * this.Definition.NoiseScrollSpeed * this.Definition.NoiseFrequency + seed;
        
        float x = MathF.Sin(t * 1.13F) + MathF.Cos(t * 0.73F);
        float y = MathF.Cos(t * 1.57F) + MathF.Sin(t * 0.91F);
        float z = MathF.Sin(t * 1.91F + 2.17F) + MathF.Cos(t * 0.67F + 1.37F);
        
        Vector3 noise = new Vector3(x, y, z);
        
        // Avoid normalizing a zero vector.
        if (noise == Vector3.Zero) {
            return Vector3.Zero;
        }
        
        return Vector3.Normalize(noise);
    }
    
    /// <summary>
    /// Ensures the renderable instance storage matches the current particle count.
    /// </summary>
    private void AdjustInstanceStorage() {
        int requiredCount = this._particles.Count;
        
        if (this._renderable.Transforms.Length != requiredCount) {
            Array.Resize(ref this._renderable.Transforms, requiredCount);
        }
    }
    
    /// <summary>
    /// Generates a random normalized direction vector.
    /// </summary>
    /// <returns>A random unit vector.</returns>
    private Vector3 RandomUnitVector() {
        Vector3 value = this.RandomInsideUnitSphere();
        
        if (value == Vector3.Zero) {
            return Vector3.UnitY;
        }
        
        return Vector3.Normalize(value);
    }
    
    /// <summary>
    /// Generates a random point inside a unit sphere.
    /// </summary>
    /// <returns>A random vector located inside the unit sphere.</returns>
    private Vector3 RandomInsideUnitSphere() {
        while (true) {
            Vector3 value = new Vector3(
                this.RandomRange(-1.0F, 1.0F),
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
    
    protected override void Dispose(bool disposing) { }
}