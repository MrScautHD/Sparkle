using Box2D;

namespace Sparkle.CSharp.Physics.Dim2.Shapes;

public class SegmentShape2D : IShape2D {

    private readonly Segment _segment;
    private readonly ShapeDef _def;
    
    public SegmentShape2D(Segment segment, ShapeDef def) {
        this._segment = segment;
        this._def = def;
    }
    
    public void CreateShape(Body body) {
        body.CreateShape(this._def, this._segment);
    }
}