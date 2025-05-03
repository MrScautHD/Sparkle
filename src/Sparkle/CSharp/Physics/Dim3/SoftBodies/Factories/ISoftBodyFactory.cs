using System.Numerics;
using Jitter2;
using Veldrid;

namespace Sparkle.CSharp.Physics.Dim3.SoftBodies.Factories;

public interface ISoftBodyFactory {
    
    /// <summary>
    /// Creates a soft body instance within the specified world.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used to create material and meshes for the soft body.</param>
    /// <param name="world">The physics world where the soft body will be created.</param>
    /// <param name="position">The initial position for the soft body.</param>
    /// <param name="rotation">The initial rotation for the soft body.</param>
    /// <returns>A new instance of a soft body.</returns>
    SimpleSoftBody CreateSoftBody(GraphicsDevice graphicsDevice, World world, Vector3 position, Quaternion rotation);
}