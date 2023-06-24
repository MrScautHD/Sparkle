using System.Numerics;
using Raylib_cs;

namespace Sparkle.csharp.entity; 

public class Camera : Entity {
    
    private Camera3D _camera;
    
    private Quaternion _oldRotation;
    
    public CameraMode Mode;

    public float MouseSensitivity;
    public float GamepadSensitivity;

    public Camera(Vector3 position, float fov, CameraMode mode = CameraMode.CAMERA_CUSTOM) : base(position) {
        this.Tag = "camera";
        this.Mode = mode;
        this.MouseSensitivity = 0.1F;
        this.GamepadSensitivity = 0.05F;
        this._camera = new() {
            position = position,
            target = position + Vector3.UnitZ,
            up = Vector3.UnitY,
            fovy = fov,
            projection = CameraProjection.CAMERA_PERSPECTIVE
        };
    }
    
    public new Vector3 Position {
        get => this._camera.position;
        set => this._camera.position = value;
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
        
        switch (this.Mode) {
            case CameraMode.CAMERA_CUSTOM:
                //this.Rotation.Y += 1;
                this.InputController();
                break;
            
            case CameraMode.CAMERA_FREE:
                this.InputController();
                break;
            
            case CameraMode.CAMERA_ORBITAL:
                Quaternion rotation = Raymath.QuaternionFromEuler(this.Rotation.X, this.Rotation.Y + 1 * Time.DeltaTime, this.Rotation.Z);
                Vector3 view = Vector3.Subtract(this.Position, this.Target);
                Vector3 pos = Vector3.Transform(view, rotation);
                this.Position = Vector3.Add(this.Target, pos);
                
                this.MoveToTarget(Input.GetMouseWheelMove());
                break;
            
            case CameraMode.CAMERA_FIRST_PERSON:
                Raylib.UpdateCamera(ref this._camera, CameraMode.CAMERA_FIRST_PERSON);
                //TODO DO A OWN FIRST PERSON CAMERA
                break;
            
            case CameraMode.CAMERA_THIRD_PERSON:
                Raylib.UpdateCamera(ref this._camera, CameraMode.CAMERA_THIRD_PERSON);
                //TODO DO A OWN THIRD PERSON CAMERA
                break;
        }
        
        this.RotationUpdate();
    }

    private void InputController() {
        if (!Input.IsGamepadAvailable(0)) {
            this.Rotation.X += Input.GetMouseDelta().X * this.MouseSensitivity;
            this.Rotation.Y += Input.GetMouseDelta().Y * this.MouseSensitivity;
            
            if (Input.IsKeyDown(KeyboardKey.KEY_W)) {
                this.Move(new Vector3(1, 0, 0));
            }

            if (Input.IsKeyDown(KeyboardKey.KEY_S)) {
                this.Move(new Vector3(-1, 0, 0));
            }

            if (Input.IsKeyDown(KeyboardKey.KEY_A)) {
                this.Move(new Vector3(0, 0, -1));
            }
                
            if (Input.IsKeyDown(KeyboardKey.KEY_D)) {
                this.Move(new Vector3(0, 0, 1));
            }
            
            if (Input.IsKeyDown(KeyboardKey.KEY_SPACE)) {
                this.Move(new Vector3(0, 1, 0));
            }
            
            if (Input.IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT)) {
                this.Move(new Vector3(0, 1, 0));
            }
        }
        else {
            this.Rotation.X += (Input.GetGamepadAxisMovement(0, GamepadAxis.GAMEPAD_AXIS_RIGHT_X) * 2) * this.GamepadSensitivity;
            this.Rotation.Y += (Input.GetGamepadAxisMovement(0, GamepadAxis.GAMEPAD_AXIS_RIGHT_Y) * 2) * this.GamepadSensitivity;
            
            if (Input.IsGamepadButtonDown(0, GamepadButton.GAMEPAD_BUTTON_RIGHT_TRIGGER_2)) {
                this.Move(new Vector3(1, 0, 0));
            }
            
            if (Input.IsGamepadButtonDown(0, GamepadButton.GAMEPAD_BUTTON_LEFT_TRIGGER_2)) {
                this.Move(new Vector3(-1, 0, 0));
            }
        }
    }
    
    private void RotationUpdate() {
        // TODO MADE TRY TO GET THE REAL ROTATION, CHECK ON RAYLIB-C
        if (this.Rotation != this._oldRotation) {
            Raylib.UpdateCameraPro(ref this._camera, Vector3.Zero, -Raymath.QuaternionToEuler(this._oldRotation), 0);
            Raylib.UpdateCameraPro(ref this._camera, Vector3.Zero, Raymath.QuaternionToEuler(this.Rotation), 0);
            
            this._oldRotation = this.Rotation;
            //Logger.Error(this.Rotation + "");
        }
        
        unsafe {
            fixed (Camera3D* cameraPtr = &this._camera) {
                Vector3 xyz = new Vector3(0, 1, 0);
                Vector3 view = Vector3.Normalize(Vector3.Subtract(this.Target, this.Position));
                float angle = (float)Math.Acos(Vector3.Dot(xyz, view));
                float angleDegrees = angle * (float) (180.0f / Math.PI);

                Logger.Error(Raymath.QuaternionToEuler(Raymath.QuaternionFromMatrix(this.GetViewMatrix())) * (float) (180.0 / Math.PI) + "                    REAL: " + this.Rotation);
                
                
                //Quaternion quaternion = Raymath.QuaternionFromMatrix(this.GetViewMatrix());
                //Vector3 euler = Raymath.QuaternionToEuler(quaternion);
                
                //Logger.Error(quaternion +"");
            }
        }
    }

    public unsafe void Move(Vector3 speedVector, bool moveInWorldPlane = false) {
        fixed (Camera3D* cameraPtr = &this._camera) {
            Raylib.CameraMoveForward(cameraPtr, speedVector.X * Time.DeltaTime, moveInWorldPlane);
            Raylib.CameraMoveUp(cameraPtr, speedVector.Y * Time.DeltaTime);
            Raylib.CameraMoveRight(cameraPtr, speedVector.Z * Time.DeltaTime, moveInWorldPlane);
        }
    }

    public unsafe void MoveToTarget(float speed) {
        fixed (Camera3D* cameraPtr = &this._camera) {
            Raylib.CameraMoveToTarget(cameraPtr, -speed);
        }
    }

    public Matrix4x4 GetViewMatrix() {
        return Raylib.GetCameraMatrix(this._camera);
    }
    
    public Matrix4x4 GetTransformMatrix() {
        return this.GetViewMatrix(); // TODO CHECK IF THAT RIGHT
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