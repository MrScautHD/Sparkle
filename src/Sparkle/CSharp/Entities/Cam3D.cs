using System.Numerics;
using Raylib_cs;
using Sparkle.CSharp.GUI;
using Sparkle.CSharp.Rendering;
using Sparkle.CSharp.Rendering.Renderers;

namespace Sparkle.CSharp.Entities;

public class Cam3D : Entity {

    private Camera3D _camera3D;
    private Frustum _frustum;
    
    public CameraMode Mode;
    public Skybox? Skybox { get; private set; }
    
    public float MouseSensitivity;
    public float MovementSpeed;
    public float OrbitalSpeed;

    /// <summary>
    /// Represents a 3D camera for rendering 3D scenes.
    /// </summary>
    public Cam3D(Vector3 position, Vector3 target, Vector3 up, float fov, CameraProjection projection, CameraMode mode = CameraMode.Free, Skybox? skybox = default) : base(Vector3.Zero) {
        this.Tag = "camera3D";
        this._camera3D = new Camera3D();
        this._frustum = new Frustum();
        this.Position = position;
        this.Target = target;
        this.Up = up;
        this.Fov = fov;
        this.Projection = projection;
        this.Mode = mode;
        this.Skybox = skybox;
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

    protected internal override void Init() {
        base.Init();
        
        if (this.Skybox != null && !this.Skybox.HasInitialized) {
            this.Skybox.Init();
        }
    }

    protected internal override void Update() {
        base.Update();

        switch (this.Mode) {
            
            case CameraMode.Free:
                if (GuiManager.ActiveGui != null) return;
                
                if (!Input.IsGamepadAvailable(0)) {
                    this.SetYaw(this.GetYaw() + Input.GetMouseDelta().X * this.MouseSensitivity, false);
                    this.SetPitch(this.GetPitch() - Input.GetMouseDelta().Y * this.MouseSensitivity, true, false, false);

                    if (Input.IsKeyDown(KeyboardKey.W)) {
                        this.MoveForward(this.MovementSpeed * Time.Delta, true);
                    }
                    
                    if (Input.IsKeyDown(KeyboardKey.S)) {
                        this.MoveForward(-this.MovementSpeed * Time.Delta, true);
                    }
                    
                    if (Input.IsKeyDown(KeyboardKey.A)) {
                        this.MoveRight(-this.MovementSpeed * Time.Delta, true);
                    }
                    
                    if (Input.IsKeyDown(KeyboardKey.D)) {
                        this.MoveRight(this.MovementSpeed * Time.Delta, true);
                    }

                    if (Input.IsKeyDown(KeyboardKey.Space)) {
                        this.MoveUp(this.MovementSpeed * Time.Delta);
                    }
                    
                    if (Input.IsKeyDown(KeyboardKey.LeftShift)) {
                        this.MoveUp(-this.MovementSpeed * Time.Delta);
                    }
                }
                else {
                    this.SetYaw(this.GetYaw() + (Input.GetGamepadAxisMovement(0, GamepadAxis.RightX) * 2) * this.MouseSensitivity, false);
                    this.SetPitch(this.GetPitch() - (Input.GetGamepadAxisMovement(0, GamepadAxis.RightY) * 2) * this.MouseSensitivity, true, false, false);
                    
                    if (Input.IsGamepadButtonDown(0, GamepadButton.RightTrigger2)) {
                        this.MoveForward(this.MovementSpeed * Time.Delta, true);
                    }
                    
                    if (Input.IsGamepadButtonDown(0, GamepadButton.LeftTrigger2)) {
                        this.MoveForward(-this.MovementSpeed * Time.Delta, true);
                    }
                    
                    if (Input.IsGamepadButtonDown(0, GamepadButton.RightTrigger1)) {
                        this.MoveRight(this.MovementSpeed * Time.Delta, true);
                    }
                    
                    if (Input.IsGamepadButtonDown(0, GamepadButton.LeftTrigger1)) {
                        this.MoveRight(-this.MovementSpeed * Time.Delta, true);
                    }

                    if (Input.IsGamepadButtonDown(0, GamepadButton.RightFaceUp)) {
                        this.MoveUp(this.MovementSpeed * Time.Delta);
                    }
                    
                    if (Input.IsGamepadButtonDown(0, GamepadButton.RightFaceDown)) {
                        this.MoveUp(-this.MovementSpeed * Time.Delta);
                    }
                }
                break;
            
            case CameraMode.Orbital:
                Matrix4x4 rotation = Raymath.MatrixRotate(this.Up, -this.OrbitalSpeed * Time.Delta);
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

    protected internal override void Draw() {
        base.Draw();
        this.Skybox?.Draw();
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
    /// Sets the skybox for the 3D camera.
    /// </summary>
    /// <param name="skybox">The skybox to set.</param>
    public void SetSkybox(Skybox? skybox) {
        this.Skybox?.Dispose();
        this.Skybox = skybox;
        
        if (this.Skybox != null && !this.Skybox.HasInitialized) {
            this.Skybox.Init();
        }
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

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (disposing) {
            this.Skybox?.Dispose();
        }
    }
}