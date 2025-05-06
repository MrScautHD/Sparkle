using Box2D;

namespace Sparkle.CSharp.Physics.Dim2.Shapes;

public class PolygonShape2D : IShape2D {

    /// <summary>
    /// The polygon geometry defining the vertices of the shape.
    /// </summary>
    private readonly Polygon _polygon;
    
    /// <summary>
    /// The shape definition specifying physical properties like density and friction.
    /// </summary>
    private readonly ShapeDef _def;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="PolygonShape2D"/> class.
    /// </summary>
    /// <param name="polygon">The polygon geometry defining the shape's structure.</param>
    /// <param name="def">The definition of the shape's physical properties.</param>
    public PolygonShape2D(Polygon polygon, ShapeDef def) {
        this._polygon = polygon;
        this._def = def;
    }
    
    /// <summary>
    /// Creates the polygon shape and attaches it to the specified physics body.
    /// </summary>
    /// <param name="body">The body to which the polygon shape will be attached.</param>
    public void CreateShape(Body body) {
        body.CreateShape(this._def, this._polygon);
    }
}