using Box2D;

namespace Sparkle.CSharp.Physics.Dim2.Shapes;

public class PolygonShape2D : IShape2D {

    private readonly Polygon _polygon;
    private readonly ShapeDef _def;
    
    public PolygonShape2D(Polygon polygon, ShapeDef def) {
        this._polygon = polygon;
        this._def = def;
    }
    
    public void CreateShape(Body body) {
        body.CreateShape(this._def, this._polygon);
    }
}