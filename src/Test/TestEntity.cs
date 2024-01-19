using System.Numerics;
using JoltPhysicsSharp;
using Raylib_cs;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.Registries.Types;
using Sparkle.CSharp.Rendering.Material;

namespace Test;

public class TestEntity : Entity {
    
    public TestEntity(Vector3 position) : base(position) {

        Material[] materials = new MaterialBuilder(TestGame.PlayerModel, ShaderRegistry.Pbr)
            .Add(MaterialMapIndex.MATERIAL_MAP_ALBEDO, TestGame.PlayerTexture)
            .Add(MaterialMapIndex.MATERIAL_MAP_METALNESS, TestGame.PlayerTexture)
            .Add(MaterialMapIndex.MATERIAL_MAP_NORMAL, TestGame.PlayerTexture)
            .Add(MaterialMapIndex.MATERIAL_MAP_EMISSION, TestGame.PlayerTexture)
            
            .Add(MaterialMapIndex.MATERIAL_MAP_ALBEDO, Color.WHITE)
            .Add(MaterialMapIndex.MATERIAL_MAP_EMISSION, new Color(255, 162, 0, 255))
            
            .Add(MaterialMapIndex.MATERIAL_MAP_METALNESS, 0)
            .Add(MaterialMapIndex.MATERIAL_MAP_ROUGHNESS, 0)
            .Add(MaterialMapIndex.MATERIAL_MAP_OCCLUSION, 1)
            .Build();
        
        this.AddComponent(new ModelRenderer(TestGame.PlayerModel, materials));

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