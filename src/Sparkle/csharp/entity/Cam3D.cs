using System.Numerics;
using Raylib_cs;
using Sparkle.csharp.graphics.util;
using Sparkle.csharp.gui;

namespace Sparkle.csharp.entity; 

public class Cam3D : Entity {

    private Camera3D _camera3D;
    private Frustum _frustum;
    
    public CameraMode Mode;
    
    public float MouseSensitivity;
    public float MovementSpeed;
    public float OrbitalSpeed;

    /// <summary>
    /// Represents a 3D camera for rendering 3D scenes.
    /// </summary>
    public Cam3D(Vector3 position, Vector3 target, Vector3 up, float fov, CameraProjection projection, CameraMode mode = CameraMode.CAMERA_FREE) : base(Vector3.Zero) {
        this.Tag = "camera3D";
        this._camera3D = new Camera3D();
        this._frustum = new Frustum();
        this.Position = position;
        this.Target = target;
        this.Up = up;
        this.Fov = fov;
        this.Projection = projection;
        this.Mode = mode;
        this.MouseSensitivity = 0.03F;
        this.MovementSpeed = 0.09F;
        this.OrbitalSpeed = 0.5F;
    }
    
    /// <summary>
    /// Gets or sets the position of the 3D camera in 3D space.
    /// </summary>
    public new Vector3 Position {
        get => this._camera3D.Position;
        set => this._camera3D.Position = value;
    }

    /// <summary>
    /// Gets the rotation of the 3D camera as a quaternion based on its current view.
    /// </summary>
    public new Quaternion Rotation {
        get {
            Matrix4x4 lookAt = Matrix4x4.CreateLookAt(this.Position, this.Target, this.Up);
            return Raymath.QuaternionFromMatrix(lookAt);
        }
    }
    
    /// <summary>
    /// Gets or sets the target position of the 3D camera.
    /// </summary>
    public Vector3 Target {
        get => this._camera3D.Target;
        set => this._camera3D.Target = value;
    }
    
    /// <summary>
    /// Gets or sets the up direction vector of the 3D camera.
    /// </summary>
    public Vector3 Up {
        get => this._camera3D.Up;
        set => this._camera3D.Up = value;
    }

    /// <summary>
    /// Gets or sets the vertical field of view (FOV) angle of the 3D camera.
    /// </summary>
    public float Fov {
        get => this._camera3D.FovY;
        set => this._camera3D.FovY = value;
    }
    
    /// <summary>
    /// Gets or sets the projection type of the 3D camera.
    /// </summary>
    public CameraProjection Projection {
        get => this._camera3D.Projection;
        set => this._camera3D.Projection = value;
    }
    
