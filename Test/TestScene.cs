using Raylib_cs;
using Sparkle.csharp.entity;
using Sparkle.csharp.scene;

namespace Test; 

public class TestScene : Scene {
    
    public TestScene(string name) : base(name) {
        this.AddEntity(new Camera(new Transform(), 70, CameraMode.CAMERA_ORBITAL));
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();
    }
}