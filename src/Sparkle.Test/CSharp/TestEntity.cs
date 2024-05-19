using System.Numerics;
using Jitter2.Collision.Shapes;
using Jitter2.LinearMath;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Materials;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.Registries.Types;
using Sparkle.CSharp.Rendering.Models;

namespace Sparkle.Test.CSharp;

public class TestEntity : Entity {
    
    public TestEntity(Vector3 position) : base(position) {
        
        // TODO REWORK THIS (By setting in the ModelRenderer the Materials it set the materials to the model (global) because the pointer so it makes no scenes to place the materials into the ModelRenderer).
        Material[] materials = new MaterialBuilder(TestGame.PlayerModel) //TODO Maybe rename it to "MaterialManipulator"
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
        ModelRenderer modelRenderer = new ModelRenderer(TestGame.PlayerModel, Vector3.Zero, materials, EffectRegistry.Pbr, Color.White);
        //modelRenderer.AnimationPlayer?.Play(0, true, 0);
        this.AddComponent(modelRenderer);
        
        // PHYSICS
        this.AddComponent(new RigidBody(new BoxShape(2, 4, 2)));
    }
    
    protected override void FixedUpdate() {
        base.FixedUpdate();

        RigidBody body = this.GetComponent<RigidBody>();
        
        if (!body.World.RayCast(new JVector(this.Position.X, this.Position.Y - 8, this.Position.Z), -JVector.UnitY, default, default, out Shape? shape, out JVector normal, out float fraction)) {
            body.JBody.AddForce(new JVector(0, 200, 0));
        }
    }

    protected override void Update() {
        base.Update();

        /*
        if (Input.IsKeyPressed(KeyboardKey.U)) {
            this.GetComponent<ModelRenderer>().AnimationPlayer.Play(1, true, 0.5F);
        }
        
        if (Input.IsKeyPressed(KeyboardKey.I)) {
            this.GetComponent<ModelRenderer>().AnimationPlayer.Play(0, true, 0.5F);
        }
        
        if (Input.IsKeyPressed(KeyboardKey.O)) {
            this.GetComponent<ModelRenderer>().AnimationPlayer.Stop();
        }
        
        if (Input.IsKeyPressed(KeyboardKey.P)) {
            this.GetComponent<ModelRenderer>().AnimationPlayer.Pause();
        }
        if (Input.IsKeyPressed(KeyboardKey.L)) {
            this.GetComponent<ModelRenderer>().AnimationPlayer.UnPause();
        }*/
    }
}