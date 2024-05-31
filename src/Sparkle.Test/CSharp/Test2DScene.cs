using System.Numerics;
using Box2D.NetStandard.Collision.Shapes;
using Box2D.NetStandard.Dynamics.Bodies;
using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.Physics.Dim2.Def;
using Sparkle.CSharp.Scenes;

namespace Sparkle.Test.CSharp;

public class Test2DScene : Scene {
    
    public Test2DScene(string name) : base(name, SceneType.Scene2D) { }

    protected override void Init() {
        base.Init();
        
        Cam2D cam2D = new Cam2D(new Vector2(0, 0), new Vector2(0, 0), Cam2D.CameraFollowMode.Smooth);
        this.AddEntity(cam2D);

        // PLAYER
        Test2DEntity player = new Test2DEntity(new Vector2(0, -32));
        player.Rotation = Quaternion.CreateFromYawPitchRoll(0, 0, 90 * RayMath.Deg2Rad);
        this.AddEntity(player);
        
        player.AddComponent(new RigidBody2D(new BodyDefinition(), new FixtureDefinition(new PolygonShape(16.0F / 2, 16.0F / 2)) {
            Density = 1.0F,
        }));
        
        // GROUND
        Test2DEntity entity = new Test2DEntity(new Vector2(0, -8));
        this.AddEntity(entity);
        
        entity.AddComponent(new RigidBody2D(new BodyDefinition() {
            Type = BodyType.Static
        }, new FixtureDefinition(new PolygonShape(16.0F / 2, 16.0F / 2))));
        
        // ELEMENTS
        for (int i = 0; i < 6; i++) {
            Test2DEntity element = new Test2DEntity(new Vector2(32 + (16 * i), -48));
            this.AddEntity(element);
        
            element.AddComponent(new RigidBody2D(new BodyDefinition() {
                Type = BodyType.Static
            }, new FixtureDefinition(new PolygonShape(16.0F / 2, 16.0F / 2))));
        }
        
        // ELEMENTS STAIRS
        for (int i = 0; i < 5; i++) {
            Test2DEntity element = new Test2DEntity(new Vector2(128 + (16 * i), -48 - (16 * i)));
            this.AddEntity(element);
        
            element.AddComponent(new RigidBody2D(new BodyDefinition() {
                Type = BodyType.Static
            }, new FixtureDefinition(new PolygonShape(16.0F / 2, 16.0F / 2))));
        }
    }

    protected override void Update() {
        base.Update();
        RigidBody2D body = this.GetEntity(1).GetComponent<RigidBody2D>();

        if (Input.IsKeyDown(KeyboardKey.G)) {
            this.GetEntity(1).Rotation = Quaternion.CreateFromYawPitchRoll(0, 0, 90 * RayMath.Deg2Rad);
        }
        
        if (Input.IsKeyDown(KeyboardKey.H)) {
            this.GetEntity(1).Position += Vector3.One;
        }
        
        if (Input.IsKeyDown(KeyboardKey.W)) {
            body.ApplyForceToCenter(new Vector2(0, -50 * 9));
        }
        
        if (Input.IsKeyDown(KeyboardKey.S)) {
            body.ApplyForceToCenter(new Vector2(0, 50 * 9));
        }
        
        if (Input.IsKeyDown(KeyboardKey.A)) {
            body.ApplyForceToCenter(new Vector2(-50 * 9, 0));
        }
        
        if (Input.IsKeyDown(KeyboardKey.D)) {
            body.ApplyForceToCenter(new Vector2(50 * 9, 0));
        }
        
        SceneManager.ActiveCam2D!.Target = new Vector2(body.Body.Position.X, body.Body.Position.Y);
    }

    protected override void Draw() {
        base.Draw();
        
        // Pink Layer
        Graphics.DrawRectangle(-200, -200, 400, 200, new Color(192, 112, 162, 100));
        
        RlGl.PushMatrix();
        RlGl.RotateF(90, 1, 0, 0);
        Graphics.DrawGrid(50, 8);
        RlGl.PopMatrix();
    }
}