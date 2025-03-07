using System.Numerics;
using Box2D.NetStandard.Dynamics.Bodies;

namespace Sparkle.CSharp.Physics.Dim2.Def;

public struct BodyDefinition {

    /// <summary>
    /// Indicates if the body is enabled.
    /// </summary>
    public bool Enabled;
    
    /// <summary>
    /// Indicates if the body is allowed to sleep.
    /// </summary>
    public bool AllowSleep;
    
    /// <summary>
    /// Indicates if the body is awake.
    /// </summary>
    public bool Awake;
    
    /// <summary>
    /// Specifies if the body is a bullet (moves through other objects).
    /// </summary>
    public bool Bullet;
    
    /// <summary>
    /// Specifies if the body has a fixed rotation.
    /// </summary>
    public bool FixedRotation;
    
    /// <summary>
    /// The scale of gravity applied to the body.
    /// </summary>
    public float GravityScale;
    
    /// <summary>
    /// The linear damping of the body (resistance to movement).
    /// </summary>
    public float LinearDamping;
    
    /// <summary>
    /// The angular damping of the body (resistance to rotation).
    /// </summary>
    public float AngularDamping;
    
    /// <summary>
    /// The angular velocity of the body.
    /// </summary>
    public float AngularVelocity;
    
    /// <summary>
    /// The linear velocity of the body.
    /// </summary>
    public Vector2 LinearVelocity;
    
    /// <summary>
    /// The <see cref="BodyType"/> of the body (e.g., static, dynamic).
    /// </summary>
    public BodyType Type;

    /// <summary>
    /// Initializes a new instance of the <see cref="BodyDefinition"/> struct with default values.
    /// </summary>
    public BodyDefinition() {
        this.Enabled = true;
        this.AllowSleep = true;
        this.Awake = true;
        this.Bullet = false;
        this.FixedRotation = false;
        this.GravityScale = 1.0F;
        this.LinearDamping = 0.0F;
        this.AngularDamping = 0.0F;
        this.AngularVelocity = 0.0F;
        this.LinearVelocity = Vector2.Zero;
        this.Type = BodyType.Dynamic;
    }
}