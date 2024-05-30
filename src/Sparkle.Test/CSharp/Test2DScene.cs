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

        Test2DEntity player = new Test2DEntity(new Vector2(0, -40));
        player.Rotation = Quaternion.CreateFromYawPitchRoll(0, 0, 90 * RayMath.Deg2Rad);
        this.AddEntity(player);
        
        player.AddComponent(new RigidBody2D(new BodyDefinition(), new FixtureDefinition(new PolygonShape(1, 1)) {
            Density = 1.0F,
        }));
        
        Test2DEntity entity = new Test2DEntity(new Vector2(0, 0));
        this.AddEntity(entity);
        
        entity.AddComponent(new RigidBody2D(new BodyDefinition() {
            Type = BodyType.Static
        }, new FixtureDefinition(new PolygonShape(100, 10))));
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
            body.Body.ApplyForceToCenter(new Vector2(0, -50));
        }
        
        if (Input.IsKeyDown(KeyboardKey.S)) {
            body.Body.ApplyForceToCenter(new Vector2(0, 50));
        }
        
        if (Input.IsKeyDown(KeyboardKey.A)) {
            body.Body.ApplyForceToCenter(new Vector2(-50, 0));
        }
        
        if (Input.IsKeyDown(KeyboardKey.D)) {
            body.Body.ApplyForceToCenter(new Vector2(50, 0));
        }
        
        SceneManager.ActiveCam2D!.Target = new Vector2(body.Body.Position.X, body.Body.Position.Y);
    }

    protected override void Draw() {
        base.Draw();
        
        RlGl.PushMatrix();
        RlGl.RotateF(90, 1, 0, 0);
        Graphics.DrawGrid(50, 10);
        RlGl.PopMatrix();
        
        // OBJECTS
        Graphics.DrawRectangle(45, 123, 5, 5, Color.White);
        Graphics.DrawRectangle(5, 12, 30, 50, new Color(192, 112, 162, 100));
    }
}