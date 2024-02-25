using System.Numerics;
using JoltPhysicsSharp;
using Raylib_cs;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.Registries.Types;
using Sparkle.CSharp.Rendering.Util;

namespace Sparkle.Test;

public class TestEntity : Entity {
    
    public TestEntity(Vector3 position) : base(position) {
        
        Material[] materials = new MaterialBuilder(TestGame.PlayerModel)
            .Add(MaterialMapIndex.Albedo, TestGame.PlayerTexture)
            .Add(MaterialMapIndex.Metalness, TestGame.PlayerTexture)
            .Add(MaterialMapIndex.Normal, TestGame.PlayerTexture)
            .Add(MaterialMapIndex.Emission, TestGame.PlayerTexture)
            
            .Add(MaterialMapIndex.Albedo, Color.White)
            .Add(MaterialMapIndex.Emission, new Color(255, 162, 0, 255))
            
            .Add(MaterialMapIndex.Metalness, 0.0F)
            .Add(MaterialMapIndex.Roughness, 0.0F)
            .Add(MaterialMapIndex.Occlusion, 1.0F)
            .Add(MaterialMapIndex.Emission, 0.01F)
            .Build();

        ModelRenderer modelRenderer = new ModelRenderer(TestGame.PlayerModel, materials, EffectRegistry.Pbr/*, default, TestGame.Animations*/);
        //modelRenderer.AnimationPlayer.Play(1, true);
        
        this.AddComponent(modelRenderer);

        BoxShape boxShape = new BoxShape(new Vector3(1, 1, 1));
        this.AddComponent(new Rigidbody(boxShape, MotionType.Dynamic));
    }
    
    /*
    protected override void FixedUpdate() {
        base.FixedUpdate();
        
        Rigidbody body = this.GetComponent<Rigidbody>();

        if (body.Simulation.RayCast(new Vector3(this.Position.X, this.Position.Y - 2, this.Position.Z), out RayCastResult result, -Vector3.UnitY, 8)) {
            float force = 100000;
            body.BodyInterface.AddForce(body.BodyId, new Vector3(0, (body.Shape.MassProperties.Mass * -body.Simulation.Settings.Gravity.Y) + force, 0));
        }
    }*/
}