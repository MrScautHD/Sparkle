using System.Numerics;

namespace Sparkle.CSharp.Physics.Dim2;

public struct PhysicsSettings2D {
    
    public Vector2 Gravity;
    public int VelocityIterations;
    public int PositionIterations;
    
    /// <summary>
    /// Represents the settings for the physics engine.
    /// </summary>
    public PhysicsSettings2D() {
        this.Gravity = new Vector2(0, 9.81F);
        this.VelocityIterations = 6;
        this.PositionIterations = 2;
    }
}