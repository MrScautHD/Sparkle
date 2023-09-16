using System.Numerics;
using Sparkle.csharp.entity;

namespace Test;

public class TestEntity : Entity {
    
    public TestEntity(Vector3 position) : base(position) {
        //Model model = this.Content.Load<Model>("model.glb");
        //Texture2D texture = this.Content.Load<Texture2D>("texture.png");

        //this.AddComponent(new ModelRenderer(model, texture));
    }
}