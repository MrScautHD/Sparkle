using Box2D;

namespace Sparkle.CSharp.Physics.Dim2.Shapes;

public class CapsuleShape2D : IShape2D {

    /// <summary>
    /// The capsule shape data used for creating the shape.
    /// </summary>
    private readonly Capsule _capsule;
    
    /// <summary>
    /// The shape definition that specifies the physical properties of the shape.
    /// </summary>
    private readonly ShapeDef _def;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="CapsuleShape2D"/> class.
    /// </summary>
    /// <param name="capsule">The capsule shape data.</param>
    /// <param name="def">The definition for the shape's physical properties.</param>
    public CapsuleShape2D(Capsule capsule, ShapeDef def) {
        this._capsule = capsule;
        this._def = def;
    }
    
    /// <summary>
    /// Creates the capsule shape and attaches it to the specified physics body.
    /// </summary>
    /// <param name="body">The body to attach the shape to.</param>
    public void CreateShape(Body body) {
        body.CreateShape(this._def, this._capsule);
    }
}