    protected internal override void Update() {
        base.Update();

        switch (this.Mode) {
            
            case CameraMode.CAMERA_FREE:
                if (GuiManager.ActiveGui != null) return;
                
                if (!Input.IsGamepadAvailable(0)) {
                    this.SetYaw(this.GetYaw() + Input.GetMouseDelta().X * this.MouseSensitivity, false);
                    this.SetPitch(this.GetPitch() - Input.GetMouseDelta().Y * this.MouseSensitivity, true, false, false);

                    if (Input.IsKeyDown(KeyboardKey.KEY_W)) {
                        this.MoveForward(this.MovementSpeed, true);
                    }
                    
                    if (Input.IsKeyDown(KeyboardKey.KEY_S)) {
                        this.MoveForward(-this.MovementSpeed, true);
                    }
                    
                    if (Input.IsKeyDown(KeyboardKey.KEY_A)) {
                        this.MoveRight(-this.MovementSpeed, true);
                    }
                    
                    if (Input.IsKeyDown(KeyboardKey.KEY_D)) {
                        this.MoveRight(this.MovementSpeed, true);
                    }

                    if (Input.IsKeyDown(KeyboardKey.KEY_SPACE)) {
                        this.MoveUp(this.MovementSpeed);
                    }
                    
                    if (Input.IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT)) {
                        this.MoveUp(-this.MovementSpeed);
                    }
                }
                else {
                    this.SetYaw(this.GetYaw() + (Input.GetGamepadAxisMovement(0, GamepadAxis.GAMEPAD_AXIS_RIGHT_X) * 2) * this.MouseSensitivity, false);
                    this.SetPitch(this.GetPitch() - (Input.GetGamepadAxisMovement(0, GamepadAxis.GAMEPAD_AXIS_RIGHT_Y) * 2) * this.MouseSensitivity, true, false, false);
                    
                    if (Input.IsGamepadButtonDown(0, GamepadButton.GAMEPAD_BUTTON_RIGHT_TRIGGER_2)) {
                        this.MoveForward(this.MovementSpeed, true);
                    }
                    
                    if (Input.IsGamepadButtonDown(0, GamepadButton.GAMEPAD_BUTTON_LEFT_TRIGGER_2)) {
                        this.MoveForward(-this.MovementSpeed, true);
                    }
                    
                    if (Input.IsGamepadButtonDown(0, GamepadButton.GAMEPAD_BUTTON_RIGHT_TRIGGER_1)) {
                        this.MoveRight(this.MovementSpeed, true);
                    }
                    
                    if (Input.IsGamepadButtonDown(0, GamepadButton.GAMEPAD_BUTTON_LEFT_TRIGGER_1)) {
                        this.MoveRight(-this.MovementSpeed, true);
                    }

                    if (Input.IsGamepadButtonDown(0, GamepadButton.GAMEPAD_BUTTON_RIGHT_FACE_UP)) {
                        this.MoveUp(this.MovementSpeed);
                    }
                    
                    if (Input.IsGamepadButtonDown(0, GamepadButton.GAMEPAD_BUTTON_RIGHT_FACE_DOWN)) {
                        this.MoveUp(-this.MovementSpeed);
                    }
                }
                break;
            
            case CameraMode.CAMERA_ORBITAL:
                Matrix4x4 rotation = Raymath.MatrixRotate(this.Up, -this.OrbitalSpeed * Time.Delta);
                Vector3 view = this.Position - this.Target;
                Vector3 transform = Vector3.Transform(view, rotation);
                this.Position = this.Target + transform;

                if (GuiManager.ActiveGui == null) {
                    this.MoveToTarget(-Input.GetMouseWheelMove());
                }
                break;
            
            case CameraMode.CAMERA_FIRST_PERSON:
                if (GuiManager.ActiveGui != null) return;
                
                if (!Input.IsGamepadAvailable(0)) {
                    this.SetYaw(this.GetYaw() + Input.GetMouseDelta().X * this.MouseSensitivity, false);
                    this.SetPitch(this.GetPitch() - Input.GetMouseDelta().Y * this.MouseSensitivity, true, false, false);
                }
                else {
                    this.SetYaw(this.GetYaw() + (Input.GetGamepadAxisMovement(0, GamepadAxis.GAMEPAD_AXIS_RIGHT_X) * 2) * this.MouseSensitivity, false);
                    this.SetPitch(this.GetPitch() - (Input.GetGamepadAxisMovement(0, GamepadAxis.GAMEPAD_AXIS_RIGHT_Y) * 2) * this.MouseSensitivity, true, false, false);
                }
                break;
            
            case CameraMode.CAMERA_THIRD_PERSON:
                if (GuiManager.ActiveGui != null) return;
                
                if (!Input.IsGamepadAvailable(0)) {
                    this.SetYaw(this.GetYaw() + Input.GetMouseDelta().X * this.MouseSensitivity, true);
                    this.SetPitch(this.GetPitch() - Input.GetMouseDelta().Y * this.MouseSensitivity, true, true, false);
                }
                else {
                    this.SetYaw(this.GetYaw() + (Input.GetGamepadAxisMovement(0, GamepadAxis.GAMEPAD_AXIS_RIGHT_X) * 2) * this.MouseSensitivity, true);
                    this.SetPitch(this.GetPitch() - (Input.GetGamepadAxisMovement(0, GamepadAxis.GAMEPAD_AXIS_RIGHT_Y) * 2) * this.MouseSensitivity, true, true, false);
                }
                break;
        }
    }
    
    /// <inheritdoc cref="Raylib.GetCameraForward(ref Camera3D)"/>
    public Vector3 GetForward() => Raylib.GetCameraForward(ref this._camera3D);
    
    /// <inheritdoc cref="Raylib.GetCameraUp(ref Camera3D)"/>
    public Vector3 GetUp() => Raylib.GetCameraUp(ref this._camera3D);
    
    /// <inheritdoc cref="Raylib.GetCameraRight(ref Camera3D)"/>
    public Vector3 GetRight() => Raylib.GetCameraRight(ref this._camera3D);
    
    /// <inheritdoc cref="Raylib.CameraMoveForward(ref Camera3D, float, CBool)"/>
    public void MoveForward(float distance, bool moveInWorldPlane) => Raylib.CameraMoveForward(ref this._camera3D, distance, moveInWorldPlane);
    
    /// <inheritdoc cref="Raylib.CameraMoveUp(ref Camera3D, float)"/>
    public void MoveUp(float distance) => Raylib.CameraMoveUp(ref this._camera3D, distance);
    
    /// <inheritdoc cref="Raylib.CameraMoveRight(ref Camera3D, float, CBool)"/>
    public void MoveRight(float distance, bool moveInWorldPlane) => Raylib.CameraMoveRight(ref this._camera3D, distance, moveInWorldPlane);
    
    /// <inheritdoc cref="Raylib.CameraMoveToTarget(ref Camera3D, float)"/>
    public void MoveToTarget(float delta) => Raylib.CameraMoveToTarget(ref this._camera3D, delta);

    /// <inheritdoc cref="Raylib.GetCameraViewMatrix(ref Camera3D)"/>
    public Matrix4x4 GetView() => Raylib.GetCameraViewMatrix(ref this._camera3D);
    
    /// <inheritdoc cref="Raylib.GetCameraProjectionMatrix(ref Camera3D, float)"/>
    public Matrix4x4 GetProjection(float aspect) => Raylib.GetCameraProjectionMatrix(ref this._camera3D, aspect);
    
    /// <inheritdoc cref="Raylib.GetCameraMatrix"/>
    public Matrix4x4 GetTransformMatrix() => Raylib.GetCameraMatrix(this._camera3D);
    
    /// <inheritdoc cref="Raylib.GetWorldToScreen"/>
    public Vector2 GetWorldToScreen(Vector3 position) => Raylib.GetWorldToScreen(position, this._camera3D);
    
    /// <inheritdoc cref="Raylib.GetWorldToScreenEx"/>
    public Vector2 GetWorldToScreen(Vector3 position, int width, int height) => Raylib.GetWorldToScreenEx(position, this._camera3D, width, height);
    
    /// <inheritdoc cref="Raylib.GetMouseRay"/>
    public Ray GetMouseRay(Vector2 mousePosition, Camera3D camera) => Raylib.GetMouseRay(mousePosition, camera);
    
    /// <summary>
    /// Gets the yaw (horizontal rotation) of the 3D camera in degrees.
    /// </summary>
    /// <returns>The yaw angle of the camera in degrees.</returns>
    public float GetYaw() {
        return Raymath.QuaternionToEuler(this.Rotation).Y * Raylib.RAD2DEG;
    }

    /// <summary>
    /// Gets the pitch (vertical rotation) of the 3D camera in degrees.
    /// </summary>
    /// <returns>The pitch angle of the camera in degrees.</returns>
    public float GetPitch() {
        return Raymath.QuaternionToEuler(this.Rotation).X * Raylib.RAD2DEG;
    }

    /// <summary>
    /// Gets the roll (tilt or bank) of the 3D camera in degrees.
    /// </summary>
    /// <returns>The roll angle of the camera in degrees.</returns>
    public float GetRoll() {
        return Raymath.QuaternionToEuler(this.Rotation).Z * Raylib.RAD2DEG;
    }

    /// <summary>
    /// Sets the yaw (horizontal rotation) of the 3D camera to the specified angle in degrees.
    /// </summary>
    /// <param name="angle">The target yaw angle in degrees.</param>
    /// <param name="rotateAroundTarget">Specifies whether to rotate around the camera's target position.</param>
    public void SetYaw(float angle, bool rotateAroundTarget) {
        float difference = this.GetYaw() * Raylib.DEG2RAD - angle * Raylib.DEG2RAD;
        
        Raylib.CameraYaw(ref this._camera3D, difference, rotateAroundTarget);
    }
    
    /// <summary>
    /// Sets the pitch (vertical rotation) of the 3D camera to the specified angle in degrees.
    /// </summary>
    /// <param name="angle">The target pitch angle in degrees.</param>
    /// <param name="lockView">Specifies whether to lock the view during the rotation.</param>
    /// <param name="rotateAroundTarget">Specifies whether to rotate around the camera's target position.</param>
    /// <param name="rotateUp">Specifies whether to rotate upwards.</param>
    public void SetPitch(float angle, bool lockView, bool rotateAroundTarget, bool rotateUp) {
        float difference = angle * Raylib.DEG2RAD - this.GetPitch() * Raylib.DEG2RAD;
        
        Raylib.CameraPitch(ref this._camera3D, difference, lockView, rotateAroundTarget, rotateUp);
    }

    /// <summary>
    /// Sets the roll (tilt or bank) of the 3D camera to the specified angle in degrees.
    /// </summary>
    /// <param name="angle">The target roll angle in degrees.</param>
    public void SetRoll(float angle) {
        float difference = this.GetRoll() * Raylib.DEG2RAD - angle * Raylib.DEG2RAD;
        
        Raylib.CameraRoll(ref this._camera3D, difference);
    }
    
    /// <summary>
    /// Retrieves the Frustum associated with the current instance.
    /// </summary>
    /// <returns>The Frustum object.</returns>
    public Frustum GetFrustum() {
        this._frustum.Extract();
        
        return this._frustum;
    }
    
    /// <summary>
    /// Retrieves the internal 3D camera used for rendering.
    /// </summary>
    /// <returns>The Camera3D object representing the current camera settings.</returns>
    internal Camera3D GetCamera3D() {
        return this._camera3D;
    }
    
    /// <summary>
    /// Prepares the rendering context for 3D graphics by configuring matrices, projection, and depth testing.
    /// </summary>
    public void BeginMode3D() {
        Raylib.BeginMode3D(this._camera3D);
    }
    
    /// <summary>
    /// Ends the 3D rendering mode and performs necessary cleanup.
    /// </summary>
    public void EndMode3D() {
        Raylib.EndMode3D();
    }
}