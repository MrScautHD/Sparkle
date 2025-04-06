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
using Sparkle.CSharp.Physics.Dim3.SoftBodies.Factories;
using Sparkle.CSharp.Physics.Dim3.SoftBodies.Types;
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
        //this.SkyBox = ContentRegistry.SkyBox;
        
        // CAMERA
        float aspectRatio = (float) Game.Instance!.MainWindow.GetWidth() / (float) Game.Instance.MainWindow.GetHeight();
        Camera3D camera3D = new Camera3D(new Vector3(10, 10, 10), Vector3.UnitY, aspectRatio, mode: CameraMode.Free);
        this.AddEntity(camera3D);
        
        // PLAYER
        Entity player = new Entity(new Transform() { Translation = new Vector3(0, 10, 0)} );
        player.AddComponent(new ModelRenderer(ContentRegistry.PlayerModel, -Vector3.UnitY));
        player.AddComponent(new RigidBody3D(new TransformedShape(new CapsuleShape(0.5F, 2), new Vector3(0, 0.5F, 0))));
        this.AddEntity(player);

        // SOFT CUBE
        Entity softCube = new Entity(new Transform() { Translation = new Vector3(10, 3, 0) } );
        softCube.AddComponent(new SoftBody3D(new SoftBodyCubeFactory(new Vector3(1, 1, 1))));
        this.AddEntity(softCube);
        
        // GROUND
        Entity ground = new Entity(new Transform() { Translation = new Vector3(0, -0.5F, 0) });
        ground.AddComponent(new RigidBody3D(new BoxShape(50, 1, 50), true, true));
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
        
        RigidBody3D playerBody = this.GetEntity(2)!.GetComponent<RigidBody3D>()!;
        SoftBody3D softCubeBody = this.GetEntity(3)!.GetComponent<SoftBody3D>()!;
        
        if (!playerBody.World.DynamicTree.RayCast(playerBody.Position - (Vector3.UnitY * 3), -Vector3.UnitY, null, null, out IDynamicTreeProxy? shape, out JVector normal, out float fraction)) {
            playerBody.SetActivationState(true);
            playerBody.AddForce(new Vector3(0, 25, 0));
        }

        if (Input.IsKeyDown(KeyboardKey.H)) {
            ((SoftBodyCube) softCubeBody.SoftBody).Center.SetActivationState(true);
            ((SoftBodyCube) softCubeBody.SoftBody).Center.AddForce(new Vector3(0, 200, 0));
        }
    }

    protected override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        base.Draw(context, framebuffer);
        context.ImmediateRenderer.DrawGird(context.CommandList, framebuffer.OutputDescription, new Transform(), 50, 1, Color.Gray);
        
        this._debugDrawer.Prepare(context.CommandList, framebuffer.OutputDescription, color: Color.Green);
        this.GetEntity(2)!.GetComponent<RigidBody3D>()!.Body.DebugDraw(this._debugDrawer);
        this.GetEntity(4)!.GetComponent<RigidBody3D>()!.Body.DebugDraw(this._debugDrawer);
        ((SoftBodyCube) this.GetEntity(3)!.GetComponent<SoftBody3D>()!.SoftBody).DebugDraw(this._debugDrawer);
    }
}