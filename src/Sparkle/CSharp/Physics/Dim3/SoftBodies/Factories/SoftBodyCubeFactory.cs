using System.Numerics;
using Jitter2;
using Jitter2.SoftBodies;
using Sparkle.CSharp.Physics.Dim3.SoftBodies.Types;

namespace Sparkle.CSharp.Physics.Dim3.SoftBodies.Factories;

public class SoftBodyCubeFactory : ISoftBodyFactory {

    /// <summary>
    /// The size dimensions of the soft body cube.
    /// </summary>
    public Vector3 Size { get; private set; }
    
    /// <summary>
    /// The mass assigned to each vertex of the cube.
    /// </summary>
    public float VertexMass { get; private set; }
    
    /// <summary>
    /// The mass assigned to the center of the cube.
    /// </summary>
    public float CenterMass { get; private set; }
    
    /// <summary>
    /// The inertia value applied to the center of the cube.
    /// </summary>
    public float CenterInertia { get; private set; }
    
    /// <summary>
    /// Controls how soft or rigid the body behaves.
    /// </summary>
    public float Softness { get; private set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SoftBodyCubeFactory"/> class with the given physical parameters.
    /// </summary>
    /// <param name="size">The size of the cube.</param>
    /// <param name="vertexMass">Mass of each vertex (default is 5.0).</param>
    /// <param name="centerMass">Mass of the cube's center (default is 0.1).</param>
    /// <param name="centerInertia">Inertia of the cube's center (default is 0.05).</param>
    /// <param name="softness">Softness factor of the body (default is 1.0).</param>
    public SoftBodyCubeFactory(Vector3 size, float vertexMass = 5.0F, float centerMass = 0.1F, float centerInertia = 0.05F, float softness = 1.0F) {
        this.Size = size;
        this.VertexMass = vertexMass;
        this.CenterMass = centerMass;
        this.CenterInertia = centerInertia;
        this.Softness = softness;
    }
    
    /// <summary>
    /// Creates a new soft body instance in the given world with specified parameters.
    /// </summary>
    /// <param name="world">The simulation world to which the soft body will be added.</param>
    /// <param name="position">The position where the soft body will be placed.</param>
    /// <param name="rotation">The rotation of the soft body in the world.</param>
    /// <param name="scale">The scale factor for the size of the soft body.</param>
    /// <returns>A new instance of a <see cref="SoftBody"/> initialized with the specified parameters.</returns>
    public SoftBody CreateSoftBody(World world, Vector3 position, Quaternion rotation, Vector3 scale) {
        return new SoftBodyCube(world, position, rotation, scale, this.Size, this.VertexMass, this.CenterMass, this.CenterInertia, this.Softness);
    }
}