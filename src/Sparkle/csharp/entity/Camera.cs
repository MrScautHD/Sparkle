using System.Numerics;
using Raylib_cs;

namespace Sparkle.csharp.entity; 

public class Camera : Entity {
    
    private Camera3D _camera;
    
    public CameraMode Mode;

    public Camera(Transform transform, float fov, CameraMode mode = CameraMode.CAMERA_CUSTOM) : base(transform) {
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

    public Vector3 Target {
        get => this._camera.target;
        set => this._camera.target = value;
    }
    
    public float Fov {
        get => this._camera.fovy;
        set => this._camera.fovy = value;
    }

    protected internal override void Update() {
        base.Update();
        this._camera.position = this.Position;
        
        switch (this.Mode) {
            case CameraMode.CAMERA_CUSTOM:
                //Raylib.UpdateCameraPro(ref this._camera, new Vector3(0.001F), Raymath.QuaternionToEuler(this.Rotation), 0);
                Raylib.UpdateCamera(ref this._camera, CameraMode.CAMERA_CUSTOM);


                float speed = 0.10F * Time.DeltaTime;
                this.Position += this._camera.target * speed;
                
                Logger.Error(this.Position + "");
                break;
            
            case CameraMode.CAMERA_FREE:
                //TODO DO A OWN FREE CAMERA
                break;
            
            case CameraMode.CAMERA_ORBITAL:
                Raylib.UpdateCamera(ref this._camera, CameraMode.CAMERA_ORBITAL);
                this.Position = this._camera.position;
                break;
            
            case CameraMode.CAMERA_FIRST_PERSON:
                //TODO DO A OWN FIRST PERSON CAMERA
                break;
            
            case CameraMode.CAMERA_THIRD_PERSON:
                //TODO DO A OWN THIRD PERSON CAMERA
                break;
        }
    }

    public unsafe void MoveToTarget(float delta) {
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
    
    public Camera3D GetCamera3D() {
        return this._camera;
    }
}