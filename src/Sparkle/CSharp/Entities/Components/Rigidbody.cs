using JoltPhysicsSharp;
using Sparkle.CSharp.Physics;
using Sparkle.CSharp.Physics.Layers;

namespace Sparkle.CSharp.Entities.Components; 

public class Rigidbody : Component {
    
    public Simulation Simulation => Game.Instance.Simulation;
    public BodyInterface BodyInterface => Simulation.PhysicsSystem.BodyInterface;
    
    public Shape Shape { get; private set; }
    public MotionType MotionType { get; private set; }
    public BodyID BodyId { get; private set; }
    
    public float Friction;
    public float Restitution;
    
    private ObjectLayer _objectLayer;

    /// <summary>
    /// Initializes a new instance of the <see cref="Rigidbody"/> class with the specified parameters.
    /// </summary>
    /// <param name="shape">The shape of the rigidbody.</param>
    /// <param name="type">The motion type of the rigidbody.</param>
    /// <param name="friction">The friction coefficient of the rigidbody. Default is 0.</param>
    /// <param name="restitution">The restitution coefficient of the rigidbody. Default is 0.</param>
    public Rigidbody(Shape shape, MotionType type, float friction = 0, float restitution = 0) {
        this.Shape = shape;
        this.MotionType = type;
        this.Friction = friction;
        this.Restitution = restitution;
    }

    protected internal override void Init() {
        base.Init();
        
        switch (this.MotionType) {
            case MotionType.Static:
                this._objectLayer = (int) PhysicLayers.NonMoving;
                break;
            case MotionType.Kinematic:
                this._objectLayer = (int) PhysicLayers.Moving;
                break;
            case MotionType.Dynamic:
                this._objectLayer = (int) PhysicLayers.Moving;
                break;
        }
        
        BodyCreationSettings settings = new BodyCreationSettings(this.Shape, this.Entity.Position, this.Entity.Rotation, this.MotionType, this._objectLayer);

        Body body = this.BodyInterface.CreateBody(settings);
        body.Friction = this.Friction;
        body.Restitution = this.Restitution;
        
        this.BodyInterface.AddBody(body, Activation.Activate);
        this.BodyId = body.ID;
    }

    protected internal override void AfterUpdate() {
        base.AfterUpdate();
        
        this.BodyInterface.SetPositionAndRotationWhenChanged(this.BodyId, (Double3) this.Entity.Position, this.Entity.Rotation, Activation.Activate);
    }

    protected internal override void FixedUpdate() {
        base.FixedUpdate();
        
        this.Entity.Position = this.BodyInterface.GetPosition(this.BodyId);
        this.Entity.Rotation = this.BodyInterface.GetRotation(this.BodyId);
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            this.BodyInterface.RemoveBody(this.BodyId);
            this.BodyInterface.DestroyBody(this.BodyId);
            this.Shape.Dispose();
        }
    }
}