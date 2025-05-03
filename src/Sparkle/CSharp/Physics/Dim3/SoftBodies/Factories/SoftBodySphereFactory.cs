using System.Numerics;
using Jitter2;
using Sparkle.CSharp.Physics.Dim3.SoftBodies.Types;
using Veldrid;

namespace Sparkle.CSharp.Physics.Dim3.SoftBodies.Factories;

public class SoftBodySphereFactory : ISoftBodyFactory {
    
    /// <summary>
    /// The size of the soft-body sphere before scaling.
    /// </summary>
    public Vector3 Size { get; private set; }
    
    /// <summary>
    /// The scale applied to the sphere's geometry.
    /// </summary>
    public Vector3 Scale { get; private set; }

    /// <summary>
    /// The number of recursive subdivisions used to refine the sphere's geometry.
    /// </summary>
    public int Subdivisions { get; private set; }
    
    /// <summary>
    /// The mass of each individual vertex in the soft-body mesh.
    /// </summary>
    public float VertexMass { get; private set; }
    
    /// <summary>
    /// The mass of the central body in the soft-body structure.
    /// </summary>
    public float CenterMass { get; private set; }
    
    /// <summary>
    /// The inertia factor of the central body.
    /// </summary>
    public float CenterInertia { get; private set; }
    
    /// <summary>
    /// The softness value used for spring constraints.
    /// </summary>
    public float Softness { get; private set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SoftBodySphereFactory"/> class with parameters used to generate soft-body spheres.
    /// </summary>
    /// <param name="size">The base size of the soft-body sphere.</param>
    /// <param name="scale">Optional scaling factor to apply to the sphere. If null, defaults to <see cref="Vector3.One"/>.</param>
    /// <param name="subdivisions">The number of recursive subdivisions used to refine the unit sphere's mesh.</param>
    /// <param name="vertexMass">The mass to assign to each vertex in the generated soft body. Default is 120.0.</param>
    /// <param name="centerMass">The mass to assign to the central rigid body. Default is 1.0.</param>
    /// <param name="centerInertia">The inertia tensor scale to use for the central body. Default is 0.05.</param>
    /// <param name="softness">The softness value used for spring constraints between the center and vertex bodies. Default is 0.75.</param>
    public SoftBodySphereFactory(Vector3 size, Vector3? scale = null, int subdivisions = 4, float vertexMass = 120.0F, float centerMass = 1.0F, float centerInertia = 0.05F, float softness = 0.75F) {
        this.Size = size;
        this.Scale = scale ?? Vector3.One;
        this.Subdivisions = subdivisions;
        this.VertexMass = vertexMass;
        this.CenterMass = centerMass;
        this.CenterInertia = centerInertia;
        this.Softness = softness;
    }

    /// <summary>
    /// Creates a soft body sphere instance using the specified parameters and factory settings.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used for rendering the soft body sphere.</param>
    /// <param name="world">The physics world where the soft body sphere will reside.</param>
    /// <param name="position">The initial position of the soft body sphere.</param>
    /// <param name="rotation">The initial rotation of the soft body sphere.</param>
    /// <returns>A new instance of <see cref="SimpleSoftBody"/> representing the soft body sphere.</returns>
    public SimpleSoftBody CreateSoftBody(GraphicsDevice graphicsDevice, World world, Vector3 position, Quaternion rotation) {
        return new SoftBodySphere(graphicsDevice, world, position, rotation, this.Size, this.Scale, this.Subdivisions, this.VertexMass, this.CenterMass, this.CenterInertia, this.Softness);
    }
}