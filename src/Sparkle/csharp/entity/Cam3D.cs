using System.Numerics;
using Raylib_cs;

namespace Sparkle.csharp.entity; 

public class Cam3D : Entity {

    private Camera3D _camera3D;
    
    public CameraMode Mode;
    
    public Cam3D(Vector3 position, Vector3 target, Vector3 up, float fov, CameraProjection projection, CameraMode mode = CameraMode.CAMERA_FREE) : base(Vector3.Zero) {
        this.Tag = "camera3D";
        this._camera3D = new Camera3D();
        this.Position = position;
        this.Target = target;
        this.Up = up;
        this.Fov = fov;
        this.Projection = projection;
        this.Mode = mode;
    }
    
    /// <summary>
    /// Gets or sets the position of the 3D camera in 3D space.
    /// </summary>
    public new Vector3 Position {
        get => this._camera3D.position;
        set => this._camera3D.position = value;
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
        get => this._camera3D.target;
        set => this._camera3D.target = value;
    }
    
    /// <summary>
    /// Gets or sets the up direction vector of the 3D camera.
    /// </summary>
    public Vector3 Up {
        get => this._camera3D.up;
        set => this._camera3D.up = value;
    }

    /// <summary>
    /// Gets or sets the vertical field of view (FOV) angle of the 3D camera.
    /// </summary>
    public float Fov {
        get => this._camera3D.fovy;
        set => this._camera3D.fovy = value;
    }
    
    /// <summary>
    /// Gets or sets the projection type of the 3D camera.
    /// </summary>
    public CameraProjection Projection {
        get => this._camera3D.projection;
        set => this._camera3D.projection = value;
    }
    
    protected internal override void Update() {
        base.Update();

        if (this.Mode != CameraMode.CAMERA_CUSTOM) {
            Raylib.UpdateCamera(ref this._camera3D, this.Mode);
        }
    }
    
    /// <summary>
    /// Retrieves the forward direction vector of the 3D camera.
    /// </summary>
    /// <returns>The forward direction vector of the camera in 3D space.</returns>
    public unsafe Vector3 GetForward() {
        fixed (Camera3D* camera = &this._camera3D) {
            return Raylib.GetCameraForward(camera);
        }
    }
    
    /// <summary>
    /// Retrieves the up direction vector of the 3D camera.
    /// </summary>
    /// <returns>The up direction vector of the camera in 3D space.</returns>
    public unsafe Vector3 GetUp() {
        fixed (Camera3D* camera = &this._camera3D) {
            return Raylib.GetCameraUp(camera);
        }
    }
    
    /// <summary>
    /// Retrieves the right direction vector of the 3D camera.
    /// </summary>
    /// <returns>The right direction vector of the camera in 3D space.</returns>
    public unsafe Vector3 GetRight() {
        fixed (Camera3D* camera = &this._camera3D) {
            return Raylib.GetCameraRight(camera);
        }
    }
    
    /// <summary>
    /// Moves the 3D camera forward by the specified distance.
    /// </summary>
    /// <param name="distance">The distance by which to move the camera forward.</param>
    /// <param name="moveInWorldPlane">Specifies whether to move in the world plane (true) or camera plane (false).</param>
    public unsafe void MoveForward(float distance, bool moveInWorldPlane) {
        fixed (Camera3D* camera = &this._camera3D) {
            Raylib.CameraMoveForward(camera, distance, moveInWorldPlane);
        }
    }
    
    /// <summary>
    /// Moves the 3D camera upward by the specified distance.
    /// </summary>
    /// <param name="distance">The distance by which to move the camera upward.</param>
    public unsafe void MoveUp(float distance) {
        fixed (Camera3D* camera = &this._camera3D) {
            Raylib.CameraMoveUp(camera, distance);
        }
    }
    
    /// <summary>
    /// Moves the 3D camera rightward by the specified distance.
    /// </summary>
    /// <param name="distance">The distance by which to move the camera rightward.</param>
    /// <param name="moveInWorldPlane">Specifies whether to move in the world plane (true) or camera plane (false).</param>
    public unsafe void MoveRight(float distance, bool moveInWorldPlane) {
        fixed (Camera3D* camera = &this._camera3D) {
            Raylib.CameraMoveRight(camera, distance, moveInWorldPlane);
        }
    }

