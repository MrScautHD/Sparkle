using System.Numerics;
using Bliss.CSharp.Camera.Dim3;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Keyboards;
using Bliss.CSharp.Materials;
using Bliss.CSharp.Transformations;
using Jitter2.Collision.Shapes;
using Jitter2.Dynamics;
using Jitter2.Dynamics.Constraints;
using Jitter2.LinearMath;
using Sparkle.CSharp;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.GUI;
using Sparkle.CSharp.Physics.Dim3.SoftBodies;
using Sparkle.CSharp.Physics.Dim3.SoftBodies.Factories;
using Sparkle.CSharp.Scenes;
using Veldrid;

namespace Sparkle.Test.CSharp.Dim3D;

public class TestScene3D : Scene {
    
    public TestScene3D(string name) : base(name, SceneType.Scene3D) { }
    
    protected override void Init() {
        base.Init();
        
        // RELATIVE MOUSE MODE.
        //Input.EnableRelativeMouseMode();
        
        // SKYBOX
        //this.SkyBox = ContentRegistry.SkyBox;
        
        // CAMERA
        float aspectRatio = (float) GlobalGraphicsAssets.Window.GetWidth() / (float) GlobalGraphicsAssets.Window.GetHeight();
        Camera3D camera3D = new Camera3D(new Vector3(10, 10, 10), Vector3.UnitY, aspectRatio, mode: CameraMode.Free);
        this.AddEntity(camera3D);
        
        // PLAYER
        Entity player = new Entity(new Transform() { Translation = new Vector3(2, 25, 0)} );
        RigidBody3D playerBody = new RigidBody3D(new TransformedShape(new CapsuleShape(0.5F, 2), new Vector3(0, 0.5F, 0)));
        player.AddComponent(playerBody);
        player.AddComponent(new ModelRenderer(ContentRegistry.PlayerModel, -Vector3.UnitY));
        this.AddEntity(player);
        
        // PLAYER LOCK ROTATION (Cannot fall over).
        HingeAngle angleConstraint = playerBody.World.CreateConstraint<HingeAngle>(playerBody.Body, playerBody.World.NullBody);
        angleConstraint.Initialize(JVector.UnitY, AngularLimit.Full);

        // SOFT CUBE
        Entity softCube = new Entity(new Transform() { Translation = new Vector3(3, 19, 0) });
        SoftBody3D softBodyCube = new SoftBody3D(new SoftBodyCubeFactory(new Vector3(1, 1, 1)), new SoftBodyRenderInfo());
        softCube.AddComponent(softBodyCube);
        this.AddEntity(softCube);
        
        softBodyCube.Mesh.Material.SetMapTexture(MaterialMapType.Albedo.GetName(), ContentRegistry.Sprite);
        
        // SOFT CLOTH
        Entity cloth = new Entity(new Transform() { Translation = new Vector3(0, 15, 0) });
        SoftBody3D softBodyCloth = new SoftBody3D(new SoftBodyClothFactory(10, 10, new Vector2(10, 10)), new SoftBodyRenderInfo() { RasterizerState = RasterizerStateDescription.CULL_NONE });
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
        
        softBodyCloth.Mesh.Material.SetMapTexture(MaterialMapType.Albedo.GetName(), ContentRegistry.Sprite);
        
        // SOFT SPHERE
        Entity sphere = new Entity(new Transform() { Translation = new Vector3(0, 1, 0) });
        SoftBody3D softBodySphere = new SoftBody3D(new SoftBodySphereFactory(new Vector3(1, 1, 1), subdivisions: 3), new SoftBodyRenderInfo());
        sphere.AddComponent(softBodySphere);
        this.AddEntity(sphere);

        softBodySphere.Mesh.Material.SetMapTexture(MaterialMapType.Albedo.GetName(), ContentRegistry.Sprite);
        
        // GROUND
        Entity ground = new Entity(new Transform() { Translation = new Vector3(0, -0.5F, 0) });
        ground.AddComponent(new RigidBody3D(new BoxShape(96, 1, 96), true, true));
        this.AddEntity(ground);
    }

