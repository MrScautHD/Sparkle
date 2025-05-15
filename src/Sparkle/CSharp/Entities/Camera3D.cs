using System.Numerics;
using Bliss.CSharp.Camera.Dim3;
using Bliss.CSharp.Graphics.Rendering;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Scenes;

namespace Sparkle.CSharp.Entities;

public class Camera3D : Entity {
    
    /// <summary>
    /// The internal camera instance used for handling camera logic.
    /// </summary>
    private Cam3D _cam3D;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Camera3D"/> class.
    /// </summary>
    /// <param name="position">The initial position of the camera.</param>
    /// <param name="target">The target point the camera is looking at.</param>
    /// <param name="aspectRatio">The aspect ratio (width / height) of the camera view.</param>
    /// <param name="up">The upward direction vector. Defaults to (0,1,0).</param>
    /// <param name="projectionType">The projection type of the camera (Perspective or Orthographic).</param>
    /// <param name="mode">The movement mode of the camera.</param>
    /// <param name="fov">The field of view in degrees. Should be between 1 and 179.</param>
    /// <param name="nearPlane">The near clipping plane. Must be positive and smaller than <paramref name="farPlane"/>.</param>
    /// <param name="farPlane">The far clipping plane. Must be positive and greater than <paramref name="nearPlane"/>.</param>
    public Camera3D(Vector3 position, Vector3 target, float aspectRatio, Vector3? up = null, ProjectionType projectionType = ProjectionType.Perspective, CameraMode mode = CameraMode.Free, float fov = 70.0F, float nearPlane = 0.1F, float farPlane = 1000.0F) : base(new Transform(), "camera3D") {
        this._cam3D = new Cam3D(position, target, aspectRatio, up, projectionType, mode, fov, nearPlane, farPlane);
    }

    /// <summary>
    /// Gets the transformation of the camera.
    /// </summary>
    public new Transform Transform => new Transform() {
        Translation = this._cam3D.Position,
        Rotation = Quaternion.CreateFromRotationMatrix(this._cam3D.GetView()),
        Scale = Vector3.One
    };
    
    /// <summary>
    /// The position of the camera in world space.
    /// </summary>
    public Vector3 Position {
        get => this._cam3D.Position;
        set => this._cam3D.Position = value;
    }

    /// <summary>
    /// The target position the camera is looking at.
    /// </summary>
    public Vector3 Target {
        get => this._cam3D.Target;
        set => this._cam3D.Target = value;
    }

    /// <summary>
    /// The up direction vector of the camera.
    /// </summary>
    public Vector3 Up {
        get => this._cam3D.Up;
        set => this._cam3D.Up = value;
    }

    /// <summary>
    /// The projection type of the camera.
    /// </summary>
    public ProjectionType ProjectionType {
        get => this._cam3D.ProjectionType;
        set => this._cam3D.ProjectionType = value;
    }

    /// <summary>
    /// The movement mode of the camera.
    /// </summary>
    public CameraMode Mode {
        get => this._cam3D.Mode;
        set => this._cam3D.Mode = value;
    }

    /// <summary>
    /// The field of view of the camera.
    /// </summary>
    public float Fov {
        get => this._cam3D.Fov;
        set => this._cam3D.Fov = value;
    }

    /// <summary>
    /// The near clipping plane distance.
    /// </summary>
    public float NearPlane {
        get => this._cam3D.NearPlane;
        set => this._cam3D.NearPlane = value;
    }

    /// <summary>
    /// The far clipping plane distance.
    /// </summary>
    public float FarPlane {
        get => this._cam3D.FarPlane;
        set => this._cam3D.FarPlane = value;
    }

    /// <summary>
    /// The mouse sensitivity for camera movement.
    /// </summary>
    public float MouseSensitivity {
        get => this._cam3D.MouseSensitivity;
        set => this._cam3D.MouseSensitivity = value;
    }

    /// <summary>
    /// The movement speed of the camera.
    /// </summary>
    public float MovementSpeed {
        get => this._cam3D.MovementSpeed;
        set => this._cam3D.MovementSpeed = value;
    }

    /// <summary>
    /// The orbital speed of the camera.
    /// </summary>
    public float OrbitalSpeed {
        get => this._cam3D.OrbitalSpeed;
        set => this._cam3D.OrbitalSpeed = value;
    }
    
    /// <summary>
    /// The aspect ratio of the camera.
    /// </summary>
    public float AspectRatio => this._cam3D.AspectRatio;

    /// <summary>
    /// Updates the state of the <see cref="Camera3D"/> object each frame.
    /// </summary>
    /// <param name="delta">The time elapsed since the last frame update, in seconds.</param>
    protected internal override void Update(double delta) {
        base.Update(delta);

        // To make sure just the active camera moves from the input.
        if (SceneManager.ActiveCam3D == this) {
            this._cam3D.Update(Time.Delta);
        }
    }

    /// <summary>
    /// Resizes the camera to match the dimensions of the given rectangle.
    /// </summary>
    /// <param name="rectangle">The rectangle defining the new dimensions for the camera's viewport.</param>
    protected internal override void Resize(Rectangle rectangle) {
        base.Resize(rectangle);
        this._cam3D.Resize((uint) rectangle.Width, (uint) rectangle.Height);
    }

