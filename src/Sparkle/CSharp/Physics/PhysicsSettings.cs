using System.Numerics;

namespace Sparkle.CSharp.Physics; 

public struct PhysicsSettings {

    public Vector3 Gravity { get; init; }
    public int MaxBodies { get; init; }
    public int MaxBodyPairs { get; init; }
    public int MaxContactConstraints { get; init; }

    /// <summary>
    /// Represents the settings for the physics engine.
    /// </summary>
    public PhysicsSettings() {
        this.Gravity = new Vector3(0, -9.81F, 0);
        this.MaxBodies = 10240;
        this.MaxBodyPairs = 65536;
        this.MaxContactConstraints = 10240;
    }
}