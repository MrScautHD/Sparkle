using Box2D;

namespace Sparkle.CSharp.Physics.Dim2.Shapes;

public class CircleShape2D : IShape2D {

    private readonly Circle _circle;
    private readonly ShapeDef _def;
    
    public CircleShape2D(Circle circle, ShapeDef def) {
        this._circle = circle;
        this._def = def;
    }
    
    public void CreateShape(Body body) {
        body.CreateShape(this._def, this._circle);
    }
}