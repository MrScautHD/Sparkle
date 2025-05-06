using Box2D;

namespace Sparkle.CSharp.Physics.Dim2.Shapes;

public class SegmentShape2D : IShape2D {
    
    /// <summary>
    /// The segment geometry defining the line segment of the shape.
    /// </summary>
    private readonly Segment _segment;
    
    /// <summary>
    /// The shape definition specifying physical properties like density and friction.
    /// </summary>
    private readonly ShapeDef _def;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SegmentShape2D"/> class.
    /// </summary>
    /// <param name="segment">The segment geometry defining the shape's structure.</param>
    /// <param name="def">The definition of the shape's physical properties.</param>
    public SegmentShape2D(Segment segment, ShapeDef def) {
        this._segment = segment;
        this._def = def;
    }
    
    /// <summary>
    /// Creates the segment shape and attaches it to the specified physics body.
    /// </summary>
    /// <param name="body">The body to which the segment shape will be attached.</param>
    public void CreateShape(Body body) {
        body.CreateShape(this._def, this._segment);
    }
}