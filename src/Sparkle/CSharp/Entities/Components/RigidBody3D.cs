using System.Numerics;
using Jitter2;
using Jitter2.Collision.Shapes;
using Jitter2.DataStructures;
using Jitter2.Dynamics;
using Jitter2.LinearMath;
using Raylib_CSharp;
using Sparkle.CSharp.Physics.Dim3;
using Sparkle.CSharp.Physics.Dim3.Conversions;
using Sparkle.CSharp.Scenes;

namespace Sparkle.CSharp.Entities.Components;

public class RigidBody3D : Component {
    
    public World World => ((Simulation3D) SceneManager.Simulation!).World;
    public RigidBody Body { get; private set; }

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
        this.Body.AddShape(this._shapes, this._setMassInertia);
        this.Body.IsStatic = this._nonMoving;
        this.Body.Friction = this._friction;
        this.Body.Restitution = this._restitution;
        this.Body.Position = PhysicsConversion.ToJVector(this.Entity.Position);
        this.Body.Orientation = PhysicsConversion.ToJQuaternion(Quaternion.Conjugate(this.Entity.Rotation));
    }

    /// <summary>
    /// Update the position of the entity based on the position of the rigid body.
    /// </summary>
    private void UpdateEntityPosition() {
        if (this.Body.IsActive) {
            this.Entity.Position = PhysicsConversion.FromJVector(this.Body.Position);
        }
    }

    /// <summary>
    /// Updates the position of the RigidBody to match the Entity's position.
    /// </summary>
    private void UpdateBodyPosition() {
        JVector entityPos = PhysicsConversion.ToJVector(this.Entity.Position);

        if (this.Body.Position != entityPos) {
            this.Body.Position = entityPos;
        }
    }

    /// <summary>
    /// Updates the rotation of the entity based on the rotation of the rigid body.
    /// </summary>
    private void UpdateEntityRotation() {
        if (this.Body.IsActive) {
            this.Entity.Rotation = PhysicsConversion.FromJQuaternion(this.Body.Orientation);
        }
    }

    /// <summary>
    /// Updates the rotation of the RigidBody to match the Entity's rotation.
    /// </summary>
    private void UpdateBodyRotation() {
        Quaternion entityRot = Quaternion.Conjugate(this.Entity.Rotation);
        Quaternion bodyRot = PhysicsConversion.FromJQuaternion(this.Body.Orientation);
        
        if (RayMath.QuaternionEquals(entityRot, bodyRot) == 0) {
            this.Body.Orientation = PhysicsConversion.ToJQuaternion(entityRot);
        }
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            this.World.Remove(this.Body);
        }
    }
}