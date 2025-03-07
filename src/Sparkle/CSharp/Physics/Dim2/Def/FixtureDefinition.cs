using Box2D.NetStandard.Collision.Shapes;
using Box2D.NetStandard.Dynamics.Fixtures;

namespace Sparkle.CSharp.Physics.Dim2.Def;

public class FixtureDefinition {

    /// <summary>
    /// The <see cref="Shape"/> of the fixture.
    /// </summary>
    public Shape Shape;
    
    /// <summary>
    /// The <see cref="Filter"/> used for collision detection.
    /// </summary>
    public Filter Filter;
    
    /// <summary>
    /// Specifies whether the fixture is a sensor (<see langword="true"/> means it doesn't collide).
    /// </summary>
    public bool IsSensor;
    
    /// <summary>
    /// The density of the fixture.
    /// </summary>
    public float Density;
    
    /// <summary>
    /// The friction coefficient of the fixture.
    /// </summary>
    public float Friction;
    
    /// <summary>
    /// The restitution (bounciness) of the fixture.
    /// </summary>
    public float Restitution;

    /// <summary>
    /// Initializes a new instance of the <see cref="FixtureDefinition"/> class with the specified <see cref="Shape"/>.
    /// </summary>
    /// <param name="shape">The <see cref="Shape"/> of the fixture.</param>
    public FixtureDefinition(Shape shape) {
        this.Shape = shape;
        this.Filter = new Filter();
        this.IsSensor = false;
        this.Density = 0.0F;
        this.Friction = 0.2F;
        this.Restitution = 0.0F;
    }
}