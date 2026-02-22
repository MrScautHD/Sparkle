using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Transformations;
using Jitter2;
using Jitter2.Collision;
using Jitter2.Collision.Shapes;
using Jitter2.DataStructures;
using Jitter2.Dynamics;
using Jitter2.Dynamics.Constraints;
using Jitter2.Unmanaged;
using Sparkle.CSharp.Physics.Dim3;
using Sparkle.CSharp.Physics.Dim3.Extensions;
using Sparkle.CSharp.Scenes;

namespace Sparkle.CSharp.Entities.Components;

public class RigidBody3D : Component {
    
    /// <summary>
    /// Gets a list of component types that are incompatible with this component.
    /// </summary>
    public override IReadOnlyList<Type> InCompatibleTypes => [
        typeof(RigidBody2D),
        typeof(SoftBody3D)
    ];
    
    /// <summary>
    /// Gets the current 3D simulation instance used by the scene.
    /// Throws an <see cref="InvalidOperationException"/> if the simulation is not of type <see cref="Simulation3D"/>.
    /// </summary>
    public Simulation3D Simulation => SceneManager.Simulation as Simulation3D ?? throw new InvalidOperationException("The current simulation must be of type Simulation3D.");

    /// <summary>
    /// Gets the physics world from the current 3D simulation.
    /// </summary>
    public World World => this.Simulation.World;
    
    /// <summary>
    /// Gets the underlying main rigid body instance used in the simulation.
    /// </summary>
    public RigidBody Body { get; private set; }

    /// <summary>
    /// Gets the unique ID of the rigid body.
    /// </summary>
    public ulong BodyId => this.Body.RigidBodyId;
    
    /// <summary>
    /// Returns whether the rigid body is currently active in the simulation.
    /// </summary>
    public bool IsActive => this.Body.IsActive;
    
    /// <summary>
    /// Gets a reference to the physical data of the rigid body.
    /// </summary>
    public ref RigidBodyData Data => ref this.Body.Data;
    
    /// <summary>
    /// Gets the handle to the rigid body data.
    /// </summary>
    public JHandle<RigidBodyData> Handle => this.Body.Handle;
    
    /// <summary>
    /// Gets the shapes associated with the rigid body.
    /// </summary>
    public ReadOnlyList<RigidBodyShape> Shapes => this.Body.Shapes;
    
    /// <summary>
    /// Gets the mass of the rigid body.
    /// </summary>
    public float Mass => this.Body.Mass;
    
    /// <summary>
    /// Gets the island this rigid body is part of (for sleeping management).
    /// </summary>
    public Island Island => this.Body.Island;
    
    /// <summary>
    /// Gets all bodies connected to this one.
    /// </summary>
    public ReadOnlyList<RigidBody> Connections => this.Body.Connections;
    
    /// <summary>
    /// Gets all constraints applied to this rigid body.
    /// </summary>
    public ReadOnlyHashSet<Constraint> Constraints => this.Body.Constraints;
    
    /// <summary>
    /// Gets the inverse inertia tensor of the rigid body.
    /// </summary>
    public Matrix4x4 InverseInertia => this.Body.InverseInertia.ToMatrix4X4();
    
    /// <summary>
    /// The positional offset of this component, always zero for RigidBody3D.
    /// </summary>
    public new Vector3 OffsetPosition => Vector3.Zero;
    
    /// <summary>
    /// Whether debug information, such as visual representations of physics-related elements, should be displayed for the rigid body.
    /// </summary>
    public bool DrawDebug;

    /// <summary>
    /// The color used for debugging visualization of the 3D rigid body's properties and behavior.
    /// </summary>
    public Color DebugDrawColor;
    
    /// <summary>
    /// Stores the initial list of shapes used to construct the rigid body.
    /// </summary>
    private ReadOnlyList<RigidBodyShape> _shapes;
    
    /// <summary>
    /// Determines whether mass and inertia should be set automatically when shapes are added.
    /// </summary>
    private bool _setMassInertia;
    
