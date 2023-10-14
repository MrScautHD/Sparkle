using System.Numerics;
using JoltPhysicsSharp;
using Raylib_cs;
using Sparkle.csharp;
using Sparkle.csharp.entity;
using Sparkle.csharp.entity.component;

namespace Test;

public class TestEntity : Entity {

    private Model _model;
    private Texture2D _texture;
    
    public TestEntity(Vector3 position) : base(position) {
        this._model = Game.Instance.Content.Load<Model>("model.glb");
        this._texture = Game.Instance.Content.Load<Texture2D>("texture.png");
        
        this.AddComponent(new ModelRenderer(this._model, this._texture));


        BoxShape boxShape = new BoxShape(new Vector3(1, 1, 1));
        //boxShape.MassProperties.ScaleToMass(100000000);
        //boxShape.Density = 10;
        this.AddComponent(new Rigidbody(boxShape, MotionType.Dynamic));
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();
        /*
        Rigidbody body = this.GetComponent<Rigidbody>();

        if (body.Simulation.RayCast(new Vector3(this.Position.X, this.Position.Y - 2, this.Position.Z), out RayCastResult result, -Vector3.UnitY, 8)) {
            float force = 100000;
            body.BodyInterface.AddForce(body.BodyId, new Vector3(0, (body.Shape.MassProperties.Mass * -body.Simulation.Settings.Gravity.Y) + force, 0));
        }*/
    }
}