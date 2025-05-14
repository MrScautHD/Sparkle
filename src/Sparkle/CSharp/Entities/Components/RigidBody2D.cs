using System.Numerics;
using Box2D;
using Sparkle.CSharp.Physics.Dim2;
using Sparkle.CSharp.Physics.Dim2.Def;
using Sparkle.CSharp.Physics.Dim2.Shapes;
using Sparkle.CSharp.Scenes;
using Vortice.Mathematics;
using BoxTransform = Box2D.Transform;
using Transform = Bliss.CSharp.Transformations.Transform;

namespace Sparkle.CSharp.Entities.Components;

public class RigidBody2D : Component {

    /// <summary>
    /// Gets the default comparer used for sorting bodies.
    /// </summary>
    public static IComparer<Body> DefaultComparer => Body.DefaultComparer;
    
    /// <summary>
    /// Gets the default equality comparer for body instances.
    /// </summary>
    public static IEqualityComparer<Body> DefaultEqualityComparer => Body.DefaultEqualityComparer;
    
    /// <summary>
    /// Gets a list of component types that are incompatible with this component.
    /// </summary>
    public override IReadOnlyList<Type> InCompatibleTypes => [
        typeof(RigidBody3D),
        typeof(SoftBody3D)
    ];
    
    /// <summary>
    /// Gets the current 2D simulation instance used by the scene.
    /// Throws an <see cref="InvalidOperationException"/> if the simulation is not of type <see cref="Simulation2D"/>.
    /// </summary>
    public Simulation2D Simulation => SceneManager.Simulation as Simulation2D ?? throw new InvalidOperationException("The current simulation must be of type Simulation2D.");
    
    /// <summary>
    /// Gets the physics world from the current 2D simulation.
    /// </summary>
    public World World => this.Simulation.World;
    
    /// <summary>
    /// The positional offset of this component, always zero for RigidBody2D.
    /// </summary>
    public new Vector3 OffsetPosition => Vector3.Zero;

    /// <summary>
    /// Gets the internal physics body.
    /// </summary>
    public Body Body => this._body;

    /// <summary>
    /// Gets a value indicating whether the internal body is valid.
    /// </summary>
    public bool IsValid => this._body.Valid;

    /// <summary>
    /// Gets the current world position of the body.
    /// </summary>
    public Vector2 Position => this._body.Position;

    /// <summary>
    /// Gets the rotation of the body.
    /// </summary>
    public Rotation Rotation => this._body.Rotation;

    /// <summary>
    /// Gets the mass of the body.
    /// </summary>
    public float Mass => this._body.Mass;

    /// <summary>
    /// Gets the rotational inertia of the body.
    /// </summary>
    public float RotationalInertia => this._body.RotationalInertia;

    /// <summary>
    /// Gets the local center of mass of the body.
    /// </summary>
    public Vector2 LocalCenterOfMass => this._body.LocalCenterOfMass;

    /// <summary>
    /// Gets the local center of mass of the body.
    /// </summary>
    public Vector2 WorldCenterOfMass => this._body.WorldCenterOfMass;
    
    /// <summary>
    /// Gets the axis-aligned bounding box (AABB) of the body.
    /// </summary>
    public AABB AABB => this._body.AABB;

    /// <summary>
    /// Gets the shapes attached to the body.
    /// </summary>
    public ReadOnlySpan<Shape> Shapes => this._body.Shapes;

    /// <summary>
    /// Gets the joints attached to the body.
    /// </summary>
    public ReadOnlySpan<Joint> Joints => this._body.Joints;

    /// <summary>
    /// Gets the contact data for the body.
    /// </summary>
    public ReadOnlySpan<ContactData> Contacts => this._body.Contacts;

    /// <summary>
    /// Represents the underlying physics body of the rigid body.
    /// </summary>
    private Body _body;

    /// <summary>
    /// Defines the body properties and behavior for a 2D rigid body.
    /// </summary>
    private BodyDefinition _bodyDef;

    /// <summary>
    /// Represents a collection of 2D shapes associated with the rigid body.
    /// </summary>
    private List<IShape2D> _shapes;

    /// <summary>
    /// Indicates whether the synchronization process is currently active (When sync entity with body).
    /// </summary>
    private bool _isSyncing;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="RigidBody2D"/> class with a single collision shape.
    /// </summary>
    /// <param name="bodyDef">The body definition containing physical properties.</param>
    /// <param name="shape">The collision shape to attach to the body.</param>
    public RigidBody2D(BodyDefinition bodyDef, IShape2D shape) : this(bodyDef, [shape]) { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="RigidBody2D"/> class with multiple collision shapes.
    /// </summary>
    /// <param name="bodyDef">The body definition containing physical properties.</param>
    /// <param name="shapes">The list of collision shapes to attach to the body.</param>
    public RigidBody2D(BodyDefinition bodyDef, List<IShape2D> shapes) : base(Vector3.Zero) {
        this._bodyDef = bodyDef;
        this._shapes = shapes;
    }

    /// <summary>
    /// Gets or sets the type of the rigid body, determining its physical behavior (e.g., static, dynamic, or kinematic).
    /// </summary>
    public BodyType Type {
        get => this._body.Type;
        set => this._body.Type = value;
    }

    /// <summary>
    /// Gets or sets the name of the body.
    /// </summary>
    public string? Name {
        get => this._body.Name;
        set => this._body.Name = value;
    }

    /// <summary>
    /// Gets or sets user-defined data associated with the rigid body.
    /// </summary>
    public object? UserData {
        get => this._body.UserData;
        set => this._body.UserData = value;
    }

    /// <summary>
    /// Gets or sets the transformation of the body, including its position and rotation in the world space.
    /// </summary>
    public BoxTransform Transform {
        get => this._body.Transform;
        set => this._body.Transform = value;
    }

    /// <summary>
    /// Gets or sets the linear velocity of the rigid body in 2D space.
    /// </summary>
    public Vector2 LinearVelocity {
        get => this._body.LinearVelocity;
        set => this._body.LinearVelocity = value;
    }

    /// <summary>
    /// Gets or sets the angular velocity of the body in radians per second.
    /// </summary>
    public float AngularVelocity {
        get => this._body.AngularVelocity;
        set => this._body.AngularVelocity = value;
    }

    /// <summary>
    /// Gets or sets the mass data of the rigid body, including mass, center of mass, and rotational inertia.
    /// </summary>
    public MassData MassData {
        get => this._body.MassData;
        set => this._body.MassData = value;
    }

    /// <summary>
    /// Gets or sets the linear damping of the body, which reduces its linear velocity over time.
    /// </summary>
    public float LinearDamping {
        get => this._body.LinearDamping;
        set => this._body.LinearDamping = value;
    }

    /// <summary>
    /// Gets or sets the angular damping of the rigid body, which reduces the angular velocity over time to simulate rotational drag.
    /// </summary>
    public float AngularDamping {
        get => this._body.AngularDamping;
        set => this._body.AngularDamping = value;
    }

    /// <summary>
    /// Gets or sets the factor by which the global gravity is scaled for this body.
    /// </summary>
    public float GravityScale {
        get => this._body.GravityScale;
        set => this._body.GravityScale = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the rigid body is awake.
    /// </summary>
    public bool Awake {
        get => this._body.Awake;
        set => this._body.Awake = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the body can enter a sleep state when idle.
    /// </summary>
    public bool SleepEnabled {
        get => this._body.SleepEnabled;
        set => this._body.SleepEnabled = value;
    }

    /// <summary>
    /// Gets or sets the threshold value that determines the conditions under which the body can enter sleep mode.
    /// </summary>
    public float SleepThreshold {
        get => this._body.SleepThreshold;
        set => this._body.SleepThreshold = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the body is enabled.
    /// </summary>
    public bool Enabled {
        get => this._body.Enabled;
        set => this._body.Enabled = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the body is prevented from rotating.
    /// </summary>
    public bool FixedRotation {
        get => this._body.FixedRotation;
        set => this._body.FixedRotation = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the body should use continuous collision detection.
    /// </summary>
    public bool Bullet {
        get => this._body.Bullet;
        set => this._body.Bullet = value;
    }

    /// <summary>
    /// Initializes the <see cref="RigidBody2D"/> component.
    /// </summary>
    protected internal override void Init() {
        base.Init();
        this.CreateBody();
        this.Simulation.BodyMoved += this.BodyMoving;
        this.Entity.Transform.OnUpdate += this.OnEntityMoving;
    }

    /// <summary>
    /// Converts a point in world coordinates to a point in the local space of the body.
    /// </summary>
    /// <param name="worldPoint">The point in world coordinates to be transformed to local coordinates.</param>
    /// <returns>The transformed point in the local coordinates of the body.</returns>
    public Vector2 GetLocalPoint(Vector2 worldPoint) {
        return this._body.GetLocalPoint(worldPoint);
    }

    /// <summary>
    /// Transforms the specified local point to world coordinates relative to the rigid body.
    /// </summary>
    /// <param name="localPoint">The point in local coordinates to be transformed into world coordinates.</param>
    /// <returns>A <see cref="Vector2"/> representing the transformed point in world coordinates.</returns>
    public Vector2 GetWorldPoint(Vector2 localPoint) {
        return this._body.GetWorldPoint(localPoint);
    }

    /// <summary>
    /// Converts a vector from world coordinates to local coordinates relative to this body.
    /// </summary>
    /// <param name="worldVector">The vector in world coordinates to be converted to local coordinates.</param>
    /// <returns>The converted vector in local coordinates.</returns>
    public Vector2 GetLocalVector(Vector2 worldVector) {
        return this._body.GetLocalVector(worldVector);
    }

    /// <summary>
    /// Converts a given vector from local space to world space.
    /// </summary>
    /// <param name="localVector">The vector defined in the local coordinate system.</param>
    /// <returns>The vector transformed into world space coordinates.</returns>
    public Vector2 GetWorldVector(Vector2 localVector) {
        return this._body.GetWorldVector(localVector);
    }

    /// <summary>
    /// Sets the target transform for the rigid body to interpolate towards over a specified time step.
    /// </summary>
    /// <param name="target">The desired target transform for the rigid body.</param>
    /// <param name="timeStep">The time step over which the rigid body interpolates towards the target transform.</param>
    public void SetTargetTransform(BoxTransform target, float timeStep) {
        this._body.SetTargetTransform(target, timeStep);
    }

    /// <summary>
    /// Calculates the velocity at a given local point on the rigid body.
    /// </summary>
    /// <param name="localPoint">The point in local coordinates for which to calculate the velocity.</param>
    /// <returns>The velocity at the specified local point as a <see cref="Vector2"/>.</returns>
    public Vector2 GetLocalPointVelocity(Vector2 localPoint) {
        return this._body.GetLocalPointVelocity(localPoint);
    }

    /// <summary>
    /// Calculates the velocity of the rigid body at a specific world point.
    /// </summary>
    /// <param name="worldPoint">The point in world coordinates where velocity is to be calculated.</param>
    /// <returns>The velocity of the rigid body at the specified world point.</returns>
    public Vector2 GetWorldPointVelocity(Vector2 worldPoint) {
        return this._body.GetWorldPointVelocity(worldPoint);
    }

    /// <summary>
    /// Applies a force to the rigid body at a specified point.
    /// </summary>
    /// <param name="force">The force vector to apply.</param>
    /// <param name="point">The world position where the force is applied.</param>
    /// <param name="wake">Indicates whether to wake up the body, if it is sleeping.</param>
    public void ApplyForce(Vector2 force, Vector2 point, bool wake) {
        this._body.ApplyForce(force, point, wake);
    }

    /// <summary>
    /// Applies a force to the center of the rigid body, influencing its velocity.
    /// </summary>
    /// <param name="force">The two-dimensional vector defining the magnitude and direction of the force to apply.</param>
    /// <param name="wake">A boolean indicating whether to wake up the body if it is currently sleeping.</param>
    public void ApplyForceToCenter(Vector2 force, bool wake) {
        this._body.ApplyForceToCenter(force, wake);
    }

    /// <summary>
    /// Applies a torque to the rigid body, affecting its angular velocity.
    /// </summary>
    /// <param name="torque">The torque to apply, measured in Newton-meters.</param>
    /// <param name="wake">Determines whether to wake the body if it is currently sleeping.</param>
    public void ApplyTorque(float torque, bool wake) {
        this._body.ApplyTorque(torque, wake);
    }

    /// <summary>
    /// Applies a linear impulse to the body at a specific point in world coordinates.
    /// </summary>
    /// <param name="impulse">The impulse vector to apply to the body.</param>
    /// <param name="point">The world position where the impulse is applied.</param>
    /// <param name="wake">A value indicating whether to wake the body if it is currently sleeping.</param>
    public void ApplyLinearImpulse(Vector2 impulse, Vector2 point, bool wake) {
        this._body.ApplyLinearImpulse(impulse, point, wake);
    }

    /// <summary>
    /// Applies a linear impulse to the center of mass of the rigid body.
    /// </summary>
    /// <param name="impulse">The vector representing the magnitude and direction of the impulse to apply.</param>
    /// <param name="wake">A boolean indicating whether to wake the body if it is currently sleeping.</param>
    public void ApplyLinearImpulseToCenter(Vector2 impulse, bool wake) {
        this._body.ApplyLinearImpulseToCenter(impulse, wake);
    }

    /// <summary>
    /// Applies an angular impulse to the rigid body, affecting its angular velocity.
    /// </summary>
    /// <param name="impulse">The magnitude of the angular impulse to apply.</param>
    /// <param name="wake">A boolean indicating whether the body should be woken up if it is sleeping.</param>
    public void ApplyAngularImpulse(float impulse, bool wake) {
        this._body.ApplyAngularImpulse(impulse, wake);
    }

    /// <summary>
    /// Enables or disables contact events for the rigid body.
    /// </summary>
    /// <param name="flags">A boolean value indicating whether contact events should be enabled or disabled.</param>
    public void EnableContactEvents(bool flags) {
        this._body.EnableContactEvents(flags);
    }

    /// <summary>
    /// Enables or disables hit event handling for the rigid body.
    /// </summary>
    /// <param name="flags">A boolean value indicating whether hit events should be enabled (true) or disabled (false).</param>
    public void EnableHitEvents(bool flags) {
        this._body.EnableHitEvents(flags);
    }

    /// <summary>
    /// Creates a new shape and attaches it to the body using the specified shape definition and geometry.
    /// </summary>
    /// <param name="def">The shape definition containing physical and configuration data for the shape.</param>
    /// <param name="circle">The circle geometry representing the shape.</param>
    /// <returns>The created shape instance.</returns>
    public Shape CreateShape(ShapeDef def, Circle circle) {
        return this._body.CreateShape(def, circle);
    }

    /// <summary>
    /// Creates a new shape for the rigid body using the specified shape definition and segment.
    /// </summary>
    /// <param name="def">The shape definition that specifies the properties of the shape.</param>
    /// <param name="segment">The segment used to define specific geometry of the shape.</param>
    /// <returns>The created shape instance.</returns>
    public Shape CreateShape(ShapeDef def, Segment segment) {
        return this._body.CreateShape(def, segment);
    }

    /// <summary>
    /// Creates a new shape and attaches it to the body based on the provided definition and capsule geometry.
    /// </summary>
    /// <param name="def">The shape definition containing attributes such as density, friction, and filter information.</param>
    /// <param name="capsule">The capsule geometry describing the shape's dimensions and orientation.</param>
    /// <returns>A <see cref="Shape"/> instance representing the newly created shape.</returns>
    public Shape CreateShape(ShapeDef def, Capsule capsule) {
        return this._body.CreateShape(def, capsule);
    }

    /// <summary>
    /// Creates a new shape and attaches it to the rigid body using the provided shape definition and polygon data.
    /// </summary>
    /// <param name="def">The shape definition containing properties for the new shape.</param>
    /// <param name="polygon">The polygon data defining the geometry of the shape.</param>
    /// <return>Returns the created <see cref="Shape"/> instance.</return>
    public Shape CreateShape(ShapeDef def, Polygon polygon) {
        return this._body.CreateShape(def, polygon);
    }

    /// <summary>
    /// Creates a new chain shape based on the specified chain definition.
    /// </summary>
    /// <param name="def">The chain definition containing the configuration details for the chain shape.</param>
    /// <returns>A <see cref="ChainShape"/> instance representing the created chain shape.</returns>
    public ChainShape CreateChain(ChainDef def) {
        return this._body.CreateChain(def);
    }

    /// <summary>
    /// Creates and initializes a physical body in the 2D world using the specified body definition and associated shapes.
    /// </summary>
    private void CreateBody() {
        this._body = this.World.CreateBody(new BodyDef() {
            Type = this._bodyDef.Type,
            Position = new Vector2(this.Entity.Transform.Translation.X, this.Entity.Transform.Translation.Y),
            Rotation = this.Entity.Transform.Rotation.ToEuler().Z,
            LinearVelocity = this._bodyDef.LinearVelocity,
            AngularVelocity = this._bodyDef.AngularVelocity,
            LinearDamping = this._bodyDef.LinearDamping,
            AngularDamping = this._bodyDef.AngularDamping,
            GravityScale = this._bodyDef.GravityScale,
            SleepThreshold = this._bodyDef.SleepThreshold,
            Name = this._bodyDef.Name,
            UserData = this._bodyDef.UserData,
            EnableSleep = this._bodyDef.EnableSleep,
            IsAwake = this._bodyDef.IsAwake,
            FixedRotation = this._bodyDef.FixedRotation,
            IsBullet = this._bodyDef.IsBullet,
            IsEnabled = this._bodyDef.IsEnabled,
            AllowFastRotation = this._bodyDef.AllowFastRotation
        });
        
        foreach (IShape2D shape in this._shapes) {
            shape.CreateShape(this._body);
        }
    }
    
    /// <summary>
    /// Handles the logic for syncing the entity's position and rotation with the associated physics body's movement.
    /// </summary>
    /// <param name="moveEvent">The event data containing information about the body movement.</param>
    private void BodyMoving(BodyMoveEvent moveEvent) {
        if (moveEvent.Body == this._body) {
            if (!this._isSyncing) {
                this._isSyncing = true;
                try {
                    // Sync Position.
                    Vector2 entityPos = new Vector2(this.Entity.Transform.Translation.X, this.Entity.Transform.Translation.Y);
                    Vector2 bodyPos = moveEvent.Body.Position;

                    if (bodyPos != entityPos) {
                        this.Entity.Transform.Translation = new Vector3(bodyPos.X, bodyPos.Y, 0);
                    }

                    // Sync Rotation.
                    Quaternion entityRot = this.Entity.Transform.Rotation;
                    Quaternion bodyRot = Quaternion.CreateFromYawPitchRoll(0, 0, moveEvent.Body.Rotation);

                    if (entityRot != bodyRot) {
                        this.Entity.Transform.Rotation = bodyRot;
                    }
                } finally {
                    this._isSyncing = false;
                }
            }
        }
    }

    /// <summary>
    /// Handles the logic for sync the body's position and rotation with the associated entity movement.
    /// </summary>
    /// <param name="transform">The transform of the entity.</param>
    private void OnEntityMoving(Transform transform) {
        if (!this._isSyncing) {
            this._isSyncing = true;
            try {
                // Sync Position.
                Vector2 entityPos = new Vector2(transform.Translation.X, transform.Translation.Y);
                Vector2 bodyPos = this.Position;

                if (bodyPos != entityPos) {
                    this.Transform = this.Transform with {
                        Position = entityPos
                    };
                }

                // Sync Rotation.
                Quaternion entityRot = transform.Rotation;
                Quaternion bodyRot = Quaternion.CreateFromYawPitchRoll(0, 0, this.Rotation);

                if (entityRot != bodyRot) {
                    this.Transform = this.Transform with {
                        Rotation = entityRot.ToEuler().Z
                    };
                }
            } finally {
                this._isSyncing = false;
            }
        }
    }
    
    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (disposing) {
            this._body.Destroy();
        }
    }
}