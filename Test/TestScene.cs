using System.Numerics;
using Raylib_cs;
using Sparkle.csharp.entity;
using Sparkle.csharp.scene;

namespace Test; 

public class TestScene : Scene {
    
    public TestScene(string name) : base(name) {
        Transform transform = new Transform {
            translation = new Vector3(10.0f, 10.0f, 10.0f)
        };

        Camera camera = new Camera(transform, 70, CameraMode.CAMERA_ORBITAL);
        this.AddEntity(camera);
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();
    }
}