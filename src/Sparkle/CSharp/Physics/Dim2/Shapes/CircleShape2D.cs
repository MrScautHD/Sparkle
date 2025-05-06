using Box2D;

namespace Sparkle.CSharp.Physics.Dim2.Shapes;

public class CircleShape2D : IShape2D {

    /// <summary>
    /// The circle geometry containing radius and center information.
    /// </summary>
    private readonly Circle _circle;
    
    /// <summary>
    /// The shape definition specifying physical properties like density and friction.
    /// </summary>
    private readonly ShapeDef _def;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="CircleShape2D"/> class.
    /// </summary>
    /// <param name="circle">The circle geometry defining the shape's radius and center.</param>
    /// <param name="def">The definition of the shape's physical properties.</param>
    public CircleShape2D(Circle circle, ShapeDef def) {
        this._circle = circle;
        this._def = def;
    }
    
    /// <summary>
    /// Creates the circle shape and attaches it to the specified physics body.
    /// </summary>
    /// <param name="body">The body to which the circle shape will be attached.</param>
    public void CreateShape(Body body) {
        body.CreateShape(this._def, this._circle);
    }
}