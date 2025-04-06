using System.Numerics;
using Jitter2;
using Jitter2.SoftBodies;

namespace Sparkle.CSharp.Physics.Dim3.SoftBodies.Factories;

public interface ISoftBodyFactory {
    
    /// <summary>
    /// Creates a soft body instance within the specified world.
    /// </summary>
    /// <param name="world">The physics world where the soft body will be created.</param>
    /// <param name="position">The initial position for the soft body.</param>
    /// <param name="rotation">The initial rotation for the soft body.</param>
    /// <param name="scale">The scaling factor for the soft body.</param>
    /// <returns>A new instance of a soft body.</returns>
    SoftBody CreateSoftBody(World world, Vector3 position, Quaternion rotation, Vector3 scale);
}