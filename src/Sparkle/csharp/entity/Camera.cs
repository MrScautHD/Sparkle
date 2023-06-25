using System.Numerics;
using Raylib_cs;

namespace Sparkle.csharp.entity; 

public class Camera : Entity {
    
    public Matrix4x4 View { get; private set; }
    public Matrix4x4 Projection { get; private set; }
    
    public CameraProjection ProjectionType;
    
    public float Fov;
    public Vector3 Up;

    public float AspectRatio;
    public float NearPlane;
    public float FarPlane;

    public CameraMode Mode;

    public float MouseSensitivity;
    public float GamepadSensitivity;

    public Vector3 Target;
    
    private Vector3 _angleRot;
    public Vector3 AngleRot => this._angleRot;

    public Camera(Vector3 position, float fov, CameraMode mode = CameraMode.CAMERA_FREE) : base(position) {
        this.Tag = "camera";
        this.ProjectionType = CameraProjection.CAMERA_PERSPECTIVE;
        this.Fov = fov;
        this.Up = Vector3.UnitY;
        this.AspectRatio = (float) this.Window.GetScreenSize().Width / (float) this.Window.GetScreenSize().Height;
        this.NearPlane = 0.01F;
        this.FarPlane = 1000;
        this.Target = position + Vector3.UnitZ;
        this.Mode = mode;
        this.MouseSensitivity = 0.1F;
        this.GamepadSensitivity = 0.05F;
        
        this.View = Raymath.MatrixLookAt(this.Position, this.Target, this.Up);
        this.GenProjection();
    }
    
    protected internal override void Update() {
        base.Update();
        
        this.CalculateTargetPosition();
        
        switch (this.Mode) {
            case CameraMode.CAMERA_FREE:
                this.InputController();
                break;
            
            case CameraMode.CAMERA_ORBITAL:
                this.RotateWithAngle(30 * Time.DeltaTime, this._angleRot.X, this._angleRot.Z);
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
        if (Input.IsGamepadAvailable(0)) {
            float yaw = this._angleRot.Y - (Input.GetMouseDelta().X * this.MouseSensitivity);
            float pitch = this._angleRot.X + (Input.GetMouseDelta().Y * this.MouseSensitivity);
            this.RotateWithAngle(yaw, pitch, 0);

            if (Input.IsKeyDown(KeyboardKey.KEY_W)) {
                this.Move(new Vector3(0, 0, 1));
            }

            if (Input.IsKeyDown(KeyboardKey.KEY_S)) {
                this.Move(new Vector3(0, 0, -1));
            }

            if (Input.IsKeyDown(KeyboardKey.KEY_A)) {
                this.Move(new Vector3(1, 0, 0));
            }
                
            if (Input.IsKeyDown(KeyboardKey.KEY_D)) {
                this.Move(new Vector3(-1, 0, 0));
            }
            
            if (Input.IsKeyDown(KeyboardKey.KEY_SPACE)) {
                this.Move(new Vector3(0, 1, 0));
            }
            
            if (Input.IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT)) {
                this.Move(new Vector3(0, -1, 0));
            }
        }
        else {
            float yaw = this._angleRot.Y - (Input.GetGamepadAxisMovement(0, GamepadAxis.GAMEPAD_AXIS_RIGHT_X) * 2) * this.GamepadSensitivity;
            float pitch = this._angleRot.X + (Input.GetGamepadAxisMovement(0, GamepadAxis.GAMEPAD_AXIS_RIGHT_Y) * 2) * this.GamepadSensitivity;
            this.RotateWithAngle(yaw, pitch, 0);
            
            if (Input.IsGamepadButtonDown(0, GamepadButton.GAMEPAD_BUTTON_RIGHT_TRIGGER_2)) {
                this.MoveForward(1);
            }
            
            if (Input.IsGamepadButtonDown(0, GamepadButton.GAMEPAD_BUTTON_LEFT_TRIGGER_2)) {
                this.MoveForward(-1);
            }
        }
    }

    private void CalculateTargetPosition() {
        if (this.Mode != CameraMode.CAMERA_ORBITAL) {
            Vector3 viewDir = Vector3.Transform(Vector3.UnitZ, this.Rotation);
            this.Target = this.Position + viewDir;
        }
    }
    
    public Vector3 GetForward() {
        // TODO Find a way to not use Target anymore for this (Use Rotation for it, and create something like GetLookToTarget...)
        return Vector3.Normalize(Vector3.Subtract(this.Position, this.Target));
    }
    
    public void MoveForward(float speed) {
        this.Position -= this.GetForward() * (speed * Time.DeltaTime);
    }
    
    public void MoveToTarget(float delta) {
        float distance = Vector3.Distance(this.Position, this.Target);
        
        if (distance - delta <= 0) {
            return;
        }

        this.Position += this.GetForward() * -delta;
    }
    
    public void Move(Vector3 speedVector) {
        Vector3 right = Vector3.Normalize(Vector3.Cross(this.Up, this.GetForward()));
        
        this.Position -= right * (speedVector.X * Time.DeltaTime);
        this.Position += this.Up * (speedVector * Time.DeltaTime);
        this.Position -= this.GetForward() * (speedVector.Z * Time.DeltaTime);

    }

    public void RotateWithAngle(float yaw, float pitch, float roll) {
        this._angleRot.Y = yaw % 360;
        this._angleRot.X = Math.Clamp(pitch, -89, 89);
        this._angleRot.Z = roll % 360;
        
        this.Rotation = Quaternion.CreateFromYawPitchRoll(this._angleRot.Y * Raylib.DEG2RAD, this._angleRot.X * Raylib.DEG2RAD, this._angleRot.Z * Raylib.DEG2RAD);
    }
    
    private void GenProjection() {
        if (this.ProjectionType == CameraProjection.CAMERA_PERSPECTIVE) {
            this.Projection = Raymath.MatrixPerspective(this.Fov * Raylib.DEG2RAD, this.AspectRatio, this.NearPlane, this.FarPlane);
        }
        else {
            float top = this.Fov / 2.0F;
            float right = top * this.AspectRatio;
            
            this.Projection = Raymath.MatrixOrtho(-right, right, -top, top, this.NearPlane, this.FarPlane);
        }
    }
    
    public void BeginMode3D() {
        Rlgl.rlDrawRenderBatchActive();
        Rlgl.rlMatrixMode(MatrixMode.PROJECTION);
        Rlgl.rlPushMatrix();
        Rlgl.rlLoadIdentity();
        
        this.AspectRatio = (float) this.Window.GetScreenSize().Width / (float) this.Window.GetScreenSize().Height;

        if (this.ProjectionType == CameraProjection.CAMERA_PERSPECTIVE) {
            float top = Rlgl.RL_CULL_DISTANCE_NEAR * MathF.Tan(this.Fov * 0.5F * Raylib.DEG2RAD);
            float right = top * this.AspectRatio;

            Rlgl.rlFrustum(-right, right, -top, top, this.NearPlane, this.FarPlane);
            this.Projection = Raymath.MatrixPerspective(this.Fov * Raylib.DEG2RAD, this.AspectRatio, this.NearPlane, this.FarPlane);
        }
        else {
            float top = this.Fov / 2.0F;
            float right = top * this.AspectRatio;

            Rlgl.rlOrtho(-right, right, -top, top, this.NearPlane, this.FarPlane);
            this.Projection = Raymath.MatrixOrtho(-right, right, -top, top, this.NearPlane, this.FarPlane);
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