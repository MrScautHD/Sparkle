using System.Numerics;
using Raylib_cs;
using Sparkle.csharp;
using Sparkle.csharp.entity;
using Sparkle.csharp.graphics.util;
using Sparkle.csharp.gui;
using Sparkle.csharp.scene;

namespace Test; 

public class Test3DScene : Scene {
    
    public Test3DScene(string name) : base(name) { }
    
    protected override void Init() {
        Vector3 pos = new Vector3(10.0f, 10.0f, 10.0f);
        Cam3D cam3D = new Cam3D(pos, Vector3.Zero, Vector3.UnitY, 70, CameraProjection.CAMERA_PERSPECTIVE, CameraMode.CAMERA_FREE);
        this.AddEntity(cam3D);
        
        /*
        for (int i = 0; i < 1000; i++) {
            this.AddEntity(new TestEntity(new Vector3(0, i, 0)));
        }*/
        
        
        TestEntity entity = new TestEntity(new Vector3(0, 20, 0));
        this.AddEntity(entity);
        
        this.AddEntity(new GroundEntity(Vector3.Zero));
    }

    protected override void Update() {
        base.Update();
        
        if (Input.IsKeyPressed(KeyboardKey.KEY_E)) {
            GuiManager.SetGui(new TestGui("Test"));
        }

        if (Input.IsKeyPressed(KeyboardKey.KEY_R)) {
            GuiManager.SetGui(null);
        }
    }

    protected override void Draw() {
        base.Draw();
        
        SceneManager.MainCam3D!.BeginMode3D();
        
        ModelHelper.DrawGrid(100, 1);
        ModelHelper.DrawCube(SceneManager.MainCam3D.Target, 2, 2, 2, Color.RED);
        
        SceneManager.MainCam3D.EndMode3D();
    }
}