using Box2D;

namespace Sparkle.CSharp.Physics.Dim2.Shapes;

public class ChainShape2D : IShape2D {

    private readonly ChainDef _def;
    
    public ChainShape2D(ChainDef def) {
        this._def = def;
    }
    
    public void CreateShape(Body body) {
        body.CreateChain(this._def);
    }
}