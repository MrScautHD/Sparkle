using System.Numerics;
using Box2D.NetStandard.Collision.Shapes;
using Box2D.NetStandard.Common;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Contacts;
using Box2D.NetStandard.Dynamics.Fixtures;
using Box2D.NetStandard.Dynamics.Joints;
using Box2D.NetStandard.Dynamics.World;
using Raylib_CSharp;
using Sparkle.CSharp.Physics.Dim2;
using Sparkle.CSharp.Physics.Dim2.Def;
using Sparkle.CSharp.Scenes;

namespace Sparkle.CSharp.Entities.Components;

public class RigidBody2D : Component {

    public World World => ((Simulation2D) SceneManager.Simulation!).World;
    public Body Body { get; private set; }

    private BodyDefinition _bodyDefinition;
    private FixtureDefinition _fixtureDefinition;
    
    /// <summary>
    /// Constructor for creating a RigidBody2D object.
    /// </summary>
    /// <param name="bodyDefinition">The body definition for the rigid body.</param>
    /// <param name="fixtureDefinition">The fixture definition for the rigid body.</param>
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
        
        this.UpdateBodyPosition();
        this.UpdateBodyRotation();
    }

    protected internal override void FixedUpdate() {
        base.FixedUpdate();
        
        this.UpdateEntityPosition();
        this.UpdateEntityRotation();
    }
    
    public override Component Clone() {
        return new RigidBody2D(this._bodyDefinition, this._fixtureDefinition);
    }
    
    /// <summary>
    /// Creates the body for the rigid body component.
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
            angle = RayMath.QuaternionToEuler(this.Entity.Rotation).Z,
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
    private void UpdateEntityPosition() {
        if (this.Body.IsAwake()) {
            this.Entity.Position = new Vector3(this.Body.Position.X, this.Body.Position.Y, 0);
        }
    }

    /// <summary>
    /// Updates the position of the RigidBody to match the Entity's position.
    /// </summary>
    private void UpdateBodyPosition() {
        Vector2 entityPos = new Vector2(this.Entity.Position.X, this.Entity.Position.Y);
        
        if (this.GetPosition() != entityPos) {
            this.SetTransform(entityPos, 0);
        }
    }

    /// <summary>
    /// Updates the rotation of the entity based on the rotation of the rigid body.
    /// </summary>
    private void UpdateEntityRotation() {
        if (this.IsAwake()) {
            this.Entity.Rotation = Quaternion.CreateFromYawPitchRoll(0, 0, this.Body.GetAngle());
        }
    }
    
    /// <summary>
    /// Updates the rotation of the RigidBody to match the Entity's rotation.
    /// </summary>
    private void UpdateBodyRotation() {
        float entityRot = RayMath.QuaternionToEuler(this.Entity.Rotation).Z;
        
        if (RayMath.FloatEquals(entityRot, this.GetAngle()) == 0) {
            this.SetTransform(this.GetPosition(), entityRot);
        }
    }

    /// <summary>
    /// Returns the next body in the world. This is used to iterate over all bodies
    /// </summary>
    /// <returns>The next body in the world</returns>
    public Body GetNext() {
        return this.Body.GetNext();
    }

    /// <summary>
    /// Gets the first joint in the joint list of the RigidBody.
    /// </summary>
    /// <returns>The first joint in the joint list.</returns>
    public JointEdge GetJointList() {
        return this.Body.GetJointList();
    }

    /// <summary>
    /// Retrieves the contact list of the RigidBody.
    /// </summary>
    /// <returns>The first contact in the contact list.</returns>
    public ContactEdge GetContactList() {
        return this.Body.GetContactList();
    }

    /// <summary>
    /// Checks if the given RigidBody has the specified flag.
    /// </summary>
    /// <param name="flag">The RigidBody flag to check for.</param>
    /// <returns> Returns true if the RigidBody has the specified flag; otherwise, false. </returns>
    public bool HasFlag(BodyFlags flag) {
        return this.Body.HasFlag(flag);
    }

    /// <summary>
    /// Returns the world center of the RigidBody.
    /// </summary>
    /// <returns>The world center of the RigidBody.</returns>
    public Vector2 GetWorldCenter() {
        return this.Body.GetWorldCenter();
    }

    /// <summary>
    /// Returns the local center of mass of the RigidBody.
    /// </summary>
    /// <returns>The local center of mass of the RigidBody.</returns>
    public Vector2 GetLocalCenter() {
        return this.Body.GetLocalCenter();
    }

    /// <summary>
    /// Gets the world point corresponding to the given local point.
    /// </summary>
    /// <param name="localPoint">The local point.</param>
    /// <returns>The corresponding world point.</returns>
    public Vector2 GetWorldPoint(Vector2 localPoint) {
        return this.Body.GetWorldPoint(localPoint);
    }

    /// <summary>
    /// Gets the world vector given a local vector.
    /// </summary>
    /// <param name="localVector">The local vector.</param>
    /// <returns>The world vector corresponding to the given local vector.</returns>
    public Vector2 GetWorldVector(Vector2 localVector) {
        return this.Body.GetWorldVector(localVector);
    }

    /// <summary>
    /// Returns the local point of the given world point.
    /// </summary>
    /// <param name="worldPoint">The world point to convert to local point.</param>
    /// <returns>The local point of the given world point.</returns>
    public Vector2 GetLocalPoint(Vector2 worldPoint) {
        return this.Body.GetLocalPoint(worldPoint);
    }

    /// <summary>
    /// Gets the local vector from the given world vector.
    /// </summary>
    /// <param name="worldVector">The world vector.</param>
    /// <returns>The local vector.</returns>
    public Vector2 GetLocalVector(Vector2 worldVector) {
        return this.Body.GetLocalVector(worldVector);
    }

    /// <summary>
    /// Returns the linear velocity of a point on the body in world coordinates.
    /// </summary>
    /// <param name="worldPoint">The point in world coordinates.</param>
    /// <returns>The linear velocity of the point in world coordinates.</returns>
    public Vector2 GetLinearVelocityFromWorldPoint(Vector2 worldPoint) {
        return this.Body.GetLinearVelocityFromWorldPoint(worldPoint);
    }

    /// <summary>
    /// Returns the linear velocity of a point on the body in local coordinates.
    /// </summary>
    /// <param name="localPoint">The point coordinates in local space.</param>
    /// <returns>The linear velocity vector of the point.</returns>
    public Vector2 GetLinearVelocityFromLocalPoint(Vector2 localPoint) {
        return this.Body.GetLinearVelocityFromLocalPoint(localPoint);
    }

    /// <summary>
    /// Determines whether the RigidBody is awake or not.
    /// </summary>
    /// <returns>Returns true if the RigidBody is awake, false otherwise.</returns>
    public bool IsAwake() {
        return this.Body.IsAwake();
    }

    /// <summary>
    /// Sets the awake state of the RigidBody.
    /// </summary>
    /// <param name="flag">Specifies if the RigidBody should be awake or not.</param>
    public void SetAwake(bool flag) {
        this.Body.SetAwake(flag);
    }

    /// <summary>
    /// Checks whether the RigidBody is enabled or not.
    /// </summary>
    /// <returns>Returns true if the RigidBody is enabled, otherwise false.</returns>
    public bool IsEnabled() {
        return this.Body.IsEnabled();
    }

    /// <summary>
    /// Sets the enabled flag of the RigidBody.
    /// </summary>
    /// <param name="flag">The value indicating whether the RigidBody is enabled.</param>
    public void SetEnabled(bool flag) {
        this.Body.SetEnabled(flag);
    }

    /// <summary>
    /// Determines whether sleeping is allowed for the RigidBody.
    /// </summary>
    /// <returns>True if sleeping is allowed, false otherwise.</returns>
    public bool IsSleepingAllowed() {
        return this.Body.IsSleepingAllowed();
    }

    /// <summary>
    /// Sets whether the RigidBody is allowed to enter sleep mode when not in motion.
    /// </summary>
    /// <param name="flag">A boolean value indicating whether sleep is allowed or not.</param>
    public void SetSleepingAllowed(bool flag) {
        this.Body.SetSleepingAllowed(flag);
    }

    /// <summary>
    /// Gets the type of the RigidBody.
    /// </summary>
    /// <returns>The type of the RigidBody.</returns>
    public BodyType GetBodyType() {
        return this.Body.Type();
    }

    /// <summary>
    /// Sets the body type of the RigidBody.
    /// </summary>
    /// <param name="type">The body type to set.</param>
    public void SetBodyType(BodyType type) {
        this.Body.SetType(type);
    }
    
    /// <summary>
    /// Retrieves the user data associated with the RigidBody.
    /// </summary>
    /// <typeparam name="T">The type of user data.</typeparam>
    /// <returns>The user data associated with the RigidBody.</returns>
    public T GetUserData<T>() {
        return this.Body.GetUserData<T>();
    }
    
    /// <summary>
    /// Sets the user data for the RigidBody.
    /// </summary>
    /// <param name="data">The user data to be set.</param>
    public void SetUserData(object data) {
        this.Body.SetUserData(data);
    }

    /// <summary>
    /// Check whether the RigidBody is set as a bullet.
    /// </summary>
    /// <returns> Return true if the RigidBody is set as a bullet; otherwise, false. </returns>
    public bool IsBullet() {
        return this.Body.IsBullet();
    }

    /// <summary>
    /// Sets the flag indicating if the RigidBody should be treated as a bullet.
    /// </summary>
    /// <param name="flag">True if the RigidBody should be treated as a bullet, false otherwise.</param>
    public void SetBullet(bool flag) {
        this.Body.SetBullet(flag);
    }

    /// <summary>
    /// Gets the gravity scale of the RigidBody.
    /// </summary>
    /// <returns>The gravity scale of the RigidBody.</returns>
    public float GetGravityScale() {
        return this.Body.GetGravityScale();
    }

    /// <summary>
    /// Sets the gravity scale of the RigidBody.
    /// </summary>
    /// <param name="scale">The gravity scale to be set.</param>
    public void SetGravityScale(float scale) {
        this.Body.SetGravityScale(scale);
    }

    /// <summary>
    /// Determines whether the RigidBody has fixed rotation.
    /// </summary>
    /// <returns>True if the RigidBody has fixed rotation, false otherwise.</returns>
    public bool IsFixedRotation() {
        return this.Body.IsFixedRotation();
    }

    /// <summary>
    /// Sets whether the RigidBody is allowed to rotate or not.
    /// </summary>
    /// <param name="flag">True if the RigidBody should be fixed in rotation, false otherwise.</param>
    public void SetFixedRotation(bool flag) {
        this.Body.SetFixedRotation(flag);
    }

    /// <summary>
    /// Gets the mass of the RigidBody.
    /// </summary>
    /// <returns>The mass of the RigidBody.</returns>
    public float GetMass() {
        return this.Body.GetMass();
    }

    /// <summary>
    /// Calculates the inertia of the RigidBody.
    /// </summary>
    /// <returns>The inertia of the RigidBody.</returns>
    public float GetInertia() {
        return this.Body.GetInertia();
    }

    /// <summary>
    /// Gets the mass data of the RigidBody.
    /// </summary>
    /// <param name="data">The mass data of the RigidBody.</param>
    public void GetMassData(out MassData data) {
        this.Body.GetMassData(out data);
    }

    /// <summary>
    /// Sets the mass data of the RigidBody.
    /// </summary>
    /// <param name="data">The mass data to set.</param>
    public void SetMassData(MassData data) {
        this.Body.SetMassData(data);
    }

    /// <summary>
    /// Resets the mass data of the RigidBody.
    /// </summary>
    public void ResetMassData() {
        this.Body.ResetMassData();
    }

    /// <summary>
    /// Gets the first fixture in the fixture list attached to the RigidBody.
    /// </summary>
    /// <returns>The first fixture in the fixture list.</returns>
    public Fixture GetFixtureList() {
        return this.Body.GetFixtureList();
    }

    /// <summary>
    /// Creates a new fixture and attaches it to the RigidBody.
    /// </summary>
    /// <param name="def">The fixture definition for the new fixture.</param>
    /// <returns>The created fixture.</returns>
    public Fixture CreateFixture(FixtureDef def) {
        return this.Body.CreateFixture(def);
    }

    /// <summary>
    /// Creates a fixture and attaches it to the RigidBody.
    /// </summary>
    /// <param name="shape">The shape of the fixture.</param>
    /// <param name="density">The density of the fixture.</param>
    /// <returns>The created fixture.</returns>
    public Fixture CreateFixture(Shape shape, float density = 0.0f) {
        return this.Body.CreateFixture(shape, density);
    }

    /// <summary>
    /// Destroys a fixture attached to the RigidBody.
    /// </summary>
    /// <param name="fixture">The fixture to be destroyed.</param>
    public void DestroyFixture(Fixture fixture) {
        this.Body.DestroyFixture(fixture);
    }

    /// <summary>
    /// Retrieves the position of the RigidBody in world coordinates.
    /// </summary>
    /// <returns>The position of the RigidBody in world coordinates.</returns>
    public Vector2 GetPosition() {
        return this.Body.GetPosition();
    }

    /// <summary>
    /// Gets the angle of the RigidBody in radians.
    /// </summary>
    /// <returns>The angle of the RigidBody in radians.</returns>
    public float GetAngle() {
        return this.Body.GetAngle();
    }

    /// <summary>
    /// Returns the transform of the RigidBody.
    /// </summary>
    /// <returns>The transform of the RigidBody.</returns>
    public Transform GetTransform() {
        return this.Body.GetTransform();
    }

    /// <summary>
    /// Sets the transform of the RigidBody to the specified position and angle.
    /// </summary>
    /// <param name="position">The new position of the RigidBody.</param>
    /// <param name="angle">The new angle of the RigidBody in radians.</param>
    public void SetTransform(Vector2 position, float angle) {
        this.Body.SetTransform(position, angle);
    }

    /// Retrieves the linear velocity of the RigidBody in world coordinates.
    /// @return The linear velocity of the RigidBody in world coordinates.
    /// /
    public Vector2 GetLinearVelocity() {
        return this.Body.GetLinearVelocity();
    }
    
    /// <summary>
    /// Sets the linear velocity of the RigidBody.
    /// </summary>
    /// <param name="v">The new linear velocity of the RigidBody.</param>
    public void SetLinearVelocity(Vector2 v) {
        this.Body.SetLinearVelocity(v);
    }

    /// <summary>
    /// Gets the angular velocity of the RigidBody.
    /// </summary>
    /// <returns>The angular velocity of the RigidBody.</returns>
    public float GetAngularVelocity() {
        return this.Body.GetAngularVelocity();
    }

    /// <summary>
    /// Sets the angular velocity of the RigidBody.
    /// </summary>
    /// <param name="omega">The angular velocity to set (in radians per second).</param>
    public void SetAngularVelocity(float omega) {
        this.Body.SetAngularVelocity(omega);
    }

    /// <summary>
    /// Gets the linear damping value of the RigidBody.
    /// </summary>
    /// <returns>The linear damping value of the RigidBody.</returns>
    public float GetLinearDamping() {
        return this.Body.GetLinearDamping();
    }

    /// <summary>
    /// Sets the linear damping for the RigidBody.
    /// </summary>
    /// <param name="linearDamping">The linear damping value to set.</param>
    public void SetLinearDampling(float linearDamping) {
        this.Body.SetLinearDampling(linearDamping);
    }

    /// <summary>
    /// Gets the angular damping of the RigidBody.
    /// </summary>
    /// <returns>The angular damping of the RigidBody.</returns>
    public float GetAngularDamping() {
        return this.Body.GetAngularDamping();
    }

    /// <summary>
    /// Sets the angular damping of the RigidBody.
    /// </summary>
    /// <param name="angularDamping">The angular damping value to be set.</param>
    public void SetAngularDamping(float angularDamping) {
        this.Body.SetAngularDamping(angularDamping);
    }

    /// <summary>
    /// Applies a force to the RigidBody at a specific point.
    /// </summary>
    /// <param name="force">The force to be applied.</param>
    /// <param name="point">The point at which the force should be applied in world coordinates.</param>
    /// <param name="wake">Indicates whether the RigidBody should be woken up.</param>
    public void ApplyForce(Vector2 force, Vector2 point, bool wake = true) {
        this.Body.ApplyForce(force, point, wake);
    }

    /// <summary>
    /// Applies a force to the center of the RigidBody.
    /// </summary>
    /// <param name="force">The force to be applied to the center of the RigidBody.</param>
    /// <param name="wake">Whether to wake up the RigidBody if it is sleeping (default: true).</param>
    public void ApplyForceToCenter(Vector2 force, bool wake = true) {
        this.Body.ApplyForceToCenter(force, wake);
    }

    /// <summary>
    /// Applies torque to the RigidBody.
    /// </summary>
    /// <param name="torque">The torque to be applied.</param>
    /// <param name="wake">Specifies whether the RigidBody should be woken up or not (default is true).</param>
    public void ApplyTorque(float torque, bool wake = true) {
        this.Body.ApplyTorque(torque, wake);
    }

    /// <summary>
    /// Applies a linear impulse to the RigidBody at a specific point.
    /// </summary>
    /// <param name="impulse">The magnitude and direction of the impulse to apply.</param>
    /// <param name="point">The point on the RigidBody to apply the impulse.</param>
    /// <param name="wake">Specifies whether the RigidBody should be woken up after applying the impulse. Default is true.</param>
    public void ApplyLinearImpulse(Vector2 impulse, Vector2 point, bool wake = true) {
        this.Body.ApplyLinearImpulse(impulse, point, wake);
    }

    /// <summary>
    /// Applies a linear impulse to the center of the RigidBody.
    /// </summary>
    /// <param name="impulse">The impulse to apply in units of mass * length / time.</param>
    /// <param name="wake">Specifies whether to wake up the RigidBody if it is sleeping (default is true).</param>
    public void ApplyLinearImpulseToCenter(Vector2 impulse, bool wake = true) {
        this.Body.ApplyLinearImpulseToCenter(impulse, wake);
    }

    /// <summary>
    /// Applies an angular impulse to the RigidBody.
    /// </summary>
    /// <param name="impulse">The magnitude of the angular impulse to apply.</param>
    /// <param name="wake">Determines if the RigidBody should be woken up.</param>
    public void ApplyAngularImpulse(float impulse, bool wake = true) {
        this.Body.ApplyAngularImpulse(impulse, wake);
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            this.World.DestroyBody(this.Body);
        }
    }
}