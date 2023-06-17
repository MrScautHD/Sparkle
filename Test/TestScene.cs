using Raylib_cs;
using Sparkle.csharp.entity;
using Sparkle.csharp.scene;

namespace Test; 

public class TestScene : Scene {
    
    public TestScene(string name) : base(name) {
        Camera camera = new Camera(new Transform(), 70, CameraMode.CAMERA_ORBITAL);
        this.AddEntity(camera);
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();
    }
}