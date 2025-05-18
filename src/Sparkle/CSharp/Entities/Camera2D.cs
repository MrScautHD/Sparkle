using System.Numerics;
using Bliss.CSharp.Camera.Dim2;
using Bliss.CSharp.Transformations;

namespace Sparkle.CSharp.Entities;

public class Camera2D : Entity {

    /// <summary>
    /// The internal camera instance used for handling camera logic.
    /// </summary>
    private Cam2D _cam2D;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Camera2D"/> class.
    /// </summary>
    /// <param name="position">The initial position of the camera.</param>
    /// <param name="target">The target position the camera follows.</param>
    /// <param name="size">The size of the camera viewport.</param>
    /// <param name="mode">The follow mode of the camera.</param>
    /// <param name="offset">Optional offset applied to the camera.</param>
    /// <param name="rotation">Initial rotation of the camera in degrees.</param>
    /// <param name="zoom">Initial zoom level of the camera.</param>
    public Camera2D(Vector2 position, Vector2 target, Rectangle size, CameraFollowMode mode, Vector2? offset = null, float rotation = 0.0F, float zoom = 1.0F) : base(new Transform(), "camera2D") {
        this._cam2D = new Cam2D(position, target, size, mode, offset, rotation, zoom);
    }
    
    /// <summary>
    /// Gets the transform representation of the camera.
    /// </summary>
    public new Transform Transform => new Transform() {
        Translation = new Vector3(this._cam2D.Position, 0),
        Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, float.DegreesToRadians(this._cam2D.Rotation)),
        Scale = Vector3.One
    };

    /// <summary>
    /// Gets or sets the position of the camera.
    /// </summary>
    public Vector2 Position {
        get => this._cam2D.Position;
        set => this._cam2D.Position = value;
    }

    /// <summary>
    /// Gets or sets the target position the camera follows.
    /// </summary>
    public Vector2 Target {
        get => this._cam2D.Target;
        set => this._cam2D.Target = value;
    }

    /// <summary>
    /// Gets the size of the camera viewport.
    /// </summary>
    public Rectangle Size => this._cam2D.Size;

    /// <summary>
    /// Gets or sets the camera follow mode.
    /// </summary>
    public CameraFollowMode Mode {
        get => this._cam2D.Mode;
        set => this._cam2D.Mode = value;
    }

    /// <summary>
    /// Gets or sets the offset applied to the camera position.
    /// </summary>
    public Vector2 Offset {
        get => this._cam2D.Offset;
        set => this._cam2D.Offset = value;
    }

    /// <summary>
    /// Gets or sets the camera rotation in degrees.
    /// </summary>
    public float Rotation {
        get => this._cam2D.Rotation;
        set => this._cam2D.Rotation = value;
    }

    /// <summary>
    /// Gets or sets the zoom level of the camera.
    /// </summary>
    public float Zoom {
        get => this._cam2D.Zoom;
        set => this._cam2D.Zoom = value;
    }

    /// <summary>
    /// Gets or sets the minimum follow speed of the camera.
    /// </summary>
    public float MinFollowSpeed {
        get => this._cam2D.MinFollowSpeed;
        set => this._cam2D.MinFollowSpeed = value;
    }

    /// <summary>
    /// Gets or sets the minimum follow effect length.
    /// </summary>
    public float MinFollowEffectLength {
        get => this._cam2D.MinFollowEffectLength;
        set => this._cam2D.MinFollowEffectLength = value;
    }

    /// <summary>
    /// Gets or sets the fraction follow speed for smoothing camera movement.
    /// </summary>
    public float FractionFollowSpeed {
        get => this._cam2D.FractionFollowSpeed;
        set => this._cam2D.FractionFollowSpeed = value;
    }

    /// <summary>
    /// Updates the state of the <see cref="Camera2D"/> object on each frame.
    /// </summary>
    /// <param name="delta">The time elapsed since the last frame, in seconds.</param>
    protected internal override void Update(double delta) {
        base.Update(delta);
        this._cam2D.Update(Time.Delta);
    }

    /// <summary>
    /// Resizes the camera to match the dimensions of the given rectangle.
    /// </summary>
    /// <param name="rectangle">The rectangle defining the new dimensions for the camera's viewport.</param>
    protected internal override void Resize(Rectangle rectangle) {
        base.Resize(rectangle);
        this._cam2D.Resize((uint) rectangle.Width, (uint) rectangle.Height);
    }

    /// <summary>
    /// Begins rendering with this camera.
    /// </summary>
    public void Begin() {
        this._cam2D.Begin();
    }

    /// <summary>
    /// Ends rendering with this camera.
    /// </summary>
    public void End() {
        this._cam2D.End();
    }

    /// <summary>
    /// Gets the view matrix of the camera.
    /// </summary>
    /// <returns>The view matrix.</returns>
    public Matrix4x4 GetView() {
        return this._cam2D.GetView();
    }

    /// <summary>
    /// Calculates and returns the visible area of the camera based on its current position and viewport size.
    /// </summary>
    /// <returns>A <see cref="RectangleF"/> representing the visible area in world coordinates.</returns>
    public RectangleF GetVisibleArea() {
        return this._cam2D.GetVisibleArea();
    }

    /// <summary>
    /// Converts a screen position to world coordinates.
    /// </summary>
    /// <param name="position">The screen position.</param>
    /// <returns>The corresponding world position.</returns>
    public Vector2 GetScreenToWorld(Vector2 position) {
        return this._cam2D.GetScreenToWorld(position);
    }
    
    /// <summary>
    /// Converts a world position to screen coordinates.
    /// </summary>
    /// <param name="position">The world position.</param>
    /// <returns>The corresponding screen position.</returns>
    public Vector2 GetWorldToScreen(Vector2 position) {
        return this._cam2D.GetWorldToScreen(position);
    }

    /// <summary>
    /// Gets the underlying <see cref="Cam2D"/> instance used for camera logic.
    /// </summary>
    /// <returns>The internal <see cref="Cam2D"/> instance.</returns>
    public Cam2D GetCamera2D() {
        return this._cam2D;
    }
}