    protected override void Update(double delta) {
        base.Update(delta);
        
        if (Input.IsKeyDown(KeyboardKey.Number1)) {
            this.FilterEffect = GlobalGraphicsAssets.GrayScaleEffect;
        }
        
        if (Input.IsKeyDown(KeyboardKey.Number2)) {
            this.FilterEffect = GlobalGraphicsAssets.BloomEffect;
        }
        
        if (Input.IsKeyDown(KeyboardKey.Number3)) {
            this.FilterEffect = GlobalGraphicsAssets.BlurEffect;
        }
        
        if (Input.IsKeyDown(KeyboardKey.Number4)) {
            this.FilterEffect = GlobalGraphicsAssets.PixelizerEffect;
        }
        
        if (Input.IsKeyDown(KeyboardKey.Number5)) {
            this.FilterEffect = GlobalGraphicsAssets.PosterizationEffect;
        }
        
        if (Input.IsKeyDown(KeyboardKey.Number6)) {
            this.FilterEffect = GlobalGraphicsAssets.PredatorEffect;
        }
        
        if (Input.IsKeyDown(KeyboardKey.Number7)) {
            this.FilterEffect = GlobalGraphicsAssets.SobelEffect;
        }
        
        if (Input.IsKeyDown(KeyboardKey.Q)) {
            this.FilterEffect = null;
        }
        
        // Apply MSAA.
        if (Input.IsKeyPressed(KeyboardKey.Number8)) {
            SceneManager.PostEffect = null;
            Game.Instance?.SetSampleCount(TextureSampleCount.Count8);
        }
        
        // Apply FXAA.
        if (Input.IsKeyPressed(KeyboardKey.Number9)) {
            Game.Instance?.SetSampleCount(TextureSampleCount.Count1);
            SceneManager.PostEffect = GlobalGraphicsAssets.FxaaEffect;
            
            // Set parameters.
            GlobalGraphicsAssets.FxaaEffect.ReduceMin = 1.0F / 256.0F;
            GlobalGraphicsAssets.FxaaEffect.ReduceMul = 1.0F / 4.0F;
            GlobalGraphicsAssets.FxaaEffect.SpanMax = 12.0F;
        }
        
        // Apply NONE.
        if (Input.IsKeyPressed(KeyboardKey.Number0)) {
            Game.Instance?.SetSampleCount(TextureSampleCount.Count1);
            SceneManager.PostEffect = null;
        }

        if (Input.IsKeyPressed(KeyboardKey.N)) {
            GuiManager.SetGui(new TestGui("Test"));
        }

        if (Input.IsKeyPressed(KeyboardKey.M)) {
            GuiManager.SetGui(null);
        }
    }

    protected override void FixedUpdate(double timeStep) {
        base.FixedUpdate(timeStep);
        
        RigidBody3D playerBody = this.GetEntity(2)!.GetComponent<RigidBody3D>()!;
        SoftBody3D softCubeBody = this.GetEntity(3)!.GetComponent<SoftBody3D>()!;
        SoftBody3D softClothBody= this.GetEntity(4)!.GetComponent<SoftBody3D>()!;
        SoftBody3D softSphereBody= this.GetEntity(5)!.GetComponent<SoftBody3D>()!;
        
        //if (!playerBody.World.DynamicTree.RayCast(playerBody.Position - (Vector3.UnitY * 6.5F), -Vector3.UnitY, null, null, out IDynamicTreeProxy? shape, out JVector normal, out float fraction)) {
        //    playerBody.SetActivationState(true);
        //    playerBody.AddForce(new Vector3(0, 250, 0));
        //}

        if (Input.IsKeyDown(KeyboardKey.G)) {
            this.GetEntity(2)!.Transform.Rotation *= Quaternion.CreateFromYawPitchRoll(float.DegreesToRadians(2), 0, 0);
        }
        
        if (Input.IsKeyDown(KeyboardKey.R)) {
            this.GetEntity(2)!.Transform.Translation += Vector3.UnitY;
        }

        if (Input.IsKeyDown(KeyboardKey.I)) {
            playerBody.SetActivationState(true);
            playerBody.AddForce(new Vector3(0, 100, 0));
        }

        if (Input.IsKeyDown(KeyboardKey.H)) {
            softCubeBody.Center.SetActivationState(true);
            softCubeBody.Center.AngularVelocity = new JVector(5, 0, 0);
            //softCubeBody.Center.AddForce(new Vector3(0, 200, 0));
        }

        if (Input.IsKeyDown(KeyboardKey.J)) {
            softClothBody.Center.SetActivationState(true);
            softClothBody.Center.AddForce(new JVector(0, 100, 0));
        }
        
        if (Input.IsKeyDown(KeyboardKey.T)) {
            softSphereBody.Center.SetActivationState(true);
            softSphereBody.Center.AngularVelocity = new JVector(5, 0, 0);
        }
    }

    protected override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        base.Draw(context, framebuffer);
        
        // Draw gird.
        context.ImmediateRenderer.DrawGrid(context.CommandList, framebuffer.OutputDescription, new Transform(), 96, 1, 16, Color.Gray);
        
        // Draw physics 3D debug drawer.
        context.Physics3DDebugDrawer.Begin(context.CommandList, framebuffer.OutputDescription, color: Color.Green);
        this.GetEntity(2)!.GetComponent<RigidBody3D>()!.DebugDraw(context.Physics3DDebugDrawer);
        this.GetEntity(3)!.GetComponent<SoftBody3D>()!.DebugDraw(context.Physics3DDebugDrawer);
        this.GetEntity(4)!.GetComponent<SoftBody3D>()!.DebugDraw(context.Physics3DDebugDrawer);
        this.GetEntity(5)!.GetComponent<SoftBody3D>()!.DebugDraw(context.Physics3DDebugDrawer);
        this.GetEntity(6)!.GetComponent<RigidBody3D>()!.DebugDraw(context.Physics3DDebugDrawer);
        context.Physics3DDebugDrawer.End();
    }
}