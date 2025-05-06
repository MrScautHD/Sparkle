using Box2D;

namespace Sparkle.CSharp.Physics.Dim2.Shapes;

public interface IShape2D {
    
    /// <summary>
    /// Creates and attaches a shape to the specified physics body.
    /// </summary>
    /// <param name="body">The physics body to which the shape will be attached.</param>
    void CreateShape(Body body);
}