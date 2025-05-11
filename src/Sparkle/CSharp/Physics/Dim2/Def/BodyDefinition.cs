using System.Numerics;
using Box2D;

namespace Sparkle.CSharp.Physics.Dim2.Def;

public struct BodyDefinition {
    
    /// <summary>
    /// The type of the body (e.g., Static, Dynamic, or Kinematic).
    /// </summary>
    public BodyType Type;
    
    /// <summary>
    /// The initial linear velocity of the body in world units per second.
    /// </summary>
    public Vector2 LinearVelocity;
    
    /// <summary>
    /// The initial angular velocity of the body in radians per second.
    /// </summary>
    public float AngularVelocity;
    
    /// <summary>
    /// The linear damping factor, which reduces the linear velocity over time.
    /// </summary>
    public float LinearDamping;
    
    /// <summary>
    /// The angular damping factor, which reduces the angular velocity over time.
    /// </summary>
    public float AngularDamping;
    
    /// <summary>
    /// Scale factor for the gravity applied to this body.
    /// </summary>
    public float GravityScale;
    
    /// <summary>
    /// The minimum speed under which the body can enter the sleep state.
    /// </summary>
    public float SleepThreshold;
    
    /// <summary>
    /// An optional name identifier for the body.
    /// </summary>
    public string? Name;
    
    /// <summary>
    /// Optional user-defined data associated with the body.
    /// </summary>
    public object? UserData;
    
    /// <summary>
    /// Whether the body is allowed to sleep to conserve resources.
    /// </summary>
    public bool EnableSleep;
    
    /// <summary>
    /// Whether the body should start in the awake state.
    /// </summary>
    public bool IsAwake;
    
    /// <summary>
    /// Whether the body should have a fixed rotation, preventing it from rotating.
    /// </summary>
    public bool FixedRotation;
    
    /// <summary>
    /// Whether the body is a bullet (i.e., uses continuous collision detection).
    /// </summary>
    public bool IsBullet;
    
    /// <summary>
    /// Whether the body is enabled and participates in simulation and collisions.
    /// </summary>
    public bool IsEnabled;
    
    /// <summary>
    /// Whether fast rotation is allowed, possibly sacrificing stability for performance.
    /// </summary>
    public bool AllowFastRotation;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="BodyDefinition"/> struct.
    /// </summary>
    public BodyDefinition() {
        this.Type = BodyType.Static;
        this.LinearVelocity = Vector2.Zero;
        this.AngularVelocity = 0.0F;
        this.LinearDamping = 0.0F;
        this.AngularDamping = 0.0F;
        this.GravityScale = 1.0F;
        this.SleepThreshold = 0.05F * Core.LengthUnitsPerMeter;
        this.Name = null;
        this.UserData = null;
        this.EnableSleep = true;
        this.IsAwake = true;
        this.FixedRotation = false;
        this.IsBullet = false;
        this.IsEnabled = true;
        this.AllowFastRotation = false;
    }
}