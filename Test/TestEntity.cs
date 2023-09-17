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
        this.AddComponent(new Rigidbody(new BoxShape(new Vector3(1, 1, 1)), MotionType.Dynamic));
    }
}