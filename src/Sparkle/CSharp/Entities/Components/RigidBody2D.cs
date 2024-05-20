using System.Numerics;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.World;
using Raylib_CSharp;
using Sparkle.CSharp.Physics.Dim2;
using Sparkle.CSharp.Physics.Dim2.Def;
using Sparkle.CSharp.Scenes;
using FixtureDef = Box2D.NetStandard.Dynamics.Fixtures.FixtureDef;

namespace Sparkle.CSharp.Entities.Components;

public class RigidBody2D : Component {

    public World World => ((Simulation2D) SceneManager.Simulation!).World;
    public Body Body { get; private set; }

    private BodyDefinition _bodyDefinition;
    private FixtureDefinition _fixtureDefinition;
    
    public RigidBody2D(BodyDefinition bodyDefinition, FixtureDefinition fixtureDefinition) : base(Vector3.Zero) {
        this._bodyDefinition = bodyDefinition;
        this._fixtureDefinition = fixtureDefinition;
    }

    protected internal override void Init() {
        base.Init();
        this.CreateBody();
    }

    protected internal override void AfterUpdate() {
        base.AfterUpdate();
        this.UpdateBodyPositionAndRot();
    }

    protected internal override void FixedUpdate() {
        base.FixedUpdate();
        this.UpdateEntityPositionAndRot();
    }
    
    /// <summary>
    /// Creates the body for the rigid body using the specified definition and shape.
    /// </summary>
    private void CreateBody() {
        this.Body = this.World.CreateBody(new BodyDef() {
            enabled = this._bodyDefinition.Enabled,
            allowSleep = this._bodyDefinition.AllowSleep,
            awake = this._bodyDefinition.Awake,
            bullet = this._bodyDefinition.Bullet,
            fixedRotation = this._bodyDefinition.FixedRotation,
            gravityScale = this._bodyDefinition.GravityScale,
            linearDamping = this._bodyDefinition.LinearDamping,
            angularDamping = this._bodyDefinition.AngularDamping,
            angularVelocity = this._bodyDefinition.AngularVelocity,
            linearVelocity = this._bodyDefinition.LinearVelocity,
            type = this._bodyDefinition.Type,
            angle = RayMath.QuaternionToEuler(this.Entity.Rotation).Z * RayMath.Rad2Deg,
            position = new Vector2(this.Entity.Position.X, this.Entity.Position.Y)
        });
        
        this.Body.CreateFixture(new FixtureDef() {
            shape = this._fixtureDefinition.Shape,
            filter = this._fixtureDefinition.Filter,
            isSensor = this._fixtureDefinition.IsSensor,
            density = this._fixtureDefinition.Density,
            friction = this._fixtureDefinition.Friction,
            restitution = this._fixtureDefinition.Restitution
        });
    }
    
    /// <summary>
    /// Update the position of the entity based on the position of the rigid body.
    /// </summary>
    private void UpdateEntityPositionAndRot() {
        if (this.Body.IsAwake()) {
            this.Entity.Position = new Vector3(this.Body.Position.X, this.Body.Position.Y, 0);
            this.Entity.Rotation = Quaternion.CreateFromYawPitchRoll(0, 0, this.Body.GetTransform().GetAngle());
        }
    }

    /// <summary>
    /// Updates the position of the rigid body to match the Entity's position.
    /// </summary>
    private void UpdateBodyPositionAndRot() {
        Vector2 entityPos = new Vector2(this.Entity.Position.X, this.Entity.Position.Y);

        if (this.Body.Position != entityPos) {
            this.Body.SetTransform(entityPos, RayMath.QuaternionToEuler(this.Entity.Rotation).Z * RayMath.Rad2Deg);
        }
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            this.World.DestroyBody(this.Body);
        }
    }
}