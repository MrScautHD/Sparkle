using System.Numerics;
using Raylib_cs;
using Sparkle.csharp;
using Sparkle.csharp.entity;
using Sparkle.csharp.gui;
using Sparkle.csharp.scene;

namespace Test; 

public class TestScene : Scene {

    public TestScene(string name) : base(name) {
    }

    protected override void Init() {
        
        // CREATE TRANSFORM
        Vector3 pos = new Vector3(10.0f, 10.0f, 10.0f);

        // CREATE CUSTOM CAMERA
        Camera camera = new Camera(pos, 70, CameraMode.CAMERA_ORBITAL) {
            Target = new Vector3(0, 0, 0)
        };
        
        // ADD OBJECT TO THE SCENE
        this.AddEntity(camera);
        
        // ADD TEST ENTITY
        this.AddEntity(new TestEntity(Vector3.Zero));
    }

    protected override void Update() {
        base.Update();
        
        if (Input.IsKeyPressed(KeyboardKey.KEY_E)) {
            TestGui gui = new TestGui("test");
            GuiManager.SetGui(gui);
        }

        if (Input.IsKeyPressed(KeyboardKey.KEY_R)) {
            GuiManager.SetGui(null);
        }
    }

    protected override void Draw() {
        base.Draw();
        
        // BEGIN 3D
        SceneManager.MainCamera!.BeginMode3D();

        // DRAW GIRD
        Raylib.DrawGrid(10, 1);
        
        // DRAW CUBE
        Raylib.DrawCube(new Vector3(3, 2, 3), 5, 5, 5, Color.PURPLE);
        
        // DRAW LINE
        Raylib.DrawLine3D(new Vector3(10, 3, 4), new Vector3(-10, -3, -4), Color.RED);
        
        // DRAW SECOND LINE
        Raylib.DrawLine3D(new Vector3(0, 3, 4), new Vector3(-10, -3, -4), Color.BLUE);
        
        Raylib.DrawCube(SceneManager.MainCamera.Target, 2, 2, 2, Color.RED);

        // END 3D
        SceneManager.MainCamera.EndMode3D();
    }
}