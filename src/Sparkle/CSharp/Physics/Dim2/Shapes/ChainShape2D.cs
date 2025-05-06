using Box2D;

namespace Sparkle.CSharp.Physics.Dim2.Shapes;

public class ChainShape2D : IShape2D {

    /// <summary>
    /// The chain shape definition that describes the vertices and physical properties.
    /// </summary>
    private readonly ChainDef _def;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ChainShape2D"/> class.
    /// </summary>
    /// <param name="def">The definition for the chain shape's structure and behavior.</param>
    public ChainShape2D(ChainDef def) {
        this._def = def;
    }
    
    /// <summary>
    /// Creates the chain shape and attaches it to the specified physics body.
    /// </summary>
    /// <param name="body">The body to which the chain shape will be attached.</param>
    public void CreateShape(Body body) {
        body.CreateChain(this._def);
    }
}