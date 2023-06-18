using System.Numerics;
using Raylib_cs;

namespace Sparkle.csharp.entity; 

public class Camera : Entity {

    public Camera3D Camera3D;
    public CameraMode Mode;
    
    public float Zoom;

    public Camera(Transform transform, float fov, CameraMode mode = CameraMode.CAMERA_CUSTOM) : base(transform) {
        this.Tag = "camera";
        this.Mode = mode;
        this.Camera3D = new() {
            position = transform.translation,
            target = Vector3.Zero,
            up = Vector3.UnitY,
            fovy = fov,
            projection = CameraProjection.CAMERA_PERSPECTIVE
        };
    }

    protected internal override void Update() {
        base.Update();
        if (this.Mode == CameraMode.CAMERA_CUSTOM) {
            //Raylib.UpdateCameraPro(ref this._camera, new Vector3(0.001F), Raymath.QuaternionToEuler(this.Rotation), this.Zoom);
            
            // TODO TRY TO FIX MOVEMENT + POSITION
        }
        else {
            Raylib.UpdateCamera(ref this.Camera3D, this.Mode);
        }
    }

    public unsafe void MoveToTarget(Vector3 target, float delta) {
        this.Camera3D.target = target;
        
        fixed (Camera3D* cameraPtr = &this.Camera3D) {
            Raylib.CameraMoveToTarget(cameraPtr, delta);
        }
    }

    public Matrix4x4 GetViewMatrix() {
        return Raylib.GetCameraMatrix(this.Camera3D);
    }
    
    public Matrix4x4 GetTransformMatrix() {
        return this.GetViewMatrix();
    }

    public unsafe Matrix4x4 GetProjectionMatrix() {
        fixed (Camera3D* cameraPtr = &this.Camera3D) {
            return Raylib.GetCameraProjectionMatrix(cameraPtr, 1);
        }
    }
}