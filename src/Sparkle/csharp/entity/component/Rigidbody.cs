using JoltPhysicsSharp;
using Sparkle.csharp.physics.layers;

namespace Sparkle.csharp.entity.component; 

public class Rigidbody : Component {
    
    public Shape Shape { get; private set; }
    public MotionType MotionType { get; private set; }
    public BodyID BodyId { get; private set; }
    
    public BodyInterface BodyInterface => Game.Instance.Simulation.PhysicsSystem.BodyInterface;
    
    private ObjectLayer _objectLayer;
    
    public Rigidbody(Shape shape, MotionType type) {
        this.Shape = shape;
        this.MotionType = type;
    }

    protected internal override void Init() {
        base.Init();
        
        switch (this.MotionType) {
            case MotionType.Static:
                this._objectLayer = Layers.NonMoving;
                break;
            case MotionType.Kinematic:
                this._objectLayer = Layers.Moving;
                break;
            case MotionType.Dynamic:
                this._objectLayer = Layers.Moving;
                break;
        }
        
        BodyCreationSettings settings = new BodyCreationSettings(this.Shape, this.Entity.Position, this.Entity.Rotation, this.MotionType, this._objectLayer);

        Body body = this.BodyInterface.CreateBody(settings);
        
        this.BodyInterface.AddBody(body, Activation.Activate);
        this.BodyId = body.ID;
    }

    protected internal override void AfterUpdate() {
        base.AfterUpdate();
        
        this.BodyInterface.SetPositionAndRotationWhenChanged(this.BodyId, new Double3(this.Entity.Position.X, this.Entity.Position.Y, this.Entity.Position.Z), this.Entity.Rotation, Activation.Activate);
    }

    protected internal override void FixedUpdate() {
        base.FixedUpdate();
        
        this.Entity.Position = this.BodyInterface.GetPosition(this.BodyId);
        this.Entity.Rotation = this.BodyInterface.GetRotation(this.BodyId);
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (disposing) {
            this.BodyInterface.RemoveBody(this.BodyId);
            this.BodyInterface.DestroyBody(this.BodyId);
        }
    }
}