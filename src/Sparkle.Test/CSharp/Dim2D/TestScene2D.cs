using System.Numerics;
using Bliss.CSharp.Camera.Dim2;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Keyboards;
using Bliss.CSharp.Transformations;
using Box2D;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.Physics.Dim2;
using Sparkle.CSharp.Physics.Dim2.Def;
using Sparkle.CSharp.Physics.Dim2.Shapes;
using Sparkle.CSharp.Scenes;
using Veldrid;
using Transform = Bliss.CSharp.Transformations.Transform;

namespace Sparkle.Test.CSharp.Dim2D;

public class TestScene2D : Scene {
    
    public TestScene2D(string name) : base(name, SceneType.Scene2D, null, () => new Simulation2D(new PhysicsSettings2D() { WorldDef = new WorldDef() { Gravity = new Vector2(0, 9.81F) }})) { }
    
    protected override void Init() {
        base.Init();
        
        // CAMERA
        Rectangle size = new Rectangle(0, 0, GlobalGraphicsAssets.Window.GetWidth(), GlobalGraphicsAssets.Window.GetHeight());
        Camera2D camera2D = new Camera2D(Vector2.Zero, Vector2.Zero, size, CameraFollowMode.FollowTargetSmooth, zoom: 7);
        this.AddEntity(camera2D);
        
        // PLAYER
        Entity player = new Entity(new Transform() { Translation = new Vector3(0, -32, 0) });
        player.AddComponent(new Sprite(ContentRegistry.Sprite, Vector2.Zero));
        player.AddComponent(new RigidBody2D(new BodyDefinition() { Type = BodyType.Dynamic }, new PolygonShape2D(Polygon.MakeBox(8, 8), new ShapeDef())));
        this.AddEntity(player);
        
        // GROUND
        Entity ground = new Entity(new Transform() { Translation = Vector3.Zero });
        ground.AddComponent(new Sprite(ContentRegistry.Sprite, Vector2.Zero));
        ground.AddComponent(new RigidBody2D(new BodyDefinition() { Type = BodyType.Static }, new PolygonShape2D(Polygon.MakeBox(8, 8), new ShapeDef())));
        this.AddEntity(ground);
        
        // ENV ELEMENTS
        for (int i = 0; i < 6; i++) {
            Entity element = new Entity(new Transform() { Translation = new Vector3(32 + (16 * i), -48, 0) });
            element.AddComponent(new Sprite(ContentRegistry.Sprite, Vector2.Zero));
            element.AddComponent(new RigidBody2D(new BodyDefinition() { Type = BodyType.Static }, new PolygonShape2D(Polygon.MakeBox(8, 8), new ShapeDef())));
            this.AddEntity(element);
        }
        
        // ENV ELEMENTS STAIRS
        for (int i = 0; i < 5; i++) {
            Entity element = new Entity(new Transform() { Translation = new Vector3(128 + (16 * i), -48 - (16 * i), 0) });
            element.AddComponent(new Sprite(ContentRegistry.Sprite, Vector2.Zero));
            element.AddComponent(new RigidBody2D(new BodyDefinition() { Type = BodyType.Static }, new PolygonShape2D(Polygon.MakeBox(8, 8), new ShapeDef())));
            this.AddEntity(element);
        }
    }
    
    protected override void FixedUpdate(double delta) {
        base.FixedUpdate(delta);
        Entity player = this.GetEntity(2)!;
        RigidBody2D body = player.GetComponent<RigidBody2D>()!;
        Camera2D cam2D = SceneManager.ActiveCam2D!;
        
        if (Input.IsKeyDown(KeyboardKey.G)) {
            player.Transform.Rotation = Quaternion.CreateFromYawPitchRoll(0, 0, float.DegreesToRadians(90));
        }
        
        if (Input.IsKeyDown(KeyboardKey.H)) {
            player.Transform.Translation += Vector3.One;
        }
        
        if (Input.IsKeyDown(KeyboardKey.W)) {
            body.ApplyForceToCenter(new Vector2(0, -50 * 150), true);
        }
        
        if (Input.IsKeyDown(KeyboardKey.S)) {
            body.ApplyForceToCenter(new Vector2(0, 50 * 150), true);
        }
        
        if (Input.IsKeyDown(KeyboardKey.A)) {
            body.ApplyForceToCenter(new Vector2(-50 * 150, 0), true);
        }
        
        if (Input.IsKeyDown(KeyboardKey.D)) {
            body.ApplyForceToCenter(new Vector2(50 * 150, 0), true);
        }
        
        cam2D.Target = new Vector2(body.Position.X, body.Position.Y);
    }

    protected override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        base.Draw(context, framebuffer);
        Camera2D? cam2D = SceneManager.ActiveCam2D;
        
        if (cam2D == null) {
            return;
        }
        
        context.PrimitiveBatch.Begin(context.CommandList, framebuffer.OutputDescription, view: cam2D.GetView());
        context.PrimitiveBatch.DrawFilledRectangle(new RectangleF(-200, -192, 400, 200), layerDepth: 0.4F, color: new Color(192, 112, 162, 100));
        context.PrimitiveBatch.End();
    }
}