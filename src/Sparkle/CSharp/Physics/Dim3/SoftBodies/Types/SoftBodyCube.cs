using System.Numerics;
using Jitter2;
using Jitter2.Dynamics;
using Jitter2.Dynamics.Constraints;
using Jitter2.LinearMath;
using Jitter2.SoftBodies;

namespace Sparkle.CSharp.Physics.Dim3.SoftBodies.Types;

public class SoftBodyCube : SoftBody, IDebugDrawable, IDisposable {
    
    /// <summary>
    /// Defines the edges connecting the vertices to form a cube.
    /// </summary>
    public static readonly ValueTuple<int, int>[] Edges = [
        (0, 1), (1, 2), (2, 3), (3, 0),
        (4, 5), (5, 6), (6, 7), (7, 4),
        (0, 4), (1, 5), (2, 6), (3, 7)
    ];
    
    /// <summary>
    /// The central rigid body around which the soft body cube is constrained.
    /// </summary>
    public RigidBody Center { get; private set; }

    /// <summary>
    /// The tetrahedrons that compose the internal structure of the soft body cube.
    /// </summary>
    public SoftBodyTetrahedron[] Tetrahedrons { get; private set; }
    
    /// <summary>
    /// The ball socket constraint connecting the center to the vertices.
    /// </summary>
    public BallSocket Constraint { get; private set; }

    /// <summary>
    /// Constructs a new soft body cube within the specified physics world.
    /// </summary>
    /// <param name="world">The physics simulation world.</param>
    /// <param name="position">The initial position of the cube.</param>
    /// <param name="rotation">The rotation of the cube.</param>
    /// <param name="scale">Scale applied to the cube vertices before positioning.</param>
    /// <param name="size">The actual physical size of the cube.</param>
    /// <param name="vertexMass">Mass applied to each vertex body.</param>
    /// <param name="centerMass">Mass of the central rigid body.</param>
    /// <param name="centerInertia">Inertia tensor multiplier for the central body.</param>
    /// <param name="softness">Softness of the constraints connecting vertices to the center.</param>
    /// <exception cref="ArgumentException">Thrown if any size component is less than or equal to zero.</exception>
    public SoftBodyCube(World world, Vector3 position, Quaternion rotation, Vector3 scale, Vector3 size, float vertexMass = 5.0F, float centerMass = 0.1F, float centerInertia = 0.05F, float softness = 1.0F) : base(world) {
        if (size.X <= 0.0F || size.Y <= 0.0F || size.Z <= 0.0F) {
            throw new ArgumentException("Each size component (X, Y, Z) must be greater than zero.", nameof(size));
        }
        
        // Create vertices.
        Vector3[] vertices = [
            new Vector3(1, -1, 1),
            new Vector3(1, -1, -1),
            new Vector3(-1, -1, -1),
            new Vector3(-1, -1, 1),
            new Vector3(1, 1, 1),
            new Vector3(1, 1, -1),
            new Vector3(-1, 1, -1),
            new Vector3(-1, 1, 1)
        ];
        
        // Center pos.
        Vector3 centerPos = Vector3.Zero;

        // Calculate vertices.
        for (int i = 0; i < 8; i++) {
            RigidBody body = world.CreateRigidBody();
            body.SetMassInertia(JMatrix.Zero, vertexMass, true);
            body.Position = (Vector3.Transform(vertices[i] * scale, rotation) + position) * (size / 2.0F);
            centerPos += (Vector3) body.Position;
            this.Vertices.Add(body);
        }
        
        // Create tetrahedrons.
        this.Tetrahedrons = [
            new SoftBodyTetrahedron(this, this.Vertices[0], this.Vertices[1], this.Vertices[5], this.Vertices[2]),
            new SoftBodyTetrahedron(this, this.Vertices[2], this.Vertices[5], this.Vertices[6], this.Vertices[7]),
            new SoftBodyTetrahedron(this, this.Vertices[3], this.Vertices[0], this.Vertices[2], this.Vertices[7]),
            new SoftBodyTetrahedron(this, this.Vertices[0], this.Vertices[4], this.Vertices[5], this.Vertices[7]),
            new SoftBodyTetrahedron(this, this.Vertices[0], this.Vertices[2], this.Vertices[5], this.Vertices[7])
        ];
        
        // Add tetrahedrons.
        for (int i = 0; i < 5; i++) {
            this.Tetrahedrons[i].UpdateWorldBoundingBox();
            world.DynamicTree.AddProxy(this.Tetrahedrons[i]);
            this.Shapes.Add(this.Tetrahedrons[i]);
        }

        // Create center body.
        this.Center = world.CreateRigidBody();
        this.Center.Position = centerPos / 8.0F;
        this.Center.SetMassInertia(JMatrix.Identity * centerInertia, centerMass);

        // Create constraint.
        for (int i = 0; i < 8; i++) {
            this.Constraint = world.CreateConstraint<BallSocket>(this.Center, this.Vertices[i]);
            this.Constraint.Initialize(this.Vertices[i].Position);
            this.Constraint.Softness = softness;
        }
    }
    
    /// <summary>
    /// Renders debug visuals for the cube edges and center using the provided drawer.
    /// </summary>
    /// <param name="drawer">The debug drawer interface to draw points and segments.</param>
    public void DebugDraw(IDebugDrawer drawer) {
        foreach (var spring in Edges) {
            drawer.DrawSegment(this.Vertices[spring.Item1].Position, this.Vertices[spring.Item2].Position);
            drawer.DrawPoint(this.Center.Position);
        }
    }

    /// <summary>
    /// Releases all resources used by the <see cref="SoftBodyCube"/> instance.
    /// </summary>
    public void Dispose() {
        this.Destroy();
        this.world.Remove(this.Center);
    }
}