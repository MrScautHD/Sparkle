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

public class SoftBodyCloth : SimpleSoftBody {
    
    /// <summary>
    /// The central rigid body of the soft body.
    /// </summary>
    public sealed override RigidBody Center { get; protected set; }
    
    /// <summary>
    /// Indicates whether the cloth mesh should use a grid texture layout for texture coordinates.
    /// </summary>
    private readonly bool _useGridTexture;

    /// <summary>
    /// The minimum position of the cloth's vertices within the soft body structure.
    /// </summary>
    private Vector3 _minBodyPosition;
    
    /// <summary>
    /// The maximum position of the cloth's vertices within the soft body structure.
    /// </summary>
    private Vector3 _maxBodyPosition;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SoftBodyCloth"/> class.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used for mesh creation.</param>
    /// <param name="world">The physics world in which the cloth is simulated.</param>
    /// <param name="position">The position of the cloth in world space.</param>
    /// <param name="rotation">The rotation applied to the cloth's initial layout.</param>
    /// <param name="vertexSpacing">The spacing between vertices in the grid.</param>
    /// <param name="gridWidth">The number of columns in the cloth grid.</param>
    /// <param name="gridHeight">The number of rows in the cloth grid.</param>
    /// <param name="vertexMass">The mass of each individual vertex.</param>
    /// <param name="centerMass">The mass of the central rigid body.</param>
    /// <param name="centerInertia">The inertia of the central rigid body.</param>
    /// <param name="softness">The softness factor applied to the spring constraints.</param>
    /// <param name="useDynamicCenterVertexSelection">Whether to automatically choose multiple closest vertices to connect to the center.</param>
    /// <param name="useGridTexture">Whether to use a tiled grid texture layout for UV coordinates.</param>
    public SoftBodyCloth(GraphicsDevice graphicsDevice, World world, Vector3 position, Quaternion rotation, Vector2 vertexSpacing, int gridWidth, int gridHeight, float vertexMass = 10.0F, float centerMass = 1.0F, float centerInertia = 0.05F, float softness = 0.2F, bool useDynamicCenterVertexSelection = true, bool useGridTexture = false) : base(graphicsDevice, world) {
        this._useGridTexture = useGridTexture;
        List<JTriangle> triangles = new List<JTriangle>();
        
        float halfGridWidth = gridWidth / 2.0F;
        float halfGridHeight = gridHeight / 2.0F;
        
        // Create the cloth's triangle geometry.
        for (int row = 0; row < gridWidth; row++) {
            for (int col = 0; col < gridHeight; col++) {
                float x0 = (-halfGridHeight + col) * (vertexSpacing.X * 0.1F);
                float x1 = (-halfGridHeight + col + 1.0F) * (vertexSpacing.X * 0.1F);
                float z0 = (-halfGridWidth + row) * (vertexSpacing.Y * 0.1F);
                float z1 = (-halfGridWidth + row + 1.0F) * (vertexSpacing.Y * 0.1F);

                Vector3 v0 = Vector3.Transform(new Vector3(x0, 0.0F, z0), rotation) + position / 2.0F;
                Vector3 v1 = Vector3.Transform(new Vector3(x0, 0.0F, z1), rotation) + position / 2.0F;
                Vector3 v2 = Vector3.Transform(new Vector3(x1, 0.0F, z0), rotation) + position / 2.0F;
                Vector3 v3 = Vector3.Transform(new Vector3(x1, 0.0F, z1), rotation) + position / 2.0F;

                bool isEven = (col + row) % 2 == 0;

                if (isEven) {
                    triangles.Add(new JTriangle(v0, v2, v1));
                    triangles.Add(new JTriangle(v2, v3, v1));
                }
                else {
                    triangles.Add(new JTriangle(v0, v3, v1));
                    triangles.Add(new JTriangle(v0, v2, v3));
                }
            }
        }
        
        Dictionary<JVector, ushort> vertexIndices = new Dictionary<JVector, ushort>();
        List<(ushort, ushort)> edges = new List<(ushort, ushort)>();
        List<JVector> vertices = new List<JVector>();
        List<(ushort, ushort, ushort)> tris = new List<(ushort, ushort, ushort)>();

        // Process triangles.
        foreach (JTriangle tri in triangles) {
            if (!vertexIndices.TryGetValue(tri.V0, out ushort u0)) {
                u0 = (ushort) vertexIndices.Count;
                vertexIndices[tri.V0] = u0;
                vertices.Add(tri.V0);
            }

            if (!vertexIndices.TryGetValue(tri.V1, out ushort u1)) {
                u1 = (ushort) vertexIndices.Count;
                vertexIndices[tri.V1] = u1;
                vertices.Add(tri.V1);
            }

            if (!vertexIndices.TryGetValue(tri.V2, out ushort u2)) {
                u2 = (ushort) vertexIndices.Count;
                vertexIndices[tri.V2] = u2;
                vertices.Add(tri.V2);
            }

            // Add triangles.
            tris.Add((u0, u1, u2));

            // Add edges.
            edges.Add((u0, u1));
            edges.Add((u0, u2));
            edges.Add((u1, u2));
        }
        
        Vector3 centerPos = Vector3.Zero;

        // Create rigid bodies for vertices.
        foreach (Vector3 vertex in vertices) {
            RigidBody body = world.CreateRigidBody();
            body.SetMassInertia(JMatrix.Zero, vertexMass, true);
            body.Position = vertex;
            centerPos += vertex;
            this.Vertices.Add(body);
        }
        
        // Set min and max body pos.
        this._minBodyPosition = this.Vertices.First().Position;
        this._maxBodyPosition = this.Vertices.Last().Position;

        // Create a center body.
        this.Center = world.CreateRigidBody();
        this.Center.SetMassInertia(JMatrix.Identity * centerInertia, centerMass);
        this.Center.Position = centerPos / vertices.Count;

        int vertexSelectionCount = 1;

        if (useDynamicCenterVertexSelection) {
            bool widthIsOdd = gridWidth % 2 != 0;
            bool heightIsOdd = gridHeight % 2 != 0;
        
            if (widthIsOdd && heightIsOdd) {
                vertexSelectionCount = 4;
            }
            else if (!widthIsOdd && !heightIsOdd) {
                vertexSelectionCount = 1;
            }
            else {
                vertexSelectionCount = 2;
            }
        }
        
        List<RigidBody> closestCenterVertices = this.Vertices
            .OrderBy(vertex => Vector3.DistanceSquared(centerPos / vertices.Count, vertex.Position))
            .Take(vertexSelectionCount)
            .ToList();
        
        foreach (RigidBody vertex in closestCenterVertices) {
            BallSocket ballSocket = world.CreateConstraint<BallSocket>(this.Center, vertex);
            ballSocket.Initialize(this.Center.Position);
        }
        
        // Create constraints.
        foreach ((ushort, ushort) edge in edges) {
            SpringConstraint constraint = world.CreateConstraint<SpringConstraint>(this.Vertices[edge.Item1], this.Vertices[edge.Item2]);
            constraint.Initialize(this.Vertices[edge.Item1].Position, this.Vertices[edge.Item2].Position);
            constraint.Softness = softness;
            this.Springs.Add(constraint);
        }
        
        // Create soft body triangles.
        foreach ((ushort, ushort, ushort) triangle in tris) {
            SoftBodyTriangle bodyTriangle = new SoftBodyTriangle(this, this.Vertices[triangle.Item1], this.Vertices[triangle.Item2], this.Vertices[triangle.Item3]);
            bodyTriangle.UpdateWorldBoundingBox();
            world.DynamicTree.AddProxy(bodyTriangle);
            this.Shapes.Add(bodyTriangle);
        }
    }

