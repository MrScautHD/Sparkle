using System.Numerics;
using Raylib_cs;
using Sparkle.csharp.graphics;

namespace Sparkle.csharp.entity; 

public class Camera : Entity {

    public Graphics Graphics => Game.Instance.Graphics;
    
    private Camera3D _camera;
    
    public CameraMode Mode;
    
    public float Zoom;

    public Camera(Transform transform, float fov, CameraMode mode) : base(transform) {
        this.Tag = "camera";
        this.Mode = mode;
        this._camera = new() {
            position = transform.translation,
            target = Vector3.Zero,
            up = Vector3.UnitY,
            fovy = fov,
            projection = CameraProjection.CAMERA_PERSPECTIVE
        };
    }

    public float Fov {
        get => this._camera.fovy;
        set => this._camera.fovy = value;
    }

    protected internal override void Update() {
        base.Update();
        Raylib.UpdateCameraPro(ref this._camera, new Vector3(), Raymath.QuaternionToEuler(this.Rotation), this.Zoom);
        this._camera.position = this.Position;
    }

    protected internal override void Draw() {
        base.Draw();
        
        this.Graphics.BeginMode3D(this._camera);
        
        Raylib.DrawCube(Vector3.Zero, 2.0f, 2.0f, 2.0f, Color.RED);
        Raylib.DrawCubeWires(Vector3.Zero, 2.0f, 2.0f, 2.0f, Color.MAROON);
        Raylib.DrawGrid(1000, 10);
        
        Raylib.DrawPlane(new Vector3(0.0f, 0.0f, 0.0f), new Vector2(32.0f, 32.0f), Color.LIGHTGRAY);
        
        this.Graphics.EndMode3D();
    }
    
    public unsafe void MoveToTarget(Vector3 target, float delta) {
        this._camera.target = target;
        
        fixed (Camera3D* cameraPtr = &this._camera) {
            Raylib.CameraMoveToTarget(cameraPtr, delta);
        }
    }

    public Matrix4x4 GetViewMatrix() {
        return Raylib.GetCameraMatrix(this._camera);
    }
    
    public Matrix4x4 GetTransformMatrix() {
        return this.GetViewMatrix();
    }

    public unsafe Matrix4x4 GetProjectionMatrix() {
        fixed (Camera3D* cameraPtr = &this._camera) {
            return Raylib.GetCameraProjectionMatrix(cameraPtr, 1);
        }
    }
}