using System.Numerics;
using Raylib_cs;
using Sparkle.CSharp;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Rendering.Helpers;
using Sparkle.CSharp.Scenes;

namespace Sparkle.Test.CSharp; 

public class Test2DScene : Scene {
    
    public Test2DScene(string name) : base(name, SceneType.Scene2D) { }

    protected override void Init() {
        base.Init();
        
        Cam2D cam2D = new Cam2D(new Vector2(0, 0), new Vector2(0, 0), Cam2D.CameraFollowMode.Smooth);
        this.AddEntity(cam2D);

        Test2DEntity player = new Test2DEntity(new Vector2(0, 0));
        this.AddEntity(player);
    }

    protected override void Update() {
        base.Update();
        Test2DEntity player = (Test2DEntity) this.GetEntity(1);

        if (Input.IsKeyDown(KeyboardKey.W)) {
            player.Position.Y -= 50.0F * Time.Delta;
        }
        
        if (Input.IsKeyDown(KeyboardKey.S)) {
            player.Position.Y += 50.0F * Time.Delta;
        }
        
        if (Input.IsKeyDown(KeyboardKey.A)) {
            player.Position.X -= 50.0F * Time.Delta;
        }
        
        if (Input.IsKeyDown(KeyboardKey.D)) {
            player.Position.X += 50.0F * Time.Delta;
        }
        
        SceneManager.MainCam2D!.Target = new Vector2(player.Position.X, player.Position.Y);
    }

    protected override void Draw() {
        base.Draw();
        
        // OBJECTS
        ShapeHelper.DrawRectangle(45, 123, 5, 5, Color.White);
        ShapeHelper.DrawRectangle(5, 12, 30, 50, new Color(192, 112, 162, 100));
    }
}