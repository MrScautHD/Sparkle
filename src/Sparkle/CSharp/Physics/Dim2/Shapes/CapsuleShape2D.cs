using Box2D;

namespace Sparkle.CSharp.Physics.Dim2.Shapes;

public class CapsuleShape2D : IShape2D {

    private readonly Capsule _capsule;
    private readonly ShapeDef _def;
    
    public CapsuleShape2D(Capsule capsule, ShapeDef def) {
        this._capsule = capsule;
        this._def = def;
    }
    
    public void CreateShape(Body body) {
        body.CreateShape(this._def, this._capsule);
    }
}