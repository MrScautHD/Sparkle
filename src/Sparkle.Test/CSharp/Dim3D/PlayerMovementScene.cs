using System.Numerics;
using Assimp;
using Bliss.CSharp;
using Bliss.CSharp.Camera.Dim3;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Geometry;
using Bliss.CSharp.Geometry.Meshes;
using Bliss.CSharp.Graphics.Pipelines;
using Bliss.CSharp.Graphics.VertexTypes;
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
using Sparkle.CSharp.Graphics.Particles.Dim3;
using Sparkle.CSharp.Graphics.Particles.Dim3.Collisions.Providers;
using Sparkle.CSharp.Graphics.Rendering;
using Sparkle.CSharp.Physics.Dim3;
using Sparkle.CSharp.Scenes;
using Sparkle.CSharp.Terrain;
using Veldrid;
using Scene = Sparkle.CSharp.Scenes.Scene;

namespace Sparkle.Test.CSharp.Dim3D;

public class PlayerMovementScene : Scene {
    
    public Mesh<Vertex3D> RainParticleMesh { get; private set; }
    
    public SkyBox CloudySkybox { get; private set; }
    
    public PlayerMovementScene() : base("Player-Movement-Scene", SceneType.Scene3D) { }
    
    protected override void Load(ContentManager content) {
        base.Load(content);
        
        // Meshes:
        this.RainParticleMesh = Mesh<Vertex3D>.GenQuad(GlobalGraphicsAssets.GraphicsDevice, 1, 1);
        this.RainParticleMesh.Material.Effect = GlobalResource.DefaultModelEffect.GetEffectVariant(["USE_INSTANCING"]).Effect;
        this.RainParticleMesh.Material.SetMapColor(MaterialMapType.Albedo, Color.Blue);
        this.RainParticleMesh.MeshData.VertexFormat = new VertexFormat(
            Vertex3D.VertexLayout.Name,
            [
                ..Vertex3D.VertexLayout.Layouts,
                ..Vertex3D.InstanceMatrixLayout.Layouts
            ]
        );
        
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
        
        // Particle3D spreader.
        Entity particleSpreader = new Entity(new Transform() { Translation = new Vector3(5, 0.1F, 5) });
        
        ParticleDefinition3D particleDefinition = new ParticleDefinition3D() {
            Billboard = true,
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
            SimulateInWorldSpace = true
        };
        
        ParticleSystem3D particleSystem3D = new ParticleSystem3D(this.RainParticleMesh, particleDefinition, Vector3.Zero);
        particleSpreader.AddComponent(particleSystem3D);
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
                Camera3D camera = SceneManager.ActiveCam3D;
                
                // Keep the camera position direction before updating the target.
                Vector3 direction = Vector3.Normalize(camera.Position - camera.Target);
                
                // Set the camera target to the player.
                camera.Target = new Vector3(targetPosition.X, targetPosition.Y + 2.0F, targetPosition.Z);
                
                // Update the camera position based on the new target and the previous direction.
                camera.Position = camera.Target + direction * 5.0F;
            }
        }
        
        // Draw gird.
        this.ImmediateRenderer.Begin(context.CommandList, framebuffer.OutputDescription);
        this.ImmediateRenderer.DrawGrid(new Transform(), 32, 1, 16, Color.Blue);
        //context.ImmediateRenderer.DrawCube(context.CommandList, framebuffer.OutputDescription, new Transform() { Translation = new Vector3(0, -0.5F, 0)}, new Vector3(32, 1, 32), color: Color.Gray);
        this.ImmediateRenderer.End();
        
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