using System.Numerics;
using Jitter2;
using Jitter2.Collision.Shapes;
using Jitter2.DataStructures;
using Jitter2.LinearMath;
using Raylib_cs;
using Sparkle.CSharp.Physics.Conversions;
using Sparkle.CSharp.Scenes;

namespace Sparkle.CSharp.Entities.Components;

public class RigidBody : Component {
    
    public World World => SceneManager.Simulation!.World;
    public Jitter2.Dynamics.RigidBody JBody { get; private set; }

    private ReadOnlyList<Shape> _shapes;
    private bool _setMassInertia;
    private bool _nonMoving;
    private float _friction;
    private float _restitution;

    /// <summary>
    /// Represents a rigid body component that provides physics simulation for an entity.
    /// </summary>
    public RigidBody(List<Shape> shapes, bool setMassInertia = true, bool nonMoving = false, float friction = 0.2F, float restitution = 0) {
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
        this.JBody = this.World.CreateRigidBody();
        this.JBody.AddShape(this._shapes, this._setMassInertia);
        this.JBody.IsStatic = this._nonMoving;
        this.JBody.Friction = this._friction;
        this.JBody.Restitution = this._restitution;
        this.JBody.Position = PhysicsConversion.ToJVector(this.Entity.Position);
        this.JBody.Orientation = JMatrix.CreateFromQuaternion(PhysicsConversion.ToJQuaternion(Quaternion.Conjugate(this.Entity.Rotation)));
    }

    /// <summary>
    /// Update the position of the entity based on the position of the rigid body.
    /// </summary>
    private void UpdateEntityPosition() {
        if (this.JBody.IsActive) {
            this.Entity.Position = PhysicsConversion.FromJVector(this.JBody.Position);
        }
    }

    /// <summary>
    /// Updates the position of the RigidBody to match the Entity's position.
    /// </summary>
    private void UpdateBodyPosition() {
        JVector entityPos = PhysicsConversion.ToJVector(this.Entity.Position);

        if (this.JBody.Position != entityPos) {
            this.JBody.Position = entityPos;
        }
    }

    /// <summary>
    /// Updates the rotation of the entity based on the rotation of the rigid body.
    /// </summary>
    private void UpdateEntityRotation() {
        if (this.JBody.IsActive) {
            this.Entity.Rotation = Quaternion.CreateFromRotationMatrix(PhysicsConversion.FromJMatrix(this.JBody.Orientation));
        }
    }

    /// <summary>
    /// Updates the rotation of the RigidBody to match the Entity's rotation.
    /// </summary>
    private void UpdateBodyRotation() {
        Quaternion entityRot = Quaternion.Conjugate(this.Entity.Rotation);
        Quaternion bodyRot = PhysicsConversion.FromJQuaternion(JQuaternion.CreateFromMatrix(this.JBody.Orientation));
        
        if (Raymath.QuaternionEquals(entityRot, bodyRot) == 0) {
            this.JBody.Orientation = JMatrix.CreateFromQuaternion(PhysicsConversion.ToJQuaternion(entityRot));
        }
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            this.World.Remove(this.JBody);
        }
    }
}