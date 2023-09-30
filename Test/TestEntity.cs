using System.Numerics;
using JoltPhysicsSharp;
using Raylib_cs;
using Sparkle.csharp;
using Sparkle.csharp.entity;
using Sparkle.csharp.entity.component;
using Sparkle.csharp.physics.layers;

namespace Test;

public class TestEntity : Entity {

    private Model _model;
    private Texture2D _texture;
    
    public TestEntity(Vector3 position) : base(position) {
        this._model = Game.Instance.Content.Load<Model>("model.glb");
        this._texture = Game.Instance.Content.Load<Texture2D>("texture.png");
        
        this.AddComponent(new ModelRenderer(this._model, this._texture));
        this.AddComponent(new Rigidbody(new BoxShape(new Vector3(1, 1, 1)), MotionType.Dynamic));
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();
        /*
        Rigidbody body = this.GetComponent<Rigidbody>();

        RayCastResult result = new RayCastResult();
        if (body.Simulation.PhysicsSystem.NarrowPhaseQuery.CastRay(new Double3(this.Position.X, this.Position.Y - 8, this.Position.Z), Vector3.Zero, ref result, null, null, null)) {
            
            float force = 1;
            body.BodyInterface.AddForce(body.BodyId, new Vector3(0, (8000 * -body.Simulation.Settings.Gravity.Y) + force, 0));
        }*/
    }
}