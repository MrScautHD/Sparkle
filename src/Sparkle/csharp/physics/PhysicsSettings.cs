using System.Numerics;

namespace Sparkle.csharp.physics; 

public struct PhysicsSettings {

    public Vector3 Gravity;
    public int MaxBodies;
    public int MaxBodyPairs;
    public int MaxContactConstraints;

    public PhysicsSettings() {
        this.Gravity = new Vector3(0, -9.81F, 0);
        this.MaxBodies = 10240;
        this.MaxBodyPairs = 65536;
        this.MaxContactConstraints = 10240;
    }
}