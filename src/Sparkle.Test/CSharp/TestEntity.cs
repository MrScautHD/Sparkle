using System.Numerics;
using Jitter2.Collision.Shapes;
using Jitter2.LinearMath;
using Raylib_cs;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.Registries.Types;
using Sparkle.CSharp.Rendering.Util;

namespace Sparkle.Test.CSharp;

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

        // RENDERER
        ModelRenderer modelRenderer = new ModelRenderer(TestGame.PlayerModel, materials, EffectRegistry.Pbr/*, default, TestGame.Animations*/);
        //modelRenderer.AnimationPlayer.Play(1, true);
        
        this.AddComponent(modelRenderer);
        
        // PHYSICS
        List<Shape> shapes = new List<Shape>();
        shapes.Add(new BoxShape(2, 4, 2));
        
        this.AddComponent(new RigidBody(shapes));
    }
    
    protected override void FixedUpdate() {
        base.FixedUpdate();

        RigidBody body = this.GetComponent<RigidBody>();
        
        if (!body.World.RayCast(new JVector(this.Position.X, this.Position.Y - 8, this.Position.Z), -JVector.UnitY, default, default, out Shape? shape, out JVector normal, out float fraction)) {
            body.JBody.AddForce(new JVector(0, 200, 0));
        }
    }
}