    /// <summary>
    /// Moves the 3D camera towards its target position by a specified delta distance.
    /// </summary>
    /// <param name="delta">The distance by which to move the camera towards its target.</param>
    public unsafe void MoveToTarget(float delta) {
        fixed (Camera3D* camera = &this._camera3D) {
            Raylib.CameraMoveToTarget(camera, delta);
        }
    }

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
    public unsafe void SetYaw(float angle, bool rotateAroundTarget) {
        float difference = this.GetYaw() * Raylib.DEG2RAD - angle * Raylib.DEG2RAD;
        
        fixed (Camera3D* camera = &this._camera3D) {
            Raylib.CameraYaw(camera, difference, rotateAroundTarget);
        }
    }
    
    /// <summary>
    /// Sets the pitch (vertical rotation) of the 3D camera to the specified angle in degrees.
    /// </summary>
    /// <param name="angle">The target pitch angle in degrees.</param>
    /// <param name="lockView">Specifies whether to lock the view during the rotation.</param>
    /// <param name="rotateAroundTarget">Specifies whether to rotate around the camera's target position.</param>
    /// <param name="rotateUp">Specifies whether to rotate upwards.</param>
    public unsafe void SetPitch(float angle, bool lockView, bool rotateAroundTarget, bool rotateUp) {
        float difference = angle * Raylib.DEG2RAD - this.GetPitch() * Raylib.DEG2RAD;
        
        fixed (Camera3D* camera = &this._camera3D) {
            Raylib.CameraPitch(camera, difference, lockView, rotateAroundTarget, rotateUp);
        }
    }

    /// <summary>
    /// Sets the roll (tilt or bank) of the 3D camera to the specified angle in degrees.
    /// </summary>
    /// <param name="angle">The target roll angle in degrees.</param>
    public unsafe void SetRoll(float angle) {
        float difference = this.GetRoll() * Raylib.DEG2RAD - angle * Raylib.DEG2RAD;
        
        fixed (Camera3D* camera = &this._camera3D) {
            Raylib.CameraRoll(camera, difference);
        }
    }

    /// <summary>
    /// Retrieves the view matrix for the 3D camera.
    /// </summary>
    /// <returns>The view matrix for the 3D camera.</returns>
    public unsafe Matrix4x4 GetView() {
        fixed (Camera3D* camera = &this._camera3D) {
            return Raylib.GetCameraViewMatrix(camera);
        }
    }
    
    /// <summary>
    /// Retrieves the projection matrix for the 3D camera with the specified aspect ratio.
    /// </summary>
    /// <param name="aspect">The aspect ratio used for projection.</param>
    /// <returns>The projection matrix for the 3D camera.</returns>
    public unsafe Matrix4x4 GetProjection(float aspect) {
        fixed (Camera3D* camera = &this._camera3D) {
            return Raylib.GetCameraProjectionMatrix(camera, aspect);
        }
    }

    /// <summary>
    /// Retrieves the transformation matrix representing the current 3D camera settings.
    /// </summary>
    /// <returns>The transformation matrix based on the camera's configuration.</returns>
    public Matrix4x4 GetTransformMatrix() {
        return Raylib.GetCameraMatrix(this._camera3D);
    }

    /// <summary>
    /// Converts a world-space 3D position to screen-space using the current 3D camera settings.
    /// </summary>
    /// <param name="position">The world-space 3D position to be converted.</param>
    /// <returns>The screen-space representation of the provided 3D position.</returns>
    public Vector2 GetWorldToScreen(Vector3 position) {
        return Raylib.GetWorldToScreen(position, this._camera3D);
    }
    
    /// <summary>
    /// Converts a world-space 3D position to screen-space using the current 3D camera settings.
    /// </summary>
    /// <param name="position">The world-space 3D position to be converted.</param>
    /// <param name="width">The width of the screen or viewport.</param>
    /// <param name="height">The height of the screen or viewport.</param>
    /// <returns>The screen-space representation of the provided 3D position.</returns>
    public Vector2 GetWorldToScreen(Vector3 position, int width, int height) {
        return Raylib.GetWorldToScreenEx(position, this._camera3D, width, height);
    }
    
    /// <summary>
    /// Retrieves a ray in 3D space based on the mouse position and a specified camera.
    /// </summary>
    /// <param name="mousePosition">The 2D mouse position in screen space.</param>
    /// <param name="camera">The 3D camera used for raycasting.</param>
    /// <returns>A Ray object representing the ray in 3D space.</returns>
    public Ray GetMouseRay(Vector2 mousePosition, Camera3D camera) {
        return Raylib.GetMouseRay(mousePosition, camera);
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