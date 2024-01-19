using System.Numerics;
using Raylib_cs;
using Sparkle;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Entities.Components;

namespace Test; 

public class Test2DEntity : Entity {
    
    public Test2DEntity(Vector2 position) : base(new Vector3(position.X, position.Y, 0)) {
        this.AddComponent(new Sprite(TestGame.SpriteTexture, null, null));
    }
}