using System.Numerics;
using Raylib_cs;
using Sparkle.csharp.entity;
using Sparkle.csharp.scene;

namespace Test; 

public class TestScene : Scene {
    
    public TestScene(string name) : base(name) {
        
    }

    protected override void Init() {
        
        // CREATE TRANSFORM
        Vector3 pos = new Vector3(10.0f, 10.0f, 10.0f);
        
        // CREATE ORBIT CAMERA
        //Camera camera = new Camera(pos, 70, CameraMode.CAMERA_ORBITAL) {
        //    Target = Vector3.Zero
        //};
        
        // CREATE CUSTOM CAMERA
        Camera camera = new Camera(pos, 70, CameraMode.CAMERA_CUSTOM) {
           
        };
        
        // ADD OBJECT TO THE SCENE
        this.AddEntity(camera);
    }

    protected override void Draw() {
        base.Draw();
        
        // BEGIN 3D
        Raylib.BeginMode3D(SceneManager.MainCamera!.GetCamera3D());

        // DRAW GIRD
        Raylib.DrawGrid(10, 1);
        
        // DRAW CUBE
        Raylib.DrawCube(new Vector3(3, 2, 3), 5, 5, 5, Color.PURPLE);
        
        // DRAW LINE
        Raylib.DrawLine3D(new Vector3(10, 3, 4), new Vector3(-10, -3, -4), Color.RED);
        
        // DRAW SECOND LINE
        Raylib.DrawLine3D(new Vector3(0, 3, 4), new Vector3(-10, -3, -4), Color.BLUE);
        
        Raylib.DrawCube(SceneManager.MainCamera!.GetCamera3D().target, 2, 2, 2, Color.RED);

        // END 3D
        Raylib.EndMode3D();
    }
}