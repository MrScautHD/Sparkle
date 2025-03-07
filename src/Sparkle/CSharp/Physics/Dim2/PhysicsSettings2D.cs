using System.Numerics;

namespace Sparkle.CSharp.Physics.Dim2;

public struct PhysicsSettings2D {
    
    /// <summary>
    /// Gravity for the 2D simulation.
    /// </summary>
    public Vector2 Gravity;
    
    /// <summary>
    /// Number of velocity iterations for the simulation.
    /// </summary>
    public int VelocityIterations;
    
    /// <summary>
    /// Number of position iterations for the simulation.
    /// </summary>
    public int PositionIterations;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="PhysicsSettings2D"/> struct with default values.
    /// </summary>
    public PhysicsSettings2D() {
        this.Gravity = new Vector2(0, 9.81F);
        this.VelocityIterations = 6;
        this.PositionIterations = 2;
    }
}