using System.Numerics;
using Raylib_cs;
using Sparkle.csharp;
using Sparkle.csharp.entity;
using Sparkle.csharp.entity.component;

namespace Test; 

public class Test2DEntity : Entity {
    
    private readonly Texture2D _texture;
    
    public Test2DEntity(Vector2 position) : base(new Vector3(position.X, position.Y, 0)) {
        this._texture = Game.Instance.Content.Load<Texture2D>("texture.png");
        
        this.AddComponent(new Sprite(this._texture, null, null));
    }
}