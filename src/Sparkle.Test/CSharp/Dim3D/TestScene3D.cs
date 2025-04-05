using System.Numerics;
using Bliss.CSharp;
using Bliss.CSharp.Camera.Dim3;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Keyboards;
using Bliss.CSharp.Transformations;
using Jitter2.Collision;
using Jitter2.Collision.Shapes;
using Jitter2.LinearMath;
using Sparkle.CSharp;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.Graphics.Rendering;
using Sparkle.CSharp.Scenes;
using Veldrid;

namespace Sparkle.Test.CSharp.Dim3D;

public class TestScene3D : Scene {

    private Physics3DDebugDrawer _debugDrawer;
    
    public TestScene3D(string name) : base(name, SceneType.Scene3D) { }
    
    protected override void Init() {
        base.Init();

        this._debugDrawer = new Physics3DDebugDrawer(GlobalResource.GraphicsDevice);
        
        // RELATIVE MOUSE MODE.
        Input.EnableRelativeMouseMode();
        
        // SKYBOX
        this.SkyBox = ContentRegistry.SkyBox;
        
        // CAMERA
        float aspectRatio = (float) Game.Instance!.MainWindow.GetWidth() / (float) Game.Instance.MainWindow.GetHeight();
        Camera3D camera3D = new Camera3D(new Vector3(10, 10, 10), Vector3.UnitY, aspectRatio, mode: CameraMode.Free);
        this.AddEntity(camera3D);
        
        // PLAYER
        Entity player = new Entity(new Transform() { Translation = new Vector3(0, 10, 0)} );
        player.AddComponent(new ModelRenderer(ContentRegistry.PlayerModel, -Vector3.UnitY));
        player.AddComponent(new RigidBody3D(new TransformedShape(new CapsuleShape(0.5F, 2), new Vector3(0, 0.5F, 0))));
        this.AddEntity(player);
        
        // GROUND
        Entity ground = new Entity(new Transform() { Translation = new Vector3(0, -2, 0) });
        ground.AddComponent(new RigidBody3D(new BoxShape(10, 1, 10), true, true));
        this.AddEntity(ground);
    }

    protected override void Update(double delta) {
        base.Update(delta);

        if (Input.IsKeyDown(KeyboardKey.Number1)) {
            this.SetFilterEffect(ContentRegistry.GrayScaleEffect);
        }

        if (Input.IsKeyDown(KeyboardKey.Number2)) {
            this.SetFilterEffect(null);
        }
    }

    protected override void FixedUpdate(double timeStep) {
        base.FixedUpdate(timeStep);
        
        RigidBody3D body3D = this.GetEntity(2)!.GetComponent<RigidBody3D>()!;
        
        if (!body3D.World.DynamicTree.RayCast(new Vector3(body3D.Position.X, body3D.Position.Y - 8, body3D.Position.Z), -Vector3.UnitY, null, null, out IDynamicTreeProxy? shape, out JVector normal, out float fraction)) {
            body3D.AddForce(new Vector3(0, 200, 0));
        }
    }

    protected override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        base.Draw(context, framebuffer);
        context.ImmediateRenderer.DrawGird(context.CommandList, framebuffer.OutputDescription, new Transform(), 50, 1, Color.Gray);
        
        this._debugDrawer.Prepare(context.CommandList, framebuffer.OutputDescription, color: Color.Green);
        this.GetEntity(2)!.GetComponent<RigidBody3D>()!.Body.DebugDraw(this._debugDrawer);
        this.GetEntity(3)!.GetComponent<RigidBody3D>()!.Body.DebugDraw(this._debugDrawer);
    }
}