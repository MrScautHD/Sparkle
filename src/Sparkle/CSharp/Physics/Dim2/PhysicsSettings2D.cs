using Box2D;

namespace Sparkle.CSharp.Physics.Dim2;

public struct PhysicsSettings2D {
    
    /// <summary>
    /// The definition of the physics world, including its properties and settings.
    /// </summary>
    public WorldDef WorldDef;

    /// <summary>
    /// The number of substeps used for physics simulations in a single timestep.
    /// </summary>
    public int SubStepCount;

    /// <summary>
    /// The events will be processed in parallel.
    /// </summary>
    public bool ParallelEvents;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="PhysicsSettings2D"/> struct with default values.
    /// </summary>
    public PhysicsSettings2D() {
        this.WorldDef = new WorldDef();
        this.SubStepCount = 4;
        this.ParallelEvents = false;
    }
}