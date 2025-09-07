using System.Numerics;
using Bliss.CSharp;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Geometry;
using Bliss.CSharp.Graphics.VertexTypes;
using Bliss.CSharp.Materials;
using Jitter2;
using Jitter2.Collision.Shapes;
using Jitter2.Dynamics;
using Jitter2.Dynamics.Constraints;
using Jitter2.LinearMath;
using Jitter2.SoftBodies;
using Sparkle.CSharp.Physics.Dim3.Mappables;
using Veldrid;

namespace Sparkle.CSharp.Physics.Dim3.SoftBodies.Types;

public class SoftBodySphere : SimpleSoftBody {
    
    /// <summary>
    /// The central rigid body of the soft body.
    /// </summary>
    public sealed override RigidBody Center { get; protected set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SoftBodySphere"/> class, constructing a soft-body physics object
    /// in the shape of a sphere using a triangulated hull based on a subdivided unit sphere.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used for rendering.</param>
    /// <param name="world">The physics simulation world to which the soft body belongs.</param>
    /// <param name="position">The world position where the soft body should be placed.</param>
    /// <param name="rotation">The rotation to apply to the soft body sphere.</param>
    /// <param name="size">The base size of the sphere before scaling and rotation.</param>
    /// <param name="scale">The scaling factor to apply to the size of the sphere.</param>
    /// <param name="subdivisions">The number of recursive subdivisions used to refine the unit sphere's mesh.</param>
    /// <param name="vertexMass">The mass of each vertex rigid body. Default is 120.0.</param>
    /// <param name="centerMass">The mass of the central rigid body connecting all vertices. Default is 1.0.</param>
    /// <param name="centerInertia">The inertia tensor scaling factor for the central body. Default is 0.05.</param>
    /// <param name="softness">The softness value used for spring constraints between the center and vertex bodies. Default is 0.5.</param>
    public SoftBodySphere(GraphicsDevice graphicsDevice, World world, Vector3 position, Quaternion rotation, Vector3 size, Vector3 scale, int subdivisions = 4, float vertexMass = 200.0F, float centerMass = 1.0F, float centerInertia = 0.05F, float softness = 0.75F) : base(graphicsDevice, world) {
        List<JTriangle> triangles = ShapeHelper.MakeHull(new UnitSphere(), subdivisions)
            .Select(t => new JTriangle(
                Vector3.Transform(t.V0, rotation) * (size / 2 * scale),
                Vector3.Transform(t.V1, rotation) * (size / 2 * scale),
                Vector3.Transform(t.V2, rotation) * (size / 2 * scale))
            ).ToList();
        
        // Process triangles.
        Dictionary<JVector, ushort> vertexIndices = new Dictionary<JVector, ushort>();
        List<JVector> vertices = new List<JVector>();
        
        foreach (JTriangle triangle in triangles) {
            if (!vertexIndices.ContainsKey(triangle.V0)) {
                vertexIndices[triangle.V0] = (ushort) vertices.Count;
                vertices.Add(triangle.V0);
            }
            
            if (!vertexIndices.ContainsKey(triangle.V1)) {
                vertexIndices[triangle.V1] = (ushort) vertices.Count;
                vertices.Add(triangle.V1);
            }
            
            if (!vertexIndices.ContainsKey(triangle.V2)) {
                vertexIndices[triangle.V2] = (ushort) vertices.Count;
                vertices.Add(triangle.V2);
            }
        }
        
        Vector3 centerPos = Vector3.Zero;
        
        // Create rigid bodies for vertices.
        foreach (Vector3 vertex in vertices) {
            RigidBody body = world.CreateRigidBody();
            body.SetMassInertia(JMatrix.Zero, vertexMass, true);
            body.Position = vertex + position;
            centerPos += (Vector3) body.Position;
            this.Vertices.Add(body);
        }
        
        // Create a center body.
        this.Center = world.CreateRigidBody();
        this.Center.Position = centerPos / vertices.Count;
        this.Center.Orientation = rotation;
        this.Center.SetMassInertia(JMatrix.Identity * centerInertia, centerMass);
        
        // Create constraints between center and vertices.
        foreach (RigidBody vertex in this.Vertices) {
            var constraint = world.CreateConstraint<BallSocket>(this.Center, vertex);
            constraint.Initialize(vertex.Position);
            constraint.Softness = softness;
            this.Springs.Add(constraint);
        }
        
        // Create soft body triangles.
        foreach (JTriangle triangle in triangles) {
            SoftBodyTriangle softBodyTriangle = new SoftBodyTriangle(this, this.Vertices[vertexIndices[triangle.V0]], this.Vertices[vertexIndices[triangle.V1]], this.Vertices[vertexIndices[triangle.V2]]);
            softBodyTriangle.UpdateWorldBoundingBox();
            world.DynamicTree.AddProxy(softBodyTriangle);
            this.Shapes.Add(softBodyTriangle);
        }
    }

    /// <summary>
    /// Creates a mesh representing the current soft body structure based on its geometry and material properties.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used to create the mesh and render its contents.</param>
    /// <returns>A mesh constructed from the soft body geometry, configured with vertices, indices, and material.</returns>
    protected override Mesh CreateMesh(GraphicsDevice graphicsDevice) {
        Vertex3D[] vertices = new Vertex3D[this.Shapes.Count * 3];
        uint[] indices = new uint[this.Shapes.Count * 3];
        
        // Calculate vertices and indices.
        for (int i = 0; i < this.Shapes.Count; i++) {
            if (this.Shapes[i] is SoftBodyTriangle triangle) {
                int vertexIndex = i * 3;
                
                Vector2 uv0 = this.CalculateSphereUv(triangle.Vertex1.Position);
                Vector2 uv1 = this.CalculateSphereUv(triangle.Vertex2.Position);
                Vector2 uv2 = this.CalculateSphereUv(triangle.Vertex3.Position);

                // Check if any of the UVs cross the seam (difference > 0.5).
                if (MathF.Abs(uv0.X - uv1.X) > 0.5f || MathF.Abs(uv1.X - uv2.X) > 0.5f || MathF.Abs(uv2.X - uv0.X) > 0.5f) {
                    if (uv0.X < 0.5f) uv0.X += 1.0f;
                    if (uv1.X < 0.5f) uv1.X += 1.0f;
                    if (uv2.X < 0.5f) uv2.X += 1.0f;
                }

                vertices[vertexIndex] = new Vertex3D {
                    Position = triangle.Vertex1.Position,
                    TexCoords = uv0,
                    Color = Color.White.ToRgbaFloatVec4()
                };
                
                vertices[vertexIndex + 1] = new Vertex3D {
                    Position = triangle.Vertex2.Position,
                    TexCoords = uv1,
                    Color = Color.White.ToRgbaFloatVec4()
                };
                
                vertices[vertexIndex + 2] = new Vertex3D {
                    Position = triangle.Vertex3.Position,
                    TexCoords = uv2,
                    Color = Color.White.ToRgbaFloatVec4()
                };
                
                indices[vertexIndex] = (uint) vertexIndex;
                indices[vertexIndex + 1] = (uint) (vertexIndex + 2);
                indices[vertexIndex + 2] = (uint) (vertexIndex + 1);
            }
        }
        
        Material material = new Material(GlobalResource.DefaultModelEffect);
        
        material.AddMaterialMap(MaterialMapType.Albedo, new MaterialMap {
            Texture = GlobalResource.DefaultModelTexture,
            Color = Color.White
        });
        
        return new Mesh(graphicsDevice, material, vertices, indices);
    }

    /// <summary>
    /// Updates the mesh to reflect the current positions of the vertices in the soft body structure.
    /// </summary>
    /// <param name="commandList">The command list used to update the vertex buffer for rendering.</param>
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
    /// Renders debug visualizations for the soft body.
    /// </summary>
    /// <param name="drawer">The debug drawer used to render the visualizations.</param>
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

    /// <summary>
    /// Calculates the UV coordinates for a point on the surface of a sphere based on its 3D position.
    /// The UV mapping is determined by converting the position into a unit vector and computing spherical angles.
    /// </summary>
    /// <param name="position">The 3D world position of the point on the sphere.</param>
    /// <returns>A 2D vector representing the UV coordinates, where U and V are in the range [0, 1].</returns>
    private Vector2 CalculateSphereUv(Vector3 position) {
        
        // Apply the rotation to the position relative to the sphere's center
        Vector3 rotatedPosition = Vector3.Transform(position - (Vector3) this.Center.Position, this.Center.Orientation);
    
        // Convert world position into a unit vector around the sphere center.
        Vector3 local = Vector3.Normalize(rotatedPosition);

        // Compute angles: θ around the Y-axis, φ from the Y-axis down.
        float theta = MathF.Atan2(local.Z, local.X);
        float phi = MathF.Acos(local.Y);

        // Convert angles to UV range [0,1].
        float u = (theta + MathF.PI) / (2.0F * MathF.PI);
        float v = phi / MathF.PI;

        // Wrap U into [0,1) to handle the seam.
        u = (u + 0.5F) % 1.0F;
        
        return new Vector2(u, v);
    }
}