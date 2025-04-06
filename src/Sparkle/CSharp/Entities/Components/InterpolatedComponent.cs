using System.Numerics;

namespace Sparkle.CSharp.Entities.Components;

public abstract class InterpolatedComponent : Component {
    
    /// <summary>
    /// Interpolated local position.
    /// </summary>
    public Vector3 LerpedPosition { get; private set; }
    
    /// <summary>
    /// Interpolated global position.
    /// </summary>
    public Vector3 LerpedGlobalPosition { get; private set; }
    
    /// <summary>
    /// Interpolated rotation.
    /// </summary>
    public Quaternion LerpedRotation { get; private set; }
    
    /// <summary>
    /// Interpolated scale between.
    /// </summary>
    public Vector3 LerpedScale { get; private set; }
    
    /// <summary>
    /// The position of the component during the previous fixed update.
    /// </summary>
    private Vector3 _previousPos;
    
    /// <summary>
    /// The position of the component during the current fixed update.
    /// </summary>
    private Vector3 _currentPos;
    
    /// <summary>
    /// The global position of the component during the previous fixed update.
    /// </summary>
    private Vector3 _previousGlobalPos;
    
    /// <summary>
    /// The global position of the component during the current fixed update.
    /// </summary>
    private Vector3 _currentGlobalPos;
    
    /// <summary>
    /// The rotation of the component during the previous fixed update.
    /// </summary>
    private Quaternion _previousRot;
    
    /// <summary>
    /// The rotation of the component during the current fixed update.
    /// </summary>
    private Quaternion _currentRot;

    /// <summary>
    /// The scale of the component during the previous fixed update.
    /// </summary>
    private Vector3 _previousScale;
    
    /// <summary>
    /// The scale of the component during the current fixed update.
    /// </summary>
    private Vector3 _currentScale;

    /// <summary>
    /// The time accumulated since the last fixed update, used for interpolation.
    /// </summary>
    private double _accumulator;

    /// <summary>
    /// Initializes a new instance of the <see cref="InterpolatedComponent"/> class.
    /// </summary>
    /// <param name="offsetPos">The offset position relative to the entity's transform.</param>
    protected InterpolatedComponent(Vector3 offsetPos) : base(offsetPos) { }

    /// <summary>
    /// Updates the component's state based on the provided time delta.
    /// </summary>
    /// <param name="delta">The time delta since the last update, typically in seconds.</param>
    protected internal override void Update(double delta) {
        base.Update(delta);
        
        // Lerp factor.
        float lerpFactor = (float) (this._accumulator / Time.FixedStep);
        
        // Calculate lerped Pos, Rotation and scale.
        this.LerpedPosition = Vector3.Lerp(this._previousPos, this._currentPos, lerpFactor);
        this.LerpedGlobalPosition = Vector3.Lerp(this._previousGlobalPos, this._currentGlobalPos, lerpFactor);
        this.LerpedRotation = Quaternion.Slerp(this._previousRot, this._currentRot, lerpFactor);
        this.LerpedScale = Vector3.Lerp(this._previousScale, this._currentScale, lerpFactor);
        
        // Calculate accumulator.
        this._accumulator += delta;

        while (this._accumulator >= Time.FixedStep) {
            this._accumulator -= Time.FixedStep;
        }
    }

    /// <summary>
    /// Updates the interpolated component's state in fixed intervals, aligning the current and previous
    /// transform properties (position, rotation, and scale) with the associated entity's state.
    /// </summary>
    /// <param name="fixedStep">The fixed time step duration, typically used for physics updates.</param>
    protected internal override void FixedUpdate(double fixedStep) {
        base.FixedUpdate(fixedStep);
        this._previousPos = this._currentPos;
        this._currentPos = this.Entity.Transform.Translation;
        
        this._previousGlobalPos = this._currentGlobalPos;
        this._currentGlobalPos = this.GlobalPos;

        this._previousRot = this._currentRot;
        this._currentRot = this.Entity.Transform.Rotation;

        this._previousScale = this._currentScale;
        this._currentScale = this.Entity.Transform.Scale;
    }
}