    /// <summary>
    /// The motion type of the rigid body, determining whether it is static, dynamic, or kinematic.
    /// </summary>
    private MotionType _motionType;
    
    /// <summary>
    /// The friction value to assign to the rigid body during initialization.
    /// </summary>
    private float _friction;
    
    /// <summary>
    /// The restitution (bounciness) value to assign to the rigid body during initialization.
    /// </summary>
    private float _restitution;
    
    /// <summary>
    /// Indicates whether the synchronization process is currently active (When sync entity with body).
    /// </summary>
    private bool _isSyncing;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="RigidBody3D"/> class with a single collision shape.
    /// </summary>
    /// <param name="shape">The collision shape to attach to the rigid body.</param>
    /// <param name="setMassInertia">Determines whether to automatically calculate mass and inertia from the shape.</param>
    /// <param name="motionType">Specifies whether the rigid body should be dynamic, static, or kinematic in the simulation.</param>
    /// <param name="friction">The friction coefficient of the rigid body.</param>
    /// <param name="restitution">The restitution (bounciness) of the rigid body.</param>
    /// <param name="drawDebug">Indicates whether debug visualization for the rigid body should be enabled.</param>
    public RigidBody3D(RigidBodyShape shape, bool setMassInertia = true, MotionType motionType = MotionType.Dynamic, float friction = 0.2F, float restitution = 0, bool drawDebug = false) : this([shape], setMassInertia, motionType, friction, restitution, drawDebug) { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="RigidBody3D"/> class with a collection of collision shapes.
    /// </summary>
    /// <param name="shapes">The list of collision shapes to attach to the rigid body.</param>
    /// <param name="setMassInertia">Determines whether to automatically calculate mass and inertia from the shapes.</param>
    /// <param name="motionType">Specifies whether the rigid body should be dynamic, static, or kinematic in the simulation.</param>
    /// <param name="friction">The friction coefficient of the rigid body.</param>
    /// <param name="restitution">The restitution (bounciness) of the rigid body.</param>
    /// <param name="drawDebug">Indicates whether debug visualization for the rigid body should be enabled.</param>
    public RigidBody3D(List<RigidBodyShape> shapes, bool setMassInertia = true, MotionType motionType = MotionType.Dynamic, float friction = 0.2F, float restitution = 0, bool drawDebug = false) : base(Vector3.Zero) {
        this._shapes = new ReadOnlyList<RigidBodyShape>(shapes);
        this._setMassInertia = setMassInertia;
        this._motionType = motionType;
        this._friction = friction;
        this._restitution = restitution;
        this.DrawDebug = drawDebug;
        this.DebugDrawColor = Color.White;
    }
    
    /// <summary>
    /// Determines whether the rigid body is affected by gravity.
    /// </summary>
    public bool AffectedByGravity {
        get => this.Body.AffectedByGravity;
        set => this.Body.AffectedByGravity = value;
    }
    
    /// <summary>
    /// Enables or disables speculative contact solving for better collision detection.
    /// </summary>
    public bool EnableSpeculativeContacts {
        get => this.Body.EnableSpeculativeContacts;
        set => this.Body.EnableSpeculativeContacts = value;
    }
    
    /// <summary>
    /// Gets or sets the motion type of the rigid body, determining its dynamic behavior within the physics simulation (e.g., static, dynamic, or kinematic).
    /// </summary>
    public MotionType MotionType {
        get => this.Body.MotionType;
        set => this.Body.MotionType = value;
    }
    
    /// <summary>
    /// Specifies if the rigid body is static (immovable).
    /// </summary>
    [Obsolete($"Use the {nameof(MotionType)} property instead.")]
    public bool IsStatic {
        get => this.Body.IsStatic;
        set => this.Body.IsStatic = value;
    }
    
    /// <summary>
    /// A custom user tag associated with the rigid body.
    /// </summary>
    public object? Tag {
        get => this.Body.Tag;
        set => this.Body.Tag = value;
    }
    
    /// <summary>
    /// Friction coefficient of the rigid body.
    /// </summary>
    public float Friction {
        get => this.Body.Friction;
        set => this.Body.Friction = value;
    }
    
    /// <summary>
    /// Restitution (bounciness) of the rigid body.
    /// </summary>
    public float Restitution {
        get => this.Body.Restitution;
        set => this.Body.Restitution = value;
    }
    
    /// <summary>
    /// Time before the body is considered inactive when not moving.
    /// </summary>
    public TimeSpan DeactivationTime {
        get => this.Body.DeactivationTime;
        set => this.Body.DeactivationTime = value;
    }
    
    /// <summary>
    /// Sets the angular and linear deactivation thresholds.
    /// </summary>
    public (float angular, float linear) DeactivationThreshold {
        set => this.Body.DeactivationThreshold = value;
    }
    
    /// <summary>
    /// Damping factor for angular and linear motion.
    /// </summary>
    public (float angular, float linear) Damping {
        get => this.Body.Damping;
        set => this.Body.Damping = value;
    }

    /// <summary>
    /// Gets or sets the position of the rigid body.
    /// </summary>
    public Vector3 Position {
        get => this.Body.Position;
        set => this.Body.Position = value;
    }
    
    /// <summary>
    /// Gets or sets the orientation of the rigid body.
    /// </summary>
    public Quaternion Orientation {
        get => this.Body.Orientation;
        set => this.Body.Orientation = value;
    }

    /// <summary>
    /// Gets or sets the linear velocity of the rigid body.
    /// </summary>
    public Vector3 Velocity {
        get => this.Body.Velocity;
        set => this.Body.Velocity = value;
    }
    
    /// <summary>
    /// Gets or sets the angular velocity of the rigid body.
    /// </summary>
    public Vector3 AngularVelocity {
        get => this.Body.AngularVelocity;
        set => this.Body.AngularVelocity = value;
    }
    
    /// <summary>
    /// Gets or sets the applied force on the rigid body.
    /// </summary>
    public Vector3 Force {
        get => this.Body.Force;
        set => this.Body.Force = value;
    }
    
    /// <summary>
    /// Gets or sets the applied torque on the rigid body.
    /// </summary>
    public Vector3 Torque {
        get => this.Body.Torque;
        set => this.Body.Torque = value;
    }
    
    /// <summary>
    /// Initializes the rigid body and registers it into the physics world.
    /// </summary>
    protected internal override void Init() {
        base.Init();
        this.CreateBody();
        this.Simulation.BodyMoved += this.SyncEntityToBodyTransform;
    }
    
    /// <summary>
    /// Updates the <see cref="RigidBody3D"/> component by synchronizing the rigid body's state with the entity's global transform.
    /// </summary>
    /// <param name="delta">The time elapsed since the last update, in seconds.</param>
    protected internal override void Update(double delta) {
        base.Update(delta);
        this.SyncBodyToEntityTransform(this.Entity.GlobalTransform);
    }
    
    /// <summary>
    /// Sets the activation state of the rigid body.
    /// </summary>
    /// <param name="active">Indicates whether the rigid body should be activated or deactivated.</param>
    public void SetActivationState(bool active) {
        this.Body.SetActivationState(active);
    }

    /// <summary>
    /// Adds a new collision shape to the rigid body.
    /// </summary>
    /// <param name="shape">The specific collision shape to be added.</param>
    /// <param name="setMassInertia">Indicates whether the mass and inertia should be recalculated after adding the shape.</param>
    public void AddShape(RigidBodyShape shape, bool setMassInertia = true) {
        this.Body.AddShape(shape, setMassInertia);
    }

    /// <summary>
    /// Adds a collision shape to the rigid body.
    /// </summary>
    /// <param name="shapes">The collection of shapes to add.</param>
    /// <param name="setMassInertia">Determines whether to automatically calculate mass and inertia after adding the shape.</param>
    public void AddShape(IEnumerable<RigidBodyShape> shapes, bool setMassInertia = true) {
        this.Body.AddShape(shapes, setMassInertia);
    }

    /// <summary>
    /// Removes a collision shape from the rigid body.
    /// </summary>
    /// <param name="shape">The collision shape to be removed from the rigid body.</param>
    /// <param name="setMassInertia">Determines whether to recalculate mass and inertia after removing the shape.</param>
    public void RemoveShape(RigidBodyShape shape, bool setMassInertia = true) {
        this.Body.RemoveShape(shape, setMassInertia);
    }

    /// <summary>
    /// Removes the specified collision shape from the rigid body.
    /// </summary>
    /// <param name="shapes">The collection of shapes to remove.</param>
    /// <param name="setMassInertia">Determines whether to automatically recalculate mass and inertia after removing the shape.</param>
    public void RemoveShape(IEnumerable<RigidBodyShape> shapes, bool setMassInertia = true) {
        this.Body.RemoveShape(shapes, setMassInertia);
    }

    /// <summary>
    /// Clears all collision shapes attached to the rigid body.
    /// </summary>
    /// <param name="setMassInertia">Determines whether to recalculate mass and inertia after clearing the shapes.</param>
    [Obsolete($"{nameof(this.ClearShapes)} is deprecated, please use {nameof(this.RemoveShape)} instead.")]
    public void ClearShapes(bool setMassInertia = true) {
        this.Body.ClearShapes(setMassInertia);
    }

    /// <summary>
    /// Updates the mass and inertia of the rigid body based on its current shapes and associated properties.
    /// </summary>
    public void SetMassInertia() {
        this.Body.SetMassInertia();
    }

    /// <summary>
    /// Sets the mass and automatically recalculates the inertia of the rigid body.
    /// </summary>
    /// <param name="mass">The new mass value to be applied to the rigid body.</param>
    public void SetMassInertia(float mass) {
        this.Body.SetMassInertia(mass);
    }

    /// <summary>
    /// Sets the mass and inertia of the rigid body.
    /// </summary>
    /// <param name="inertia">The inertia matrix to assign to the rigid body.</param>
    /// <param name="mass">The mass of the rigid body.</param>
    /// <param name="setAsInverse">Determines whether the provided inertia matrix should be treated as an inverse matrix.</param>
    public void SetMassInertia(Matrix4x4 inertia, float mass, bool setAsInverse = false) {
        this.Body.SetMassInertia(inertia.ToJMatrix(), mass, setAsInverse);
    }

    /// <summary>
    /// Applies a force to the rigid body.
    /// </summary>
    /// <param name="force">The force vector to apply to the rigid body.</param>
    public void AddForce(Vector3 force) {
        this.Body.AddForce(force);
    }

    /// <summary>
    /// Applies a force at a specific position on the rigid body.
    /// </summary>
    /// <param name="force">The force vector to apply to the rigid body.</param>
    /// <param name="position">The position at which the force is applied, relative to the rigid body's center of mass.</param>
    public void AddForce(Vector3 force, Vector3 position) {
        this.Body.AddForce(force, position);
    }

    /// <summary>
    /// Renders debug information for the rigid body using the specified debug drawer.
    /// </summary>
    /// <param name="drawer">The debug drawer instance used to render the debug information.</param>
    public void DebugDraw(IDebugDrawer drawer) {
        this.Body.DebugDraw(drawer);
    }
    
    /// <summary>
    /// Creates the internal rigid body and configures its shape, properties, and initial transform.
    /// </summary>
    private void CreateBody() {
        this.Body = this.World.CreateRigidBody();
        this.AddShape(this._shapes, this._setMassInertia);
        this.MotionType = this._motionType;
        this.Friction = this._friction;
        this.Restitution = this._restitution;
        this.Position = this.Entity.GlobalTransform.Translation;
        this.Orientation = this.Entity.GlobalTransform.Rotation;
    }
    
    /// <summary>
    /// Handles the logic for syncing the entity's position and rotation with the associated physics body's movement.
    /// </summary>
    private void SyncEntityToBodyTransform(RigidBody body) {
        if (body == this.Body) {
            if (!this._isSyncing) {
                this._isSyncing = true;
                try {
                    Transform globalTransform = this.Entity.GlobalTransform;
                    
                    Vector3 entityPos = globalTransform.Translation;
                    Vector3 bodyPos = body.Position;
                    
                    Quaternion entityRot = globalTransform.Rotation;
                    Quaternion bodyRot = body.Orientation;
                    
                    if (bodyPos != entityPos || bodyRot != entityRot) {
                        if (this.Entity.Parent == null) {
                            if (bodyPos != entityPos) {
                                this.Entity.LocalTransform.Translation = bodyPos;
                            }
                            
                            if (bodyRot != entityRot) {
                                this.Entity.LocalTransform.Rotation = bodyRot;
                            }
                        }
                        else {
                            Transform parentGlobal = this.Entity.Parent.GlobalTransform;
                            Quaternion invParentRot = Quaternion.Inverse(parentGlobal.Rotation);
                            
                            if (bodyPos != entityPos) {
                                this.Entity.LocalTransform.Translation = Vector3.Transform(bodyPos - parentGlobal.Translation, invParentRot) / parentGlobal.Scale;
                            }
                            
                            if (bodyRot != entityRot) {
                                this.Entity.LocalTransform.Rotation = invParentRot * bodyRot;
                            }
                        }
                        
                        // Restore children's global transforms using inline math.
                        IReadOnlyCollection<Entity> children = this.Entity.GetChildren();
                        
                        if (children.Count > 0) {
                            Transform newGlobalTransform = this.Entity.GlobalTransform;
                            Quaternion invNewParentRot = Quaternion.Inverse(newGlobalTransform.Rotation);
                            
                            foreach (Entity child in children) {
                                bool hasPhysics = false;
                                bool isKinematic = false;
                                
                                if (child.TryGetComponent(out RigidBody3D? childRb)) {
                                    hasPhysics = true;
                                    isKinematic = childRb.MotionType == MotionType.Kinematic;
                                    
                                    if (isKinematic) {
                                        childRb.SyncBodyToEntityTransform(child.GlobalTransform);
                                    }
                                }
                                else if (child.TryGetComponent(out SoftBody3D? childSb)) {
                                    hasPhysics = true;
                                    isKinematic = childSb.Center.MotionType == MotionType.Kinematic;
                                    
                                    if (isKinematic) {
                                        childSb.SyncBodyToEntityTransform(child.GlobalTransform);
                                    }
                                }
                                
                                if (hasPhysics && !isKinematic) {
                                    
                                    // Calculate what the child's global transform was.
                                    Vector3 oldChildGlobalPos = globalTransform.Translation + Vector3.Transform(child.LocalTransform.Translation * globalTransform.Scale, globalTransform.Rotation);
                                    Quaternion oldChildGlobalRot = globalTransform.Rotation * child.LocalTransform.Rotation;
                                    
                                    // Apply it as the new LocalTransform relative to the parent's new position.
                                    child.LocalTransform.Translation = Vector3.Transform(oldChildGlobalPos - newGlobalTransform.Translation, invNewParentRot) / newGlobalTransform.Scale;
                                    child.LocalTransform.Rotation = invNewParentRot * oldChildGlobalRot;
                                }
                            }
                        }
                    }
                }
                finally {
                    this._isSyncing = false;
                }
            }
        }
    }

    /// <summary>
    /// Handles the logic for sync the body's position and rotation with the associated entity movement.
    /// </summary>
    /// <param name="transform">The transform of the entity.</param>
    internal void SyncBodyToEntityTransform(Transform transform) {
        if (!this._isSyncing) {
            this._isSyncing = true;
            try {
                // Sync Position.
                Vector3 entityPos = transform.Translation;
                Vector3 bodyPos = this.Position;
                
                if (bodyPos != entityPos) {
                    this.Position = entityPos;
                }
                
                // Sync Rotation.
                Quaternion entityRot = transform.Rotation;
                Quaternion bodyRot = this.Orientation;
                
                if (entityRot != bodyRot) {
                    this.Orientation = entityRot;
                }
            }
            finally {
                this._isSyncing = false;
            }
        }
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            this.Simulation.BodyMoved -= this.SyncEntityToBodyTransform;
            this.World.Remove(this.Body);
        }
    }
}