    /// <summary>
    /// Begins the camera rendering process.
    /// </summary>
    public void Begin() {
        this._cam3D.Begin();
    }

    /// <summary>
    /// Ends the camera rendering process.
    /// </summary>
    public void End() {
        this._cam3D.End();
    }

    /// <summary>
    /// Gets the projection matrix of the camera.
    /// </summary>
    public Matrix4x4 GetProjection() {
        return this._cam3D.GetProjection();
    }

    /// <summary>
    /// Gets the view matrix of the camera.
    /// </summary>
    public Matrix4x4 GetView() {
        return this._cam3D.GetView();
    }

    /// <summary>
    /// Gets the forward direction vector of the camera.
    /// </summary>
    public Vector3 GetForward() {
        return this._cam3D.GetForward();
    }

    /// <summary>
    /// Moves the camera forward.
    /// </summary>
    /// <param name="distance">The distance to move forward.</param>
    /// <param name="moveInWorldPlane">Whether to move in the world plane.</param>
    public void MoveForward(float distance, bool moveInWorldPlane) {
        this._cam3D.MoveForward(distance, moveInWorldPlane);
    }

    /// <summary>
    /// Gets the right direction vector of the camera.
    /// </summary>
    public Vector3 GetRight() {
        return this._cam3D.GetRight();
    }

    /// <summary>
    /// Moves the camera to the right.
    /// </summary>
    /// <param name="distance">The distance to move right.</param>
    /// <param name="moveInWorldPlane">Whether to move in the world plane.</param>
    public void MoveRight(float distance, bool moveInWorldPlane) {
        this._cam3D.MoveRight(distance, moveInWorldPlane);
    }

    /// <summary>
    /// Moves the camera upwards.
    /// </summary>
    /// <param name="distance">The distance to move up.</param>
    public void MoveUp(float distance) {
        this._cam3D.MoveUp(distance);
    }

    /// <summary>
    /// Moves the camera closer to the target.
    /// </summary>
    /// <param name="distance">The distance to move.</param>
    public void MoveToTarget(float distance) {
        this._cam3D.MoveToTarget(distance);
    }

    /// <summary>
    /// Gets the current rotation of the camera.
    /// </summary>
    public Vector3 GetRotation() {
        return this._cam3D.GetRotation();
    }

    /// <summary>
    /// Gets the yaw (rotation around the up axis) of the camera.
    /// </summary>
    public float GetYaw() {
        return this._cam3D.GetYaw();
    }

    /// <summary>
    /// Sets the yaw (rotation around the up axis) of the camera.
    /// </summary>
    /// <param name="angle">The angle of rotation.</param>
    /// <param name="rotateAroundTarget">Whether to rotate around the target.</param>
    public void SetYaw(float angle, bool rotateAroundTarget) {
        this._cam3D.SetYaw(angle, rotateAroundTarget);
    }

    /// <summary>
    /// Gets the pitch (rotation around the right axis) of the camera.
    /// </summary>
    public float GetPitch() {
        return this._cam3D.GetPitch();
    }

    /// <summary>
    /// Sets the pitch (rotation around the right axis) of the camera.
    /// </summary>
    /// <param name="angle">The angle of rotation.</param>
    /// <param name="rotateAroundTarget">Whether to rotate around the target.</param>
    public void SetPitch(float angle, bool rotateAroundTarget) {
        this._cam3D.SetPitch(angle, rotateAroundTarget);
    }

    /// <summary>
    /// Gets the roll (rotation around the forward axis) of the camera.
    /// </summary>
    public float GetRoll() {
        return this._cam3D.GetRoll();
    }
    
    /// <summary>
    /// Sets the roll (rotation around the forward axis) of the camera.
    /// </summary>
    /// <param name="angle">The angle of rotation.</param>
    public void SetRoll(float angle) {
        this._cam3D.SetRoll(angle);
    }

    /// <summary>
    /// Gets the camera's frustum, used for visibility and culling calculations.
    /// </summary>
    public Frustum GetFrustum() {
        return this._cam3D.GetFrustum();
    }

    /// <summary>
    /// Converts a screen-space position to world space coordinates.
    /// </summary>
    /// <param name="position">The screen-space position.</param>
    /// <returns>The corresponding world-space position.</returns>
    public Vector3 GetScreenToWorld(Vector2 position) {
        return this._cam3D.GetScreenToWorld(position);
    }

    /// <summary>
    /// Converts a screen-space position to world space coordinates.
    /// </summary>
    /// <param name="position">The screen-space position.</param>
    /// <returns>The corresponding world-space position.</returns>
    public Vector2 GetWorldToScreen(Vector3 position) {
        return this._cam3D.GetWorldToScreen(position);
    }
    
    /// <summary>
    /// Gets the underlying <see cref="Cam3D"/> instance used for camera logic.
    /// </summary>
    /// <returns>The internal <see cref="Cam3D"/> instance.</returns>
    public Cam3D GetCamera3D() {
        return this._cam3D;
    }
}