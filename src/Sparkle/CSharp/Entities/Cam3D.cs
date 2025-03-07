using System.Numerics;
using Raylib_CSharp;
using Raylib_CSharp.Camera.Cam3D;
using Raylib_CSharp.Collision;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Windowing;
using Sparkle.CSharp.GUI;
using Sparkle.CSharp.Rendering;
using Sparkle.CSharp.Scenes;

namespace Sparkle.CSharp.Entities;

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
    public Cam3D(Vector3 position, Vector3 target, Vector3 up, float fov, CameraProjection projection, CameraMode mode = CameraMode.Free) : base(Vector3.Zero) {
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
        this.MovementSpeed = 10.0F;
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
            return RayMath.QuaternionFromMatrix(lookAt);
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
        if (SceneManager.ActiveCam3D != this) return;

        switch (this.Mode) {
            
            case CameraMode.Free:
                if (GuiManager.ActiveGui != null) return;
                
                if (!Input.IsGamepadAvailable(0)) {
                    this.SetYaw(this.GetYaw() + Input.GetMouseDelta().X * this.MouseSensitivity, false);
                    this.SetPitch(this.GetPitch() - Input.GetMouseDelta().Y * this.MouseSensitivity, true, false, false);

                    if (Input.IsKeyDown(KeyboardKey.W)) {
                        this.MoveForward(this.MovementSpeed * Time.GetFrameTime(), true);
                    }
                    
                    if (Input.IsKeyDown(KeyboardKey.S)) {
                        this.MoveForward(-this.MovementSpeed * Time.GetFrameTime(), true);
                    }
                    
                    if (Input.IsKeyDown(KeyboardKey.A)) {
                        this.MoveRight(-this.MovementSpeed * Time.GetFrameTime(), true);
                    }
                    
                    if (Input.IsKeyDown(KeyboardKey.D)) {
                        this.MoveRight(this.MovementSpeed * Time.GetFrameTime(), true);
                    }

                    if (Input.IsKeyDown(KeyboardKey.Space)) {
                        this.MoveUp(this.MovementSpeed * Time.GetFrameTime());
                    }
                    
                    if (Input.IsKeyDown(KeyboardKey.LeftShift)) {
                        this.MoveUp(-this.MovementSpeed * Time.GetFrameTime());
                    }
                }
                else {
                    this.SetYaw(this.GetYaw() + (Input.GetGamepadAxisMovement(0, GamepadAxis.RightX) * 4) * this.MouseSensitivity, false);
                    this.SetPitch(this.GetPitch() - (Input.GetGamepadAxisMovement(0, GamepadAxis.RightY) * 4) * this.MouseSensitivity, true, false, false);
                    
                    if (Input.IsGamepadButtonDown(0, GamepadButton.RightTrigger2)) {
                        this.MoveForward(this.MovementSpeed * Time.GetFrameTime(), true);
                    }
                    
                    if (Input.IsGamepadButtonDown(0, GamepadButton.LeftTrigger2)) {
                        this.MoveForward(-this.MovementSpeed * Time.GetFrameTime(), true);
                    }
                    
                    if (Input.IsGamepadButtonDown(0, GamepadButton.RightTrigger1)) {
                        this.MoveRight(this.MovementSpeed * Time.GetFrameTime(), true);
                    }
                    
                    if (Input.IsGamepadButtonDown(0, GamepadButton.LeftTrigger1)) {
                        this.MoveRight(-this.MovementSpeed * Time.GetFrameTime(), true);
                    }

                    if (Input.IsGamepadButtonDown(0, GamepadButton.RightFaceUp)) {
                        this.MoveUp(this.MovementSpeed * Time.GetFrameTime());
                    }
                    
                    if (Input.IsGamepadButtonDown(0, GamepadButton.RightFaceDown)) {
                        this.MoveUp(-this.MovementSpeed * Time.GetFrameTime());
                    }
                }
                break;
            
            case CameraMode.Orbital:
                Matrix4x4 rotation = RayMath.MatrixRotate(this.Up, -this.OrbitalSpeed * Time.GetFrameTime());
                Vector3 view = this.Position - this.Target;
                Vector3 transform = Vector3.Transform(view, rotation);
                this.Position = this.Target + transform;

                if (GuiManager.ActiveGui == null) {
                    this.MoveToTarget(-Input.GetMouseWheelMove());
                }
                break;
            
            case CameraMode.FirstPerson:
                if (GuiManager.ActiveGui != null) return;
                
                if (!Input.IsGamepadAvailable(0)) {
                    this.SetYaw(this.GetYaw() + Input.GetMouseDelta().X * this.MouseSensitivity, false);
                    this.SetPitch(this.GetPitch() - Input.GetMouseDelta().Y * this.MouseSensitivity, true, false, false);
                }
                else {
                    this.SetYaw(this.GetYaw() + (Input.GetGamepadAxisMovement(0, GamepadAxis.RightX) * 2) * this.MouseSensitivity, false);
                    this.SetPitch(this.GetPitch() - (Input.GetGamepadAxisMovement(0, GamepadAxis.RightY) * 2) * this.MouseSensitivity, true, false, false);
                }
                break;
            
            case CameraMode.ThirdPerson:
                if (GuiManager.ActiveGui != null) return;
                
                if (!Input.IsGamepadAvailable(0)) {
                    this.SetYaw(this.GetYaw() + Input.GetMouseDelta().X * this.MouseSensitivity, true);
                    this.SetPitch(this.GetPitch() - Input.GetMouseDelta().Y * this.MouseSensitivity, true, true, false);
                }
                else {
                    this.SetYaw(this.GetYaw() + (Input.GetGamepadAxisMovement(0, GamepadAxis.RightX) * 2) * this.MouseSensitivity, true);
                    this.SetPitch(this.GetPitch() - (Input.GetGamepadAxisMovement(0, GamepadAxis.RightY) * 2) * this.MouseSensitivity, true, true, false);
                }
                break;
        }
    }

    /// <inheritdoc cref="Camera3D.GetForward"/>
    public Vector3 GetForward() => this._camera3D.GetForward();
    
    /// <inheritdoc cref="Camera3D.GetUp"/>
    public Vector3 GetUp() => this._camera3D.GetUp();
    
    /// <inheritdoc cref="Camera3D.GetRight"/>
    public Vector3 GetRight() => this._camera3D.GetRight();
    
    /// <inheritdoc cref="Camera3D.MoveForward"/>
    public void MoveForward(float distance, bool moveInWorldPlane) => this._camera3D.MoveForward(distance, moveInWorldPlane);
    
    /// <inheritdoc cref="Camera3D.MoveUp"/>
    public void MoveUp(float distance) => this._camera3D.MoveUp(distance);
    
    /// <inheritdoc cref="Camera3D.MoveRight"/>
    public void MoveRight(float distance, bool moveInWorldPlane) => this._camera3D.MoveRight(distance, moveInWorldPlane);
    
    /// <inheritdoc cref="Camera3D.MoveToTarget"/>
    public void MoveToTarget(float delta) => this._camera3D.MoveToTarget(delta);

    /// <inheritdoc cref="Camera3D.GetViewMatrix"/>
    public Matrix4x4 GetViewMatrix() => this._camera3D.GetViewMatrix();
    
    /// <inheritdoc cref="Camera3D.GetProjectionMatrix"/>
    public Matrix4x4 GetProjectionMatrix(float aspect) => this._camera3D.GetProjectionMatrix(aspect);
    
    
    /// <inheritdoc cref="Camera3D.GetWorldToScreen"/>
    public Vector2 GetWorldToScreen(Vector3 position) => this._camera3D.GetWorldToScreen(position);
    
    /// <inheritdoc cref="Camera3D.GetWorldToScreenEx"/>
    public Vector2 GetWorldToScreen(Vector3 position, int width, int height) => this._camera3D.GetWorldToScreenEx(position, width, height);
    
    /// <inheritdoc cref="Camera3D.GetScreenToWorldRay"/>
    public Ray GetMouseRay(Vector2 position) => this._camera3D.GetScreenToWorldRay(position);
    
    /// <inheritdoc cref="Camera3D.GetScreenToWorldRayEx"/>
    public Ray GetMouseRay(Vector2 position, int width, int height) => this._camera3D.GetScreenToWorldRayEx(position, width, height);
    
    /// <inheritdoc cref="Camera3D.GetMatrix"/>
    public Matrix4x4 GetMatrix() => this._camera3D.GetMatrix();
    
    /// <summary>
    /// Gets the yaw (horizontal rotation) of the 3D camera in degrees.
    /// </summary>
    /// <returns>The yaw angle of the camera in degrees.</returns>
    public float GetYaw() {
        return RayMath.QuaternionToEuler(this.Rotation).Y * RayMath.Rad2Deg;
    }

    /// <summary>
    /// Gets the pitch (vertical rotation) of the 3D camera in degrees.
    /// </summary>
    /// <returns>The pitch angle of the camera in degrees.</returns>
    public float GetPitch() {
        return RayMath.QuaternionToEuler(this.Rotation).X * RayMath.Rad2Deg;
    }

    /// <summary>
    /// Gets the roll (tilt or bank) of the 3D camera in degrees.
    /// </summary>
    /// <returns>The roll angle of the camera in degrees.</returns>
    public float GetRoll() {
        return RayMath.QuaternionToEuler(this.Rotation).Z * RayMath.Rad2Deg;
    }

    /// <summary>
    /// Sets the yaw (horizontal rotation) of the 3D camera to the specified angle in degrees.
    /// </summary>
    /// <param name="angle">The target yaw angle in degrees.</param>
    /// <param name="rotateAroundTarget">Specifies whether to rotate around the camera's target position.</param>
    public void SetYaw(float angle, bool rotateAroundTarget) {
        float difference = this.GetYaw() * RayMath.Deg2Rad - angle * RayMath.Deg2Rad;
        
        this._camera3D.RotateYaw(difference, rotateAroundTarget);
    }
    
    /// <summary>
    /// Sets the pitch (vertical rotation) of the 3D camera to the specified angle in degrees.
    /// </summary>
    /// <param name="angle">The target pitch angle in degrees.</param>
    /// <param name="lockView">Specifies whether to lock the view during the rotation.</param>
    /// <param name="rotateAroundTarget">Specifies whether to rotate around the camera's target position.</param>
    /// <param name="rotateUp">Specifies whether to rotate upwards.</param>
    public void SetPitch(float angle, bool lockView, bool rotateAroundTarget, bool rotateUp) {
        float difference = angle * RayMath.Deg2Rad - this.GetPitch() * RayMath.Deg2Rad;
        
        this._camera3D.RotatePitch(difference, lockView, rotateAroundTarget, rotateUp);
    }

    /// <summary>
    /// Sets the roll (tilt or bank) of the 3D camera to the specified angle in degrees.
    /// </summary>
    /// <param name="angle">The target roll angle in degrees.</param>
    public void SetRoll(float angle) {
        float difference = this.GetRoll() * RayMath.Deg2Rad - angle * RayMath.Deg2Rad;
        
        this._camera3D.RotateRoll(difference);
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
    /// Returns the Camera3D object associated with the Cam3D instance.
    /// </summary>
    /// <returns>The Camera3D object associated with the Cam3D instance.</returns>
    public Camera3D GetCamera3D() {
        return this._camera3D;
    }
    
    /// <summary>
    /// Prepares the rendering context for 3D graphics by configuring matrices, projection, and depth testing.
    /// </summary>
    public void BeginMode3D() {
        Graphics.BeginMode3D(this._camera3D);
    }
    
    /// <summary>
    /// Ends the 3D rendering mode and performs necessary cleanup.
    /// </summary>
    public void EndMode3D() {
        Graphics.EndMode3D();
    }
}