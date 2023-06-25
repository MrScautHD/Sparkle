using System.Numerics;
using Raylib_cs;

namespace Sparkle.csharp.entity; 

public class Camera : Entity {
    
    public Matrix4x4 View { get; private set; }
    public Matrix4x4 Projection { get; private set; }
    
    public CameraProjection ProjectionType;
    
    public float Fov;
    public Vector3 Up;

    private float _aspectRatio;
    private float _nearPlane;
    private float _farPlane;

    public Vector3 Target;
    
    public CameraMode Mode;

    public float MouseSensitivity;
    public float GamepadSensitivity;

    public Camera(Vector3 position, float fov, CameraMode mode = CameraMode.CAMERA_CUSTOM) : base(position) {
        this.Tag = "camera";
        this.ProjectionType = CameraProjection.CAMERA_PERSPECTIVE;
        this.Fov = fov;
        this.Up = Vector3.UnitY;
        this._nearPlane = 0.01F;
        this._farPlane = 1000;
        this.Target = position + Vector3.UnitZ;
        this.Mode = mode;
        this.MouseSensitivity = 0.1F;
        this.GamepadSensitivity = 0.05F;
        
        this._aspectRatio = Raylib.GetScreenWidth() / (float) Raylib.GetScreenHeight();
        this.View = Raymath.MatrixLookAt(position, this.Target, this.Up);
        this.Projection = Raymath.MatrixPerspective(fov * Raylib.DEG2RAD, this._aspectRatio, this._nearPlane, this._farPlane);
    }
    
    protected internal override void Update() {
        base.Update();
        
        switch (this.Mode) {
            case CameraMode.CAMERA_CUSTOM:
                this.InputController();
                break;
            
            case CameraMode.CAMERA_FREE:
                this.InputController();
                break;
            
            case CameraMode.CAMERA_ORBITAL:
                this.RotateAxisAngle(Vector3.UnitY, 1 * Time.DeltaTime);
                Vector3 view = Vector3.Subtract(this.Position, this.Target);
                Vector3 pos = Vector3.Transform(view, this.Rotation);
                this.Position = Vector3.Add(this.Target, pos);
                
                this.MoveToTarget(Input.GetMouseWheelMove());
                break;
            
            case CameraMode.CAMERA_FIRST_PERSON:
                //TODO DO A OWN FIRST PERSON CAMERA
                break;
            
            case CameraMode.CAMERA_THIRD_PERSON:
                //TODO DO A OWN THIRD PERSON CAMERA
                break;
        }
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

    public Vector3 GetForward() {
        return Vector3.Normalize(Vector3.Subtract(this.Position, this.Target));
    }

    public void MoveForward(float speed) {
        this.Position -= (this.GetForward() * speed) * Time.DeltaTime;
    }
    
    public void MoveToTarget(float delta) {
        float distance = Vector3.Distance(this.Position, this.Target);
        
        if (distance - delta <= 0) {
            return;
        }

        this.Position += this.GetForward() * -delta;
    }
    
    public void Move(Vector3 speedVector) {
        // TODO CHECK THAT AGAIN IF THE VECTORS RIGHT!
        Vector3 right = Vector3.Cross(this.GetForward(), this.Up);
        
        this.Position += (right * speedVector.X) * Time.DeltaTime;
        this.Position += (this.GetForward().Y * speedVector) * Time.DeltaTime;
        this.Position += (this.Up * speedVector) * Time.DeltaTime;
    }

    public void BeginMode3D() {
        this._aspectRatio = Raylib.GetScreenWidth() / (float) Raylib.GetScreenHeight();
        
        Rlgl.rlDrawRenderBatchActive();
        Rlgl.rlMatrixMode(MatrixMode.PROJECTION);
        Rlgl.rlPushMatrix();
        Rlgl.rlLoadIdentity();
        
        if (this.ProjectionType == CameraProjection.CAMERA_PERSPECTIVE) {
            float top = Rlgl.RL_CULL_DISTANCE_NEAR * MathF.Tan(this.Fov * 0.5F * (MathF.PI / 180.0f));
            float right = top * this._aspectRatio;

            Rlgl.rlFrustum(-right, right, -top, top, this._nearPlane, this._farPlane);
            this.Projection = Raymath.MatrixPerspective(this.Fov * Raylib.DEG2RAD, this._aspectRatio, this._nearPlane, this._farPlane);
        }
        else {
            float top = this.Fov / 2.0F;
            float right = top * this._aspectRatio;

            Rlgl.rlOrtho(-right, right, -top, top, this._nearPlane, this._farPlane);
            this.Projection = Raymath.MatrixOrtho(-right, right, -top, top, this._nearPlane, this._farPlane);
        }
        
        Rlgl.rlMatrixMode(MatrixMode.MODELVIEW);
        Rlgl.rlLoadIdentity();
        
        this.View = Raymath.MatrixLookAt(this.Position, this.Target, this.Up);
        Rlgl.rlMultMatrixf(this.View);
        
        Rlgl.rlEnableDepthTest();
    }

    public void EndMode3D() {
        Raylib.EndMode3D();
    }
}