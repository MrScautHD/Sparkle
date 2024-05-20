using System.Numerics;
using Box2D.NetStandard.Dynamics.Bodies;

namespace Sparkle.CSharp.Physics.Dim2.Def;

public struct BodyDefinition {

    public bool Enabled;
    public bool AllowSleep;
    public bool Awake;
    public bool Bullet;
    public bool FixedRotation;
    
    public float GravityScale;
    public float LinearDamping;
    public float AngularDamping;
    public float AngularVelocity;
    
    public Vector2 LinearVelocity;
    public BodyType Type;

    /// <summary>
    /// Represents the definition of a physics body in a 2D physics simulation.
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