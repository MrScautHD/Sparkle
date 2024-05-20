using Box2D.NetStandard.Collision.Shapes;
using Box2D.NetStandard.Dynamics.Fixtures;

namespace Sparkle.CSharp.Physics.Dim2.Def;

public class FixtureDefinition {

    public Shape Shape;
    public Filter Filter;
    
    public bool IsSensor;
    
    public float Density;
    public float Friction;
    public float Restitution;

    /// <summary>
    /// Initializes a new instance of the FixtureDefinition class with the specified shape.
    /// </summary>
    /// <param name="shape">The shape of the fixture.</param>
    public FixtureDefinition(Shape shape) {
        this.Shape = shape;
        this.Filter = new Filter();
        this.IsSensor = false;
        this.Density = 0.0F;
        this.Friction = 0.2F;
        this.Restitution = 0.0F;
    }
}