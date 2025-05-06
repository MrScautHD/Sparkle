using Box2D;

namespace Sparkle.CSharp.Physics.Dim2.Shapes;

public interface IShape2D {
    
    void CreateShape(Body body);
}