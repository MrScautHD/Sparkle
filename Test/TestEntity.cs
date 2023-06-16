using Raylib_cs;
using Sparkle.csharp.entity;

namespace Test; 

public class TestEntity : Entity {
    
    public TestEntity(Transform transform) : base(transform) {
    }

    protected override void Draw() {
        base.Draw();
    }
}