using System.Numerics;
using Bliss.CSharp;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Geometry;
using Bliss.CSharp.Graphics.VertexTypes;
using Bliss.CSharp.Materials;
using Jitter2;
using Jitter2.Dynamics;
using Jitter2.Dynamics.Constraints;
using Jitter2.LinearMath;
using Jitter2.SoftBodies;
using Veldrid;

namespace Sparkle.CSharp.Physics.Dim3.SoftBodies.Types;

public class SoftBodyCube : SimpleSoftBody {
    
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
    /// <param name="graphicsDevice">The graphics device used for rendering-related operations.</param>
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
    public SoftBodyCube(GraphicsDevice graphicsDevice, World world, Vector3 position, Quaternion rotation, Vector3 scale, Vector3 size, float vertexMass = 5.0F, float centerMass = 0.1F, float centerInertia = 0.05F, float softness = 1.0F) : base(graphicsDevice, world) {
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
    /// Creates a mesh representation of the soft body cube for rendering purposes.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used for rendering-related operations.</param>
    /// <returns>A new <see cref="Mesh"/> instance representing the soft body cube.</returns>
    protected override Mesh CreateMesh(GraphicsDevice graphicsDevice) {
        Vertex3D[] vertices = new Vertex3D[24];
        uint[] indices = new uint[36];
        
        float uLeft = 0.0F;
        float uRight = 1.0F;
        float vTop = 1.0F;
        float vBottom = 0.0F;
        
        // Generate the 6 faces.
        for (int face = 0; face < 6; face++) {
            
            // Define face vertex indices.
            (int, int, int, int) faceVertexIndices = face switch {
                // Front.
                0 => (0, 1, 5, 4),
                // Back.
                1 => (2, 3, 7, 6),
                // Left.
                2 => (3, 0, 4, 7),
                // Right.
                3 => (1, 2, 6, 5),
                // Top.
                4 => (4, 5, 6, 7),
                // Bottom.
                5 => (3, 2, 1, 0),
                _ => (0, 0, 0, 0)
            };
            
            // Generate the 4 corners for the current face.
            for (int corner = 0; corner < 4; corner++) {
                
                // Calculate the face vertex index.
                int faceVertexIndex = corner switch {
                    0 => faceVertexIndices.Item1,
                    1 => faceVertexIndices.Item2,
                    2 => faceVertexIndices.Item3,
                    3 => faceVertexIndices.Item4,
                    _ => 0
                };
                
                // Calculate texCoords.
                Vector2 texCoord = corner switch {
                    0 => new Vector2(uLeft, vTop),
                    1 => new Vector2(uRight, vTop),
                    2 => new Vector2(uRight, vBottom),
                    3 => new Vector2(uLeft, vBottom),
                    _ => Vector2.Zero
                };
                
                // Add the generated vertex.
                vertices[face * 4 + corner] = new Vertex3D() {
                    Position = this.Vertices[faceVertexIndex].Position,
                    TexCoords = texCoord,
                    Color = Color.White.ToRgbaFloatVec4()
                };
            }
            
            // Add two triangles for the current face.
            int baseIndex = face * 4;
            int indexIndex = face * 6;
            indices[indexIndex + 0] = (uint) (baseIndex + 0);
            indices[indexIndex + 1] = (uint) (baseIndex + 2);
            indices[indexIndex + 2] = (uint) (baseIndex + 1);
            indices[indexIndex + 3] = (uint) (baseIndex + 2);
            indices[indexIndex + 4] = (uint) (baseIndex + 0);
            indices[indexIndex + 5] = (uint) (baseIndex + 3);
        }

        Material material = new Material(graphicsDevice, GlobalResource.DefaultModelEffect);
        
        material.AddMaterialMap(MaterialMapType.Albedo.GetName(), new MaterialMap {
            Texture = GlobalResource.DefaultModelTexture,
            Color = Color.White
        });

        return new Mesh(graphicsDevice, material, vertices, indices);
    }

    /// <summary>
    /// Updates the vertex data of the mesh to synchronize it with the current positions of the vertices in the soft body.
    /// </summary>
    /// <param name="commandList">The command list used for recording graphics commands.</param>
    public override void UpdateMesh(CommandList commandList) {
        float uLeft = 0.0F;
        float uRight = 1.0F;
        float vTop = 1.0F;
        float vBottom = 0.0F;

        // Generate the 6 faces.
        for (int face = 0; face < 6; face++) {
            
            // Define face vertex indices.
            (int, int, int, int) faceVertexIndices = face switch {
                // Front.
                0 => (0, 1, 5, 4),
                // Back.
                1 => (2, 3, 7, 6),
                // Left.
                2 => (3, 0, 4, 7),
                // Right.
                3 => (1, 2, 6, 5),
                // Top.
                4 => (4, 5, 6, 7),
                // Bottom.
                5 => (3, 2, 1, 0),
                _ => (0, 0, 0, 0)
            };
            
            // Generate the 4 corners for the current face.
            for (int corner = 0; corner < 4; corner++) {
                
                // Calculate the face vertex index.
                int faceVertexIndex = corner switch {
                    0 => faceVertexIndices.Item1,
                    1 => faceVertexIndices.Item2,
                    2 => faceVertexIndices.Item3,
                    3 => faceVertexIndices.Item4,
                    _ => 0
                };
                
                // Calculate texCoords.
                Vector2 texCoord = corner switch {
                    0 => new Vector2(uLeft, vTop),
                    1 => new Vector2(uRight, vTop),
                    2 => new Vector2(uRight, vBottom),
                    3 => new Vector2(uLeft, vBottom),
                    _ => Vector2.Zero
                };
                
                // Add the generated vertex.
                this.Mesh.SetVertexValue(face * 4 + corner, new Vertex3D() {
                    Position = this.Vertices[faceVertexIndex].Position,
                    TexCoords = texCoord,
                    Color = Color.White.ToRgbaFloatVec4()
                });
            }
        }

        // Update the vertex buffer.
        this.Mesh.UpdateVertexBuffer(commandList);
    }

    /// <summary>
    /// Renders the debug visualization for the soft body by drawing its edges and center.
    /// </summary>
    /// <param name="drawer">The debug drawer responsible for rendering shapes and points.</param>
    public override void DebugDraw(IDebugDrawer drawer) {
        foreach ((int, int) spring in Edges) {
            drawer.DrawSegment(this.Vertices[spring.Item1].Position, this.Vertices[spring.Item2].Position);
            drawer.DrawPoint(this.Center.Position);
        }
    }

    /// <summary>
    /// Releases all resources used by the <see cref="SoftBodyCube"/> instance.
    /// </summary>
    public override void Dispose() {
        this.Destroy();
        this.world.Remove(this.Center);
        this.Mesh.Dispose();
    }
}