    /// <summary>
    /// Creates a mesh representation for the soft body cloth.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used for creating the mesh.</param>
    /// <returns>A <see cref="Mesh"/> object containing the vertices, indices, and material for the soft body cloth.</returns>
    protected override Mesh CreateMesh(GraphicsDevice graphicsDevice) {
        Vertex3D[] vertices = new Vertex3D[this.Shapes.Count * 3];
        uint[] indices = new uint[this.Shapes.Count * 3];
        
        // Calculate vertices and indices.
        for (int i = 0; i < this.Shapes.Count; i++) {
            if (this.Shapes[i] is SoftBodyTriangle triangle) {
                int vertexIndex = i * 3;
                
                Vector2 texCoord1;
                Vector2 texCoord2;
                Vector2 texCoord3;
                
                if (this._useGridTexture) {
                    texCoord1 = new Vector2(triangle.Vertex1.Position.X, 1.0F - triangle.Vertex1.Position.Z);
                    texCoord2 = new Vector2(triangle.Vertex2.Position.X, 1.0F - triangle.Vertex2.Position.Z);
                    texCoord3 = new Vector2(triangle.Vertex3.Position.X, 1.0F - triangle.Vertex3.Position.Z);
                }
                else {
                    float minX = this._minBodyPosition.X;
                    float maxX = this._maxBodyPosition.X;
                    float minZ = this._minBodyPosition.Z;
                    float maxZ = this._maxBodyPosition.Z;

                    float width = maxX - minX;
                    float height = maxZ - minZ;
                    
                    texCoord1 = new Vector2(
                        (triangle.Vertex1.Position.X - minX) / width,
                        1.0F - (triangle.Vertex1.Position.Z - minZ) / height
                    );
                    
                    texCoord2 = new Vector2(
                        (triangle.Vertex2.Position.X - minX) / width,
                        1.0F - (triangle.Vertex2.Position.Z - minZ) / height
                    );
                    
                    texCoord3 = new Vector2(
                        (triangle.Vertex3.Position.X - minX) / width,
                        1.0F - (triangle.Vertex3.Position.Z - minZ) / height
                    );
                }
                
                vertices[vertexIndex] = new Vertex3D {
                    Position = triangle.Vertex1.Position,
                    TexCoords = texCoord1,
                    Color = Color.White.ToRgbaFloatVec4()
                };
                
                vertices[vertexIndex + 1] = new Vertex3D {
                    Position = triangle.Vertex2.Position,
                    TexCoords = texCoord2,
                    Color = Color.White.ToRgbaFloatVec4()
                };
                
                vertices[vertexIndex + 2] = new Vertex3D {
                    Position = triangle.Vertex3.Position,
                    TexCoords = texCoord3,
                    Color = Color.White.ToRgbaFloatVec4()
                };
                
                indices[vertexIndex] = (uint) vertexIndex;
                indices[vertexIndex + 1] = (uint) (vertexIndex + 1);
                indices[vertexIndex + 2] = (uint) (vertexIndex + 2);
            }
        }
        
        Material material = new Material(graphicsDevice, GlobalResource.DefaultModelEffect);
        
        material.AddMaterialMap(MaterialMapType.Albedo.GetName(), new MaterialMap {
            Texture = GlobalResource.DefaultModelTexture,
            Color = Color.White
        });

        return new Mesh(graphicsDevice, material, vertices, indices);
    }

    /// <summary>
    /// Updates the mesh data of the soft body cloth by recalculating vertex positions, texture coordinates, and colors.
    /// </summary>
    /// <param name="commandList">The command list used to update the vertex buffer with the new mesh data.</param>
    protected internal override void UpdateMesh(CommandList commandList) {
        for (int i = 0; i < this.Shapes.Count; i++) {
            if (this.Shapes[i] is SoftBodyTriangle triangle) {
                int vertexIndex = i * 3;
                
                this.Mesh.SetVertexValue(vertexIndex, new Vertex3D {
                    Position = this.GetLerpedVertexPos(this.Vertices.IndexOf(triangle.Vertex1)),
                    TexCoords = this.Mesh.Vertices[vertexIndex].TexCoords,
                    Color = Color.White.ToRgbaFloatVec4()
                });
                
                this.Mesh.SetVertexValue(vertexIndex + 1, new Vertex3D {
                    Position = this.GetLerpedVertexPos(this.Vertices.IndexOf(triangle.Vertex2)),
                    TexCoords = this.Mesh.Vertices[vertexIndex + 1].TexCoords,
                    Color = Color.White.ToRgbaFloatVec4()
                });
                
                this.Mesh.SetVertexValue(vertexIndex + 2, new Vertex3D {
                    Position = this.GetLerpedVertexPos(this.Vertices.IndexOf(triangle.Vertex3)),
                    TexCoords = this.Mesh.Vertices[vertexIndex + 2].TexCoords,
                    Color = Color.White.ToRgbaFloatVec4()
                });
            }
        }

        this.Mesh.UpdateVertexBuffer(commandList);
    }

    /// <summary>
    /// Draws debug visualization of the soft body cloth, including its triangles, springs, and center point.
    /// </summary>
    /// <param name="drawer">The debug drawer used for rendering visual debug information.</param>
    public override void DebugDraw(IDebugDrawer drawer) {
        
        // Draw shapes.
        foreach (SoftBodyShape shape in this.Shapes) {
            if (shape is SoftBodyTriangle triangle) {
                drawer.DrawSegment(triangle.Vertex1.Position, triangle.Vertex2.Position);
                drawer.DrawSegment(triangle.Vertex2.Position, triangle.Vertex3.Position);
                drawer.DrawSegment(triangle.Vertex3.Position, triangle.Vertex1.Position);
            }
        }
        
        // Draw springs.
        foreach (Constraint spring in this.Springs) {
            drawer.DrawSegment(spring.Body1.Position, spring.Body2.Position);
        }
        
        // Draw center point.
        drawer.DrawPoint(this.Center.Position);
    }
}