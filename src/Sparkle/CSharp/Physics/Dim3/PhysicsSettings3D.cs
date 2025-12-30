using System.Numerics;

namespace Sparkle.CSharp.Physics.Dim3;

public struct PhysicsSettings3D {

    /// <summary>
    /// The gravitational force applied in the simulation.
    /// Default is (0, -9.81, 0) representing Earth's gravity.
    /// </summary>
    public Vector3 Gravity;
    
    /// <summary>
    /// Specifies whether the physics simulation runs using multiple threads.
    /// </summary>
    public bool MultiThreaded;
    
    /// <summary>
    /// Represents the settings for the physics engine.
    /// </summary>
    public PhysicsSettings3D() {
        this.Gravity = new Vector3(0, -9.81F, 0);
        this.MultiThreaded = true;
    }
}