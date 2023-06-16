using System.Numerics;
using Raylib_cs;

namespace Sparkle.csharp.entity; 

public class Camera : Entity {

    public Camera3D Camera3D { get; private set; }
    
    public Camera(Transform transform) : base(transform) {
        this.Tag = "camera";
        
        this.Camera3D = new() {
            position = transform.translation,
            target = Vector3.Zero,
            up = Vector3.UnitY,
            fovy = 70F,
            projection = CameraProjection.CAMERA_PERSPECTIVE
        };
    }

    protected internal override void Draw() {
        base.Draw();

        Camera3D camera = this.Camera3D;
        
        Raylib.UpdateCamera(ref camera, CameraMode.CAMERA_FREE);
        Raylib.ClearBackground(Color.DARKGRAY);
        
        Raylib.BeginMode3D(camera);
        
        Raylib.DrawCube(Vector3.Zero, 2.0f, 2.0f, 2.0f, Color.RED);
        Raylib.DrawCubeWires(Vector3.Zero, 2.0f, 2.0f, 2.0f, Color.MAROON);
        Raylib.DrawGrid(10, 1);
        
        Raylib.EndMode3D();
    }
}