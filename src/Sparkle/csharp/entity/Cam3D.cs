using System.Numerics;
using Raylib_cs;
using Sparkle.csharp.gui;
using Sparkle.csharp.window;

namespace Sparkle.csharp.entity; 

public class Cam3D : Entity {
    
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

    /// <summary>
    /// Initializes a new instance of the <see cref="Cam3D"/> with the specified position, field of view (fov), and camera mode.
    /// Also sets various camera properties such as tag, projection type, aspect ratio, near/far planes, and sensitivity.
    /// Finally, it initializes the View and Projection matrices for the camera.
    /// </summary>
    /// <param name="position">Initial position of the camera in 3D space, as a Vector3.</param>
    /// <param name="fov">Field of view angle, in degrees, in the y-axis.</param>
    /// <param name="mode">Camera movement mode, default is CameraMode.CAMERA_FREE.</param>
    public Cam3D(Vector3 position, float fov, CameraMode mode = CameraMode.CAMERA_FREE) : base(position) {
        this.Tag = "camera";
        this.ProjectionType = CameraProjection.CAMERA_PERSPECTIVE;
        this.Fov = fov;
        this.Up = Vector3.UnitY;
        this.AspectRatio = (float) Window.GetScreenWidth() / (float) Window.GetScreenHeight();
        this.NearPlane = 0.01F;
        this.FarPlane = 1000;
        this.Target = position + Vector3.UnitZ;
        this.Mode = mode;
        this.MouseSensitivity = 0.1F;
        this.GamepadSensitivity = 0.05F;
        
        this.View = Raymath.MatrixLookAt(this.Position, this.Target, this.Up);
        this.Projection = this.GenProjection();
    }
    
    protected internal override void Update() {
        base.Update();

        this.CalculateTargetPosition();
        
        switch (this.Mode) {
            case CameraMode.CAMERA_FREE:
                if (GuiManager.ActiveGui == null) {
                    this.InputController();
                }
                break;
            
            case CameraMode.CAMERA_ORBITAL:
                Matrix4x4 rotation = Raymath.MatrixRotate(this.Up, -1.5F * Time.Delta);
                Vector3 view = this.Position - this.Target;
                Vector3 pos = Vector3.Transform(view, rotation);
                this.Position = this.Target + pos;
                
                if (GuiManager.ActiveGui == null) {
                    this.MoveToTarget(Input.GetMouseWheelMove());
                }
                break;
            
            case CameraMode.CAMERA_FIRST_PERSON:
                //TODO DO A OWN FIRST PERSON CAMERA
                break;
            
            case CameraMode.CAMERA_THIRD_PERSON:
                //TODO DO A OWN THIRD PERSON CAMERA
                break;
        }
    }
    
    /// <summary>
    /// Controls the entity's movement and rotation based on input from mouse, keyboard, or gamepad.
    /// </summary>
    private void InputController() {
        if (!Input.IsGamepadAvailable(0)) {
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

    /// <summary>
    /// Calculates the target position based on the camera's mode and rotation.
    /// </summary>
    private void CalculateTargetPosition() {
        if (this.Mode != CameraMode.CAMERA_ORBITAL) {
            Vector3 viewDir = Vector3.Transform(Vector3.UnitZ, this.Rotation);
            this.Target = this.Position + viewDir;
        }
    }
    
    /// <summary>
    /// Retrieves the normalized forward direction vector of the camera.
    /// </summary>
    /// <returns>The normalized forward direction vector.</returns>
    public Vector3 GetForward() {
        return Vector3.Normalize(Vector3.Subtract(this.Position, this.Target));
    }
    
    /// <summary>
    /// Moves the camera forward or backward by a specified speed.
    /// </summary>
    /// <param name="speed">The speed of movement.</param>
    public void MoveForward(float speed) {
        this.Position -= this.GetForward() * (speed * Time.Delta);
    }
    
    /// <summary>
    /// Moves the object towards a target position based on the given delta value.
    /// </summary>
    /// <param name="delta">The amount of movement to apply.</param>
    public void MoveToTarget(float delta) {
        float distance = Vector3.Distance(this.Position, this.Target);
        
        if (distance - delta <= 0) {
            return;
        }

        this.Position += this.GetForward() * -delta;
    }
    
    /// <summary>
    /// Moves the object based on the provided speed vector.
    /// </summary>
    /// <param name="speedVector">The vector representing the movement speed along different axes.</param>
    public void Move(Vector3 speedVector) {
        Vector3 right = Vector3.Normalize(Vector3.Cross(this.Up, this.GetForward()));
        
        this.Position -= right * (speedVector.X * Time.Delta);
        this.Position += this.Up * (speedVector * Time.Delta);
        this.Position -= this.GetForward() * (speedVector.Z * Time.Delta);
    }

    /// <summary>
    /// Rotates the object using the specified yaw, pitch, and roll angles.
    /// </summary>
    /// <param name="yaw">The yaw angle (rotation around the vertical axis) in degrees.</param>
    /// <param name="pitch">The pitch angle (rotation around the lateral axis) in degrees, clamped between -89 and 89 degrees.</param>
    /// <param name="roll">The roll angle (rotation around the longitudinal axis) in degrees.</param>
    public void RotateWithAngle(float yaw, float pitch, float roll) {
        this._angleRot.Y = yaw % 360;
        this._angleRot.X = Math.Clamp(pitch, -89, 89);
        this._angleRot.Z = roll % 360;
        
        this.Rotation = Quaternion.CreateFromYawPitchRoll(this._angleRot.Y * Raylib.DEG2RAD, this._angleRot.X * Raylib.DEG2RAD, this._angleRot.Z * Raylib.DEG2RAD);
    }
    
    /// <summary>
    /// Generates a projection matrix based on the camera's projection type.
    /// </summary>
    /// <returns>The generated projection matrix.</returns>
    private Matrix4x4 GenProjection() {
        if (this.ProjectionType == CameraProjection.CAMERA_PERSPECTIVE) {
            return Raymath.MatrixPerspective(this.Fov * Raylib.DEG2RAD, this.AspectRatio, this.NearPlane, this.FarPlane);
        }
        else {
            float top = this.Fov / 2.0F;
            float right = top * this.AspectRatio;
            
            return Raymath.MatrixOrtho(-right, right, -top, top, this.NearPlane, this.FarPlane);
        }
    }
    
    /// <summary>
    /// Prepares the rendering context for 3D graphics by configuring matrices, projection, and depth testing.
    /// </summary>
    public void BeginMode3D() {
        Rlgl.rlDrawRenderBatchActive();
        Rlgl.rlMatrixMode(MatrixMode.PROJECTION);
        Rlgl.rlPushMatrix();
        Rlgl.rlLoadIdentity();
        
        this.AspectRatio = (float) Window.GetScreenWidth() / (float) Window.GetScreenHeight();

        this.Projection = this.GenProjection();
        Rlgl.rlSetMatrixProjection(this.Projection);
        
        Rlgl.rlMatrixMode(MatrixMode.MODELVIEW);
        Rlgl.rlLoadIdentity();
        
        this.View = Raymath.MatrixLookAt(this.Position, this.Target, this.Up);
        Rlgl.rlMultMatrixf(this.View);
        
        Rlgl.rlEnableDepthTest();
    }
    
    /// <summary>
    /// Ends the 3D rendering mode and performs necessary cleanup.
    /// </summary>
    public void EndMode3D() {
        Raylib.EndMode3D();
    }
}