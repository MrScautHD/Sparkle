using System.Numerics;
using Bliss.CSharp;
using Bliss.CSharp.Camera.Dim3;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Keyboards;
using Bliss.CSharp.Materials;
using Bliss.CSharp.Transformations;
using Jitter2.Collision;
using Jitter2.Collision.Shapes;
using Jitter2.Dynamics;
using Jitter2.Dynamics.Constraints;
using Jitter2.LinearMath;
using Sparkle.CSharp;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.Graphics.Rendering;
using Sparkle.CSharp.Physics.Dim3.SoftBodies;
using Sparkle.CSharp.Physics.Dim3.SoftBodies.Factories;
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
        Entity player = new Entity(new Transform() { Translation = new Vector3(0, 23, 0)} );
        RigidBody3D playerBody = new RigidBody3D(new TransformedShape(new CapsuleShape(0.5F, 2), new Vector3(0, 0.5F, 0)));
        player.AddComponent(playerBody);
        player.AddComponent(new ModelRenderer(ContentRegistry.PlayerModel, -Vector3.UnitY));
        this.AddEntity(player);
        
        // PLAYER LOCK ROTATION (Cannot fall over).
        //HingeAngle angleConstraint = playerBody.World.CreateConstraint<HingeAngle>(playerBody.Body, playerBody.World.NullBody);
        //angleConstraint.Initialize(JVector.UnitY, AngularLimit.Full);

        // SOFT CUBE
        Entity softCube = new Entity(new Transform() { Translation = new Vector3(0, 18, 0) } );
        SoftBody3D softBodyCube = new SoftBody3D(new SoftBodyCubeFactory(new Vector3(1, 1, 1)), new SoftBodyRenderInfo());
        softCube.AddComponent(softBodyCube);
        this.AddEntity(softCube);
        
        softBodyCube.Mesh.Material.SetMapTexture(MaterialMapType.Albedo.GetName(), ContentRegistry.PlayerSprite);
        
        // SOFT CLOTH
        Entity cloth = new Entity(new Transform() { Translation = new Vector3(0, 15, 0) });
        SoftBody3D softBodyCloth = new SoftBody3D(new SoftBodyClothFactory(10, 10, new Vector2(10, 10)), new SoftBodyRenderInfo() { RasterizerState = RasterizerStateDescription.CULL_NONE} );
        cloth.AddComponent(softBodyCloth);
        this.AddEntity(cloth);
        
        RigidBody fb0 = softBodyCloth.Vertices.OrderByDescending(item => +item.Position.X + item.Position.Z).First();
        var c0 = softBodyCloth.World.CreateConstraint<BallSocket>(fb0, softBodyCloth.World.NullBody);
        c0.Initialize(fb0.Position);

        RigidBody fb1 = softBodyCloth.Vertices.OrderByDescending(item => +item.Position.X - item.Position.Z).First();
        var c1 = softBodyCloth.World.CreateConstraint<BallSocket>(fb1, softBodyCloth.World.NullBody);
        c1.Initialize(fb1.Position);

        RigidBody fb2 = softBodyCloth.Vertices.OrderByDescending(item => -item.Position.X + item.Position.Z).First();
        var c2 = softBodyCloth.World.CreateConstraint<BallSocket>(fb2, softBodyCloth.World.NullBody);
        c2.Initialize(fb2.Position);

        RigidBody fb3 = softBodyCloth.Vertices.OrderByDescending(item => -item.Position.X - item.Position.Z).First();
        var c3 = softBodyCloth.World.CreateConstraint<BallSocket>(fb3, softBodyCloth.World.NullBody);
        c3.Initialize(fb3.Position);
        
        softBodyCloth.Mesh.Material.SetMapTexture(MaterialMapType.Albedo.GetName(), ContentRegistry.PlayerSprite);
        
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
        SoftBody3D softClothBody= this.GetEntity(4)!.GetComponent<SoftBody3D>()!;
        
        if (!playerBody.World.DynamicTree.RayCast(playerBody.Position - (Vector3.UnitY * 6.5F), -Vector3.UnitY, null, null, out IDynamicTreeProxy? shape, out JVector normal, out float fraction)) {
            playerBody.SetActivationState(true);
            playerBody.AddForce(new Vector3(0, 250, 0));
        }

        if (Input.IsKeyDown(KeyboardKey.H)) {
            softCubeBody.Center.SetActivationState(true);
            softCubeBody.Center.AngularVelocity = new JVector(5, 0, 0);
            //softCubeBody.Center.AddForce(new Vector3(0, 200, 0));
        }

        if (Input.IsKeyDown(KeyboardKey.J)) {
            softClothBody.Center.SetActivationState(true);
            softClothBody.Center.AddForce(new JVector(0, 14, 0));
        }
    }

    protected override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        base.Draw(context, framebuffer);
        
        // Draw gird.
        context.ImmediateRenderer.DrawGird(context.CommandList, framebuffer.OutputDescription, new Transform(), 50, 1, Color.Gray);
        
        // Prepare physics debug drawer.
        this._debugDrawer.Prepare(context.CommandList, framebuffer.OutputDescription, color: Color.Green);
        
        // TODO: ADD BACK WHEN IT REWORKED!
        // Draw physics debug renderers.
        this.GetEntity(2)!.GetComponent<RigidBody3D>()!.DebugDraw(this._debugDrawer);
        this.GetEntity(5)!.GetComponent<RigidBody3D>()!.DebugDraw(this._debugDrawer);
        this.GetEntity(3)!.GetComponent<SoftBody3D>()!.DebugDraw(this._debugDrawer);
        this.GetEntity(4)!.GetComponent<SoftBody3D>()!.DebugDraw(this._debugDrawer);
    }
}