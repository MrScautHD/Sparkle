using System.Numerics;
using Jitter2.Collision.Shapes;
using Jitter2.LinearMath;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.Models;

namespace Sparkle.Test.CSharp.Dim3;

public class TestEntity : Entity {
    
    public TestEntity(Vector3 position) : base(position) { }

    protected override void Init() {
        base.Init();
        
        // RENDERER
        ModelRenderer modelRenderer = new ModelRenderer(ContentRegistry.PlayerModel, Vector3.Zero, Color.White, ContentRegistry.Animations);
        modelRenderer.AnimationPlayer?.Play(0, true);
        this.AddComponent(modelRenderer);
        
        // PHYSICS
        this.AddComponent(new RigidBody3D(new BoxShape(2, 4, 2)));
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();
        
        RigidBody3D body3D = this.GetComponent<RigidBody3D>();
        
        if (!body3D.World.RayCast(new JVector(this.Position.X, this.Position.Y - 8, this.Position.Z), -JVector.UnitY, default, default, out Shape? shape, out JVector normal, out float fraction)) {
            body3D.AddForce(new Vector3(0, 200, 0));
        }
    }

    protected override void Update() {
        base.Update();

        ModelAnimationPlayer player = this.GetComponent<ModelRenderer>().AnimationPlayer!;
        
        if (Input.IsKeyPressed(KeyboardKey.One)) {
            player.Play(0, true);
        }
        
        if (Input.IsKeyPressed(KeyboardKey.Two)) {
            player.Play(1, true);
        }
                
        if (Input.IsKeyPressed(KeyboardKey.Three)) {
            player.Play(2, true);
        }
        
        if (Input.IsKeyPressed(KeyboardKey.L)) {
            player.Stop();
        }
        
        if (Input.IsKeyPressed(KeyboardKey.P)) {
            if (player.IsPaused) {
                player.UnPause();
            }
            else {
                player.Pause();
            }
        }
    }
}