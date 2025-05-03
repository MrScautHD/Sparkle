using System.Numerics;
using Jitter2;
using Sparkle.CSharp.Physics.Dim3.SoftBodies.Types;
using Veldrid;

namespace Sparkle.CSharp.Physics.Dim3.SoftBodies.Factories;

public class SoftBodyCubeFactory : ISoftBodyFactory {

    /// <summary>
    /// The size dimensions of the soft body cube.
    /// </summary>
    public Vector3 Size { get; private set; }

    /// <summary>
    /// The scale factor applied to the soft body dimensions during creation.
    /// </summary>
    public Vector3 Scale { get; private set; }
    
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
    /// <param name="scale">The scale of the cube.</param>
    /// <param name="vertexMass">Mass of each vertex (default is 5.0).</param>
    /// <param name="centerMass">Mass of the cube's center (default is 0.1).</param>
    /// <param name="centerInertia">Inertia of the cube's center (default is 0.05).</param>
    /// <param name="softness">Softness factor of the body (default is 1.0).</param>
    public SoftBodyCubeFactory(Vector3 size, Vector3? scale = null, float vertexMass = 5.0F, float centerMass = 0.1F, float centerInertia = 0.05F, float softness = 1.0F) {
        this.Size = size;
        this.Scale = scale ?? Vector3.One;
        this.VertexMass = vertexMass;
        this.CenterMass = centerMass;
        this.CenterInertia = centerInertia;
        this.Softness = softness;
    }

    /// <summary>
    /// Creates a new soft body instance in the given world with specified parameters.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used to render the soft body.</param>
    /// <param name="world">The simulation world to which the soft body will be added.</param>
    /// <param name="position">The position where the soft body will be placed.</param>
    /// <param name="rotation">The rotation of the soft body in the world.</param>
    /// <returns>A new instance of a <see cref="SimpleSoftBody"/> initialized with the specified parameters.</returns>
    public SimpleSoftBody CreateSoftBody(GraphicsDevice graphicsDevice, World world, Vector3 position, Quaternion rotation) {
        return new SoftBodyCube(graphicsDevice, world, position, rotation, this.Size, this.Scale, this.VertexMass, this.CenterMass, this.CenterInertia, this.Softness);
    }
}