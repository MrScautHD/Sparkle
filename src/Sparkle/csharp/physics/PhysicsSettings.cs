using System.Numerics;

namespace Sparkle.csharp.physics; 

public struct PhysicsSettings {

    public Vector3 Gravity;
    public uint MaxBodies;
    public uint NumBodyMutexes;
    public uint MaxBodyPairs;
    public uint MaxContactConstraints;

    public PhysicsSettings() {
        this.Gravity = new Vector3(0, -9.81F, 0);
        this.MaxBodies = 70000;
        this.NumBodyMutexes = 0;
        this.MaxBodyPairs = 70000;
        this.MaxContactConstraints = 70000;
    }
}