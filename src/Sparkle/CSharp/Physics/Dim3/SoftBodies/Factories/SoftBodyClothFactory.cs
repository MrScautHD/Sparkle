using System.Numerics;
using Jitter2;
using Sparkle.CSharp.Physics.Dim3.SoftBodies.Types;
using Veldrid;

namespace Sparkle.CSharp.Physics.Dim3.SoftBodies.Factories;

public class SoftBodyClothFactory : ISoftBodyFactory {

    /// <summary>
    /// The width of the cloth grid.
    /// </summary>
    public int GirdWidth { get; private set; }
    
    /// <summary>
    /// The height of the cloth grid.
    /// </summary>
    public int GirdHeight { get; private set; }
    
    /// <summary>
    /// The vertex spacing of the cloth grid.
    /// </summary>
    public Vector2 VertexSpacing { get; private set; }

    /// <summary>
    /// The mass of each vertex in the cloth simulation.
    /// </summary>
    public float VertexMass { get; private set; }

    /// <summary>
    /// The mass of the center of the cloth.
    /// </summary>
    public float CenterMass { get; private set; }

    /// <summary>
    /// The inertia of the center of the cloth.
    /// </summary>
    public float CenterInertia { get; private set; }
    
    /// <summary>
    /// The softness of the cloth simulation.
    /// </summary>
    public float Softness { get; private set; }

    /// <summary>
    /// Indicates whether to dynamically select the center vertex for the cloth simulation.
    /// </summary>
    public bool UseDynamicCenterVertexSelection { get; private set; }

    /// <summary>
    /// Indicates whether to use a tiled grid texture layout for UV coordinates.
    /// </summary>
    public bool UseGridTexture { get; private set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SoftBodyClothFactory"/> class with the specified cloth simulation parameters.
    /// </summary>
    /// <param name="girdWidth">The number of vertices along the width of the cloth grid.</param>
    /// <param name="girdHeight">The number of vertices along the height of the cloth grid.</param>
    /// <param name="vertexSpacing">The spacing between adjacent vertices in the cloth grid.</param>
    /// <param name="vertexMass">The mass of each vertex in the cloth grid.</param>
    /// <param name="centerMass">The mass of the center vertex (used to stabilize the cloth).</param>
    /// <param name="centerInertia">The inertia of the center vertex.</param>
    /// <param name="softness">A factor that determines the elasticity or softness of the cloth.</param>
    /// <param name="useDynamicCenterVertexSelection">Determines whether the center vertex should be dynamically selected based on vertex positions.</param>
    /// <param name="useGridTexture">Whether to use a tiled grid texture layout for UV coordinates.</param>
    public SoftBodyClothFactory(int girdWidth, int girdHeight, Vector2 vertexSpacing, float vertexMass = 10.0F, float centerMass = 1.0F, float centerInertia = 0.05F, float softness = 0.2F, bool useDynamicCenterVertexSelection = true, bool useGridTexture = false) {
        this.GirdWidth = girdWidth;
        this.GirdHeight = girdHeight;
        this.VertexSpacing = vertexSpacing;
        this.VertexMass = vertexMass;
        this.CenterMass = centerMass;
        this.CenterInertia = centerInertia;
        this.Softness = softness;
        this.UseDynamicCenterVertexSelection = useDynamicCenterVertexSelection;
        this.UseGridTexture = useGridTexture;
    }

    /// <summary>
    /// Creates a new soft body cloth simulation in the given world with specified parameters.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used for rendering.</param>
    /// <param name="world">The simulation world where the soft body will be created.</param>
    /// <param name="position">The position where the cloth will be placed.</param>
    /// <param name="rotation">The rotation of the cloth in the world.</param>
    /// <returns>A new instance of <see cref="SimpleSoftBody"/> representing the soft body cloth.</returns>
    public SimpleSoftBody CreateSoftBody(GraphicsDevice graphicsDevice, World world, Vector3 position, Quaternion rotation) {
        return new SoftBodyCloth(graphicsDevice, world, position, rotation, this.GirdWidth, this.GirdHeight, this.VertexSpacing, this.VertexMass, this.CenterMass, this.CenterInertia, this.Softness, this.UseDynamicCenterVertexSelection, this.UseGridTexture);
    }
}