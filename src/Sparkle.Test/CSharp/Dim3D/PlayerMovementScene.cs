using System.Numerics;
using Bliss.CSharp;
using Bliss.CSharp.Camera.Dim3;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Geometry;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Materials;
using Bliss.CSharp.Transformations;
using Jitter2.Collision.Shapes;
using Jitter2.Dynamics;
using Sparkle.CSharp.Content;
using Sparkle.CSharp.Content.Types;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.Graphics.Particles;
using Sparkle.CSharp.Graphics.Particles.Collisions.Providers;
using Sparkle.CSharp.Graphics.Rendering;
using Sparkle.CSharp.Physics.Dim3;
using Sparkle.CSharp.Scenes;
using Veldrid;

namespace Sparkle.Test.CSharp.Dim3D;

public class PlayerMovementScene : Scene {
    
    public Mesh RainParticleMesh { get; private set; }
    
    public SkyBox CloudySkybox { get; private set; }
    
    public PlayerMovementScene() : base("Player-Movement-Scene", SceneType.Scene3D) { }
    
    protected override void Load(ContentManager content) {
        base.Load(content);
        
        // Meshes:
        this.RainParticleMesh = Mesh.GenQuad(GlobalGraphicsAssets.GraphicsDevice, 1, 1);
        this.RainParticleMesh.Material.Effect = GlobalResource.ModelInstancingEffect;
        this.RainParticleMesh.Material.SetMapColor(MaterialMapType.Albedo, Color.Blue);
        
        // Skybox's:
        this.CloudySkybox = new SkyBox(content.GraphicsDevice, content.Load(new CubemapContent("content/skybox.png"), false));
    }
    
    protected override void Init() {
        base.Init();
        
        // Relative mouse mode.
        Input.EnableRelativeMouseMode();
        
        // Set skybox.
        this.SkyBox = this.CloudySkybox;
        
        // Create camera.
        float aspectRatio = (float) GlobalGraphicsAssets.Window.GetWidth() / (float) GlobalGraphicsAssets.Window.GetHeight();
        Camera3D camera3D = new Camera3D(new Vector3(0, 6, 3), Vector3.UnitY, aspectRatio, mode: CameraMode.ThirdPerson);
        this.AddEntity(camera3D);
        
        // Create ground.
        Entity ground = new Entity(new Transform() { Translation = new Vector3(0, -0.5F, 0) });
        ground.AddComponent(new RigidBody3D(new BoxShape(32, 1, 32), true, MotionType.Static) {
            DrawDebug = true,
            DebugDrawColor = Color.Green
        });
        this.AddEntity(ground);
        
        // Create player.
        Player player = new Player(new Transform() { Translation = new Vector3(0, 4, 0) });
        this.AddEntity(player);
        
        // Particle spreader.
        Entity particleSpreader = new Entity(new Transform() { Translation = new Vector3(5, 0, 5) });
        
        ParticleDefinition particleDefinition = new ParticleDefinition() {
            Looping = true,
            Duration = 9999.0F,
            EmissionRate = 140.0F,
            MaxParticles = 600,
            StartLifetime = 1.8F,
            LifetimeRandomness = 0.35F,
            StartSpeed = 8.5F,
            SpeedRandomness = 1.5F,
            StartScale = new Vector3(0.05F, 0.12F, 0.05F),
            EndScale = new Vector3(0.02F, 0.04F, 0.02F),
            Acceleration = Vector3.Zero,
            Gravity = new Vector3(0, -12.0F, 0),
            Direction = Vector3.UnitY,
            Spread = 0.22F,
            SpawnBox = new Vector3(0.25F, 0.02F, 0.25F),
            CollisionProvider = new ParticleCollisionProvider3D((Simulation3D) this.Simulation),
            Bounciness = 0.15F,
            CollisionDamping = 0.35F,
            CollisionSurfaceOffset = 0.02F,
            SimulateInWorldSpace = true,
            Billboard = true
        };
        
        ParticleSystem particleSystem = new ParticleSystem(this.RainParticleMesh, particleDefinition, Vector3.Zero);
        particleSpreader.AddComponent(particleSystem);
        this.AddEntity(particleSpreader);
    }
    
    protected override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        
        // Track camera Target to player.
        if (this.GetEntitiesWithTag("player").FirstOrDefault() is Player player) {
            Vector3 targetPosition = player.GlobalTransform.Translation;
            
            // Fetch the ModelRenderer and use its interpolated global position for smoother tracking.
            if (player.TryGetComponent(out ModelRenderer? renderer)) {
                targetPosition = renderer.LerpedGlobalPosition;
            }
            
            if (SceneManager.ActiveCam3D != null) {
                
                // Keep the camera position direction before updating the target.
                Vector3 direction = Vector3.Normalize(SceneManager.ActiveCam3D.Position - SceneManager.ActiveCam3D.Target);
                
                // Set the camera target to the player.
                SceneManager.ActiveCam3D.Target = new Vector3(targetPosition.X, targetPosition.Y + 2.0F, targetPosition.Z);
                
                // Update the camera position based on the new target and the previous direction.
                SceneManager.ActiveCam3D.Position = SceneManager.ActiveCam3D.Target + direction * 5.0F;
            }
        }
        
        // Draw gird.
        context.ImmediateRenderer.DrawGrid(context.CommandList, framebuffer.OutputDescription, new Transform(), 32, 1, 16, Color.Blue);
        //context.ImmediateRenderer.DrawCube(context.CommandList, framebuffer.OutputDescription, new Transform() { Translation = new Vector3(0, -0.5F, 0)}, new Vector3(32, 1, 32), color: Color.Gray);
        
        // Draw the base method.
        base.Draw(context, framebuffer);
    }
    
    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        
        if (disposing) {
            this.RainParticleMesh.Dispose();
            this.CloudySkybox.Dispose();
        }
    }
}