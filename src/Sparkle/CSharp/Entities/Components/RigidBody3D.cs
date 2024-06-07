using System.Numerics;
using Jitter2;
using Jitter2.Collision;
using Jitter2.Collision.Shapes;
using Jitter2.DataStructures;
using Jitter2.Dynamics;
using Jitter2.Dynamics.Constraints;
using Jitter2.UnmanagedMemory;
using Raylib_CSharp;
using Sparkle.CSharp.Physics.Dim3;
using Sparkle.CSharp.Physics.Dim3.Conversions;
using Sparkle.CSharp.Scenes;

namespace Sparkle.CSharp.Entities.Components;

public class RigidBody3D : Component {
    
    public World World => ((Simulation3D) SceneManager.Simulation!).World;
    public RigidBody Body { get; private set; }

    public ulong BodyId => this.Body.RigidBodyId;
    public bool IsActive => this.Body.IsActive;
    public ref RigidBodyData Data => ref this.Body.Data;
    public JHandle<RigidBodyData> Handle => this.Body.Handle;
    public ReadOnlyList<Shape> Shapes => this.Body.Shapes;
    public float Mass => this.Body.Mass;
    public Island Island => this.Body.Island;
    public ReadOnlyList<RigidBody> Connections => this.Body.Connections;
    public ReadOnlyHashSet<Constraint> Constraints => this.Body.Constraints;
    public Matrix4x4 InverseInertia => PhysicsConversion.FromJMatrix(this.Body.InverseInertia);
    
    private ReadOnlyList<Shape> _shapes;
    private bool _setMassInertia;
    private bool _nonMoving;
    private float _friction;
    private float _restitution;

    /// <summary>
    /// Constructor for creating a RigidBody3D object with a single shape.
    /// </summary>
    /// <param name="shape">The shape of the rigid body.</param>
    /// <param name="setMassInertia">Flag indicating whether to set mass and inertia.</param>
    /// <param name="nonMoving">Flag indicating whether the body is non-moving.</param>
    /// <param name="friction">Friction coefficient.</param>
    /// <param name="restitution">Restitution coefficient.</param>
    public RigidBody3D(Shape shape, bool setMassInertia = true, bool nonMoving = false, float friction = 0.2F, float restitution = 0) : this([], setMassInertia, nonMoving, friction, restitution) {
        List<Shape> shapes = new List<Shape>();
        shapes.Add(shape);
        this._shapes = new ReadOnlyList<Shape>(shapes);
    }
    
    /// <summary>
    /// Constructor for creating a RigidBody3D object.
    /// </summary>
    /// <param name="shapes">List of shapes composing the rigid body.</param>
    /// <param name="setMassInertia">Flag indicating whether to set mass and inertia.</param>
    /// <param name="nonMoving">Flag indicating whether the body is non-moving.</param>
    /// <param name="friction">Friction coefficient.</param>
    /// <param name="restitution">Restitution coefficient.</param>
    public RigidBody3D(List<Shape> shapes, bool setMassInertia = true, bool nonMoving = false, float friction = 0.2F, float restitution = 0) : base(Vector3.Zero) {
        this._shapes = new ReadOnlyList<Shape>(shapes);
        this._setMassInertia = setMassInertia;
        this._nonMoving = nonMoving;
        this._friction = friction;
        this._restitution = restitution;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the rigid body is affected by gravity.
    /// </summary>
    public bool AffectedByGravity {
        get => this.Body.AffectedByGravity;
        set => this.Body.AffectedByGravity = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether speculative contacts are enabled for the rigid body.
    /// </summary>
    public bool EnableSpeculativeContacts {
        get => this.Body.EnableSpeculativeContacts;
        set => this.Body.EnableSpeculativeContacts = value;
    }
    
    /// <summary>
    /// Gets or sets a value indicating whether the rigid body is static.
    /// </summary>
    public bool IsStatic {
        get => this.Body.IsStatic;
        set => this.Body.IsStatic = value;
    }
    
    /// <summary>
    /// Gets or sets a tag object associated with the rigid body.
    /// </summary>
    public object? Tag {
        get => this.Body.Tag;
        set => this.Body.Tag = value;
    }
    
    /// <summary>
    /// Gets or sets the friction coefficient of the rigid body.
    /// </summary>
    public float Friction {
        get => this.Body.Friction;
        set => this.Body.Friction = value;
    }
    
    /// <summary>
    /// Gets or sets the restitution (bounciness) coefficient of the rigid body.
    /// </summary>
    public float Restitution {
        get => this.Body.Restitution;
        set => this.Body.Restitution = value;
    }
    
    /// <summary>
    /// Gets or sets the deactivation time of the rigid body.
    /// </summary>
    public TimeSpan DeactivationTime {
        get => this.Body.DeactivationTime;
        set => this.Body.DeactivationTime = value;
    }
    
    /// <summary>
    /// Sets the deactivation thresholds for the rigid body.
    /// </summary>
    public (float angular, float linear) DeactivationThreshold {
        set => this.Body.DeactivationThreshold = value;
    }
    
    /// <summary>
    /// Gets or sets the damping values for the rigid body.
    /// </summary>
    public (float angular, float linear) Damping {
        get => this.Body.Damping;
        set => this.Body.Damping = value;
    }
    
    /// <summary>
    /// Gets or sets the position of the rigid body.
    /// </summary>
    public Vector3 Position {
        get => PhysicsConversion.FromJVector(this.Body.Position);
        set => this.Body.Position = PhysicsConversion.ToJVector(value);
    }
    
    /// <summary>
    /// Gets or sets the orientation of the rigid body.
    /// </summary>
    public Quaternion Orientation {
        get => PhysicsConversion.FromJQuaternion(this.Body.Orientation);
        set => this.Body.Orientation = PhysicsConversion.ToJQuaternion(value);
    }
    
    /// <summary>
    /// Gets or sets the velocity of the rigid body.
    /// </summary>
    public Vector3 Velocity {
        get => PhysicsConversion.FromJVector(this.Body.Velocity);
        set => this.Body.Velocity = PhysicsConversion.ToJVector(value);
    }
    
    /// <summary>
    /// Gets or sets the angular velocity of the rigid body.
    /// </summary>
    public Vector3 AngularVelocity {
        get => PhysicsConversion.FromJVector(this.Body.AngularVelocity);
        set => this.Body.AngularVelocity = PhysicsConversion.ToJVector(value);
    }
    
    /// <summary>
    /// Gets or sets the force applied to the rigid body.
    /// </summary>
    public Vector3 Force {
        get => PhysicsConversion.FromJVector(this.Body.Force);
        set => this.Body.Force = PhysicsConversion.ToJVector(value);
    }
    
    /// <summary>
    /// Gets or sets the torque applied to the rigid body.
    /// </summary>
    public Vector3 Torque {
        get => PhysicsConversion.FromJVector(this.Body.Torque);
        set => this.Body.Torque = PhysicsConversion.ToJVector(value);
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
    
    /// <summary>
    /// Creates the body for the rigid body component.
    /// </summary>
    private void CreateBody() {
        this.Body = this.World.CreateRigidBody();
        this.AddShape(this._shapes, this._setMassInertia);
        this.IsStatic = this._nonMoving;
        this.Friction = this._friction;
        this.Restitution = this._restitution;
        this.Position = this.Entity.Position;
        this.Orientation = Quaternion.Conjugate(this.Entity.Rotation);
    }

    /// <summary>
    /// Update the position of the entity based on the position of the rigid body.
    /// </summary>
    private void UpdateEntityPosition() {
        if (this.Body.IsActive) {
            this.Entity.Position = this.Position;
        }
    }

    /// <summary>
    /// Updates the position of the RigidBody to match the Entity's position.
    /// </summary>
    private void UpdateBodyPosition() {
        if (this.Position != this.Entity.Position) {
            this.Position = this.Entity.Position;
        }
    }

    /// <summary>
    /// Updates the rotation of the entity based on the rotation of the rigid body.
    /// </summary>
    private void UpdateEntityRotation() {
        if (this.Body.IsActive) {
            this.Entity.Rotation = this.Orientation;
        }
    }

    /// <summary>
    /// Updates the rotation of the RigidBody to match the Entity's rotation.
    /// </summary>
    private void UpdateBodyRotation() {
        Quaternion entityRot = Quaternion.Conjugate(this.Entity.Rotation);
        
        if (RayMath.QuaternionEquals(entityRot, this.Orientation) == 0) {
            this.Orientation = entityRot;
        }
    }

    /// <summary>
    /// Sets the activation state of the RigidBody.
    /// </summary>
    /// <param name="active">True if the body should be active, false otherwise.</param>
    public void SetActivationState(bool active) {
        this.Body.SetActivationState(active);
    }

    /// <summary>
    /// Adds a shape to the RigidBody and sets the mass and inertia.
    /// </summary>
    /// <param name="shape">The shape to add.</param>
    /// <param name="setMassInertia">Flag indicating whether to set the mass and inertia.</param>
    public void AddShape(Shape shape, bool setMassInertia = true) {
        this.Body.AddShape(shape, setMassInertia);
    }

    /// <summary>
    /// Adds a shape to the RigidBody.
    /// </summary>
    /// <param name="shapes">The shapes to be added.</param>
    /// <param name="setMassInertia">Flag indicating whether to set mass and inertia.</param>
    public void AddShape(IEnumerable<Shape> shapes, bool setMassInertia = true) {
        this.Body.AddShape(shapes, setMassInertia);
    }

    /// <summary>
    /// Removes a shape from the RigidBody.
    /// </summary>
    /// <param name="shape">The shape to remove.</param>
    /// <param name="setMassInertia">Flag indicating whether to update mass and inertia.</param>
    public void RemoveShape(Shape shape, bool setMassInertia = true) {
        this.Body.RemoveShape(shape, setMassInertia);
    }

    /// <summary>
    /// Removes the specified shape from the RigidBody.
    /// </summary>
    /// <param name="shapes">The shapes to be removed.</param>
    /// <param name="setMassInertia">Flag indicating whether to set mass and inertia after removing the shape.</param>
    public void RemoveShape(IEnumerable<Shape> shapes, bool setMassInertia = true) {
        this.Body.RemoveShape(shapes, setMassInertia);
    }

    /// <summary>
    /// Clears all shapes from the RigidBody.
    /// </summary>
    /// <param name="setMassInertia">Flag indicating whether to set the mass and inertia after clearing the shapes.</param>
    public void ClearShapes(bool setMassInertia = true) {
        this.Body.ClearShapes(setMassInertia);
    }

    /// <summary>
    /// Sets the mass and inertia of the RigidBody.
    /// </summary>
    public void SetMassInertia() {
        this.Body.SetMassInertia();
    }

    /// <summary>
    /// Sets the mass and inertia of the RigidBody based on the specified mass.
    /// </summary>
    /// <param name="mass">The mass of the rigid body.</param>
    public void SetMassInertia(float mass) {
        this.Body.SetMassInertia(mass);
    }

    /// <summary>
    /// Sets the mass and inertia of the RigidBody.
    /// </summary>
    /// <param name="inertia">The inertia matrix of the rigid body.</param>
    /// <param name="mass">The mass of the rigid body.</param>
    /// <param name="setAsInverse">A flag indicating whether the given inertia matrix is the inverse inertia.</param>
    public void SetMassInertia(Matrix4x4 inertia, float mass, bool setAsInverse = false) {
        this.Body.SetMassInertia(PhysicsConversion.ToJMatrix(inertia), mass, setAsInverse);
    }

    /// <summary>
    /// Applies a force to the RigidBody.
    /// </summary>
    /// <param name="force">The force to be applied.</param>
    public void AddForce(Vector3 force) {
        this.Body.AddForce(PhysicsConversion.ToJVector(force));
    }

    /// <summary>
    /// Adds a force to the RigidBody at the specified position, This force will affect the body's linear motion.
    /// </summary>
    /// <param name="force">The force to be applied.</param>
    /// <param name="position">The position at which the force is applied.</param>
    public void AddForce(Vector3 force, Vector3 position) {
        this.Body.AddForce(PhysicsConversion.ToJVector(force), PhysicsConversion.ToJVector(position));
    }

    /// <summary>
    /// Debug draws the rigid body using the specified debug drawer.
    /// </summary>
    /// <param name="drawer">The debug drawer used to draw the rigid body.</param>
    public void DebugDraw(IDebugDrawer drawer) {
        this.Body.DebugDraw(drawer);
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            this.World.Remove(this.Body);
        }
    }
}