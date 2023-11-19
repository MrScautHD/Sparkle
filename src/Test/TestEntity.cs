using System.Numerics;
using JoltPhysicsSharp;
using Raylib_cs;
using Sparkle.csharp.entity;
using Sparkle.csharp.entity.component;
using Sparkle.csharp.registry.types;

namespace Test;

public class TestEntity : Entity {
    
    public TestEntity(Vector3 position) : base(position) {
        this.AddComponent(new ModelRenderer(TestGame.PlayerModel, TestGame.PlayerTexture, ShaderRegistry.Light, TestGame.Animations));
        this.GetComponent<ModelRenderer>().AnimationPlayer.Play(1, true);

        BoxShape boxShape = new BoxShape(new Vector3(1, 1, 1));
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