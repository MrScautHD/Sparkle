using System.Numerics;
using LibNoise;
using LibNoise.Primitive;
using Raylib_cs;
using Sparkle.CSharp.Rendering.Helpers;

namespace Sparkle.CSharp.Terrain;

public class MarchingCubes {

    private SimplexPerlin _perlin;
    
    private int _width;
    private int _height;
    
    private float _scale;
    private float _heightThreshold;

    private bool _use3DNoise;
    
    private List<Vector3> _vertices;
    private List<Vector3> _normals;
    private List<Vector2> _texCoords;
    private List<ushort> _triangles;
    private float[,,] _heights;
    
    /// <summary>
    /// Initializes a new instance of the MarchingCubes class with the specified parameters.
    /// </summary>
    /// <param name="seed">Seed value for initializing the Perlin noise generator.</param>
    /// <param name="width">Width of the 3D grid.</param>
    /// <param name="height">Height of the 3D grid.</param>
    /// <param name="scale">Scaling factor applied to the grid.</param>
    /// <param name="heightThreshold">Threshold value determining the surface generation.</param>
    /// <param name="use3DNoise">Optional parameter indicating whether to use 3D noise for generating terrain.</param>
    public MarchingCubes(int seed, int width, int height, float scale, float heightThreshold, bool use3DNoise = false) {
        this._perlin = new SimplexPerlin(seed, NoiseQuality.Best);
        this._width = width;
        this._height = height;
        this._scale = scale;
        this._heightThreshold = heightThreshold;
        this._use3DNoise = use3DNoise;
        this._vertices = new List<Vector3>();
        this._normals = new List<Vector3>();
        this._texCoords = new List<Vector2>();
        this._triangles = new List<ushort>();
    }
    
    /// <summary>
    /// Sets heights for the terrain within a specified region.
    /// </summary>
    /// <param name="position">The position of the starting corner of the region.</param>
    /// <param name="width">The width of the region in units.</param>
    /// <param name="height">The height of the region in units.</param>
    public void SetHeights(Vector3 position, int width, int height) {
        this._heights = new float[width + 1, height + 1, width + 1];

        for (int x = 0; x < width + 1; x++) {
            for (int y = 0; y < height + 1; y++) {
                for (int z = 0; z < width + 1; z++) {
                    Vector3 worldPos = new Vector3(position.X + x, position.Y + y, position.Z + z);

                    if (this._use3DNoise) {
                        this._heights[x, y, z] = this._perlin.GetValue(worldPos.X / this._width * this._scale, worldPos.Y / this._height * this._scale, worldPos.Z / this._width * this._scale);
                    }
                    else {
                        float currentHeight = this._height * this._perlin.GetValue(worldPos.X * this._scale, worldPos.Z * this._scale);
                        float distToSurface;

                        if (worldPos.Y <= currentHeight - 0.5f) {
                            distToSurface = 0f;
                        }
                        else if (worldPos.Y > currentHeight + 0.5f) {
                            distToSurface = 1f;
                        }
                        else if (worldPos.Y > currentHeight) {
                            distToSurface = worldPos.Y - currentHeight;
                        }
                        else {
                            distToSurface = currentHeight - worldPos.Y;
                        }

                        this._heights[x, y, z] = distToSurface;
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Marches cubes within a specified region to generate terrain.
    /// </summary>
    /// <param name="position">The position of the starting corner of the region.</param>
    /// <param name="width">The width of the region in units.</param>
    /// <param name="height">The height of the region in units.</param>
    public void MarchCubes(Vector3 position, int width, int height) {
        this._vertices.Clear();
        this._normals.Clear();
        this._texCoords.Clear();
        this._triangles.Clear();
        
        float[] corners = new float[8];
        
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                for (int z = 0; z < width; z++) {
                    
                    for (int i = 0; i < 8; i++) {
                        Vector3 corner = new Vector3(x, y, z) + MarchingCubesTables.Corners[i];
                        corners[i] = this._heights[(int) corner.X, (int) corner.Y, (int) corner.Z];
                    }

                    this.MarchCube(new Vector3(position.X + x, position.Y + y, position.Z + z), corners);
                }
            }
        }
    }

    /// <summary>
    /// Marches a cube at the given position using the Marching Cubes algorithm, generating the mesh vertices, normals, and triangles.
    /// </summary>
    /// <param name="position">The position of the cube in 3D space.</param>
    /// <param name="corners">The array containing the values of the cube corners.</param>
    private void MarchCube(Vector3 position, float[] corners) {
        int index = this.GetConfigIndex(corners);

        if (index == 0 || index == 255) {
            return;
        }

        int edgeIndex = 0;
        
        for (int t = 0; t < 5; t++) {
            for (int v = 0; v < 3; v++) {
                int triTableValue = MarchingCubesTables.Triangles[index, edgeIndex];

                if (triTableValue == -1) {
                    return;
                }

                Vector3 edgeStart = position + MarchingCubesTables.Edges[triTableValue, 0];
                Vector3 edgeEnd = position + MarchingCubesTables.Edges[triTableValue, 1];

                Vector3 vertex = (edgeStart + edgeEnd) / 2;
                Vector3 normal = Vector3.Normalize(edgeEnd - edgeStart);

                this._vertices.Add(vertex);
                this._normals.Add(normal);
                this._texCoords.Add(new Vector2(edgeIndex / (float) this._width, edgeIndex / (float) this._height));
                this._triangles.Add((ushort) (this._vertices.Count - 1));

                edgeIndex++;
            }
        }
    }
    
    /// <summary>
    /// Generates a mesh from the Marching Cubes algorithm using the stored vertices, normals, and triangles.
    /// </summary>
    /// <returns>A Model object representing the generated mesh.</returns>
    public Model GenerateModel() {
        Mesh mesh = new Mesh(this._vertices.Count, this._triangles.Count);
        mesh.AllocVertices();
        mesh.AllocNormals();
        mesh.AllocTexCoords();
        mesh.AllocIndices();
        
        Span<Vector3> meshVertices = mesh.VerticesAs<Vector3>();
        Span<Vector3> meshNormals = mesh.NormalsAs<Vector3>();
        Span<Vector2> meshTexCoords = mesh.TexCoordsAs<Vector2>();
        Span<ushort> meshTriangle = mesh.IndicesAs<ushort>();
        
        for (int i = 0; i < this._vertices.Count; i++) {
            meshVertices[i] = this._vertices[i];
        }
        
        for (int i = 0; i < this._normals.Count; i++) {
            meshNormals[i] = this._normals[i];
        }
        
        for (int i = 0; i < this._texCoords.Count; i++) {
            meshTexCoords[i] = this._texCoords[i];
        }

        for (int i = 0; i < this._triangles.Count; i++) {
            meshTriangle[i] = this._triangles[i];
        }
        
        MeshHelper.Upload(ref mesh, false); // UPDATE BUFFER FOR TERRAIN MANIPLUATION
        
        return ModelHelper.LoadFromMesh(mesh);
    }

    /// <summary>
    /// Update the mesh by updating the vertex buffer with the new vertices data.
    /// </summary>
    /// <param name="mesh">The mesh to update.</param>
    private unsafe void UpdateMesh(Mesh mesh) {
        fixed (Vector3* verticesPtr = this._vertices.ToArray()) {
            for (int i = 0; i < this._vertices.Count; i++) {
                MeshHelper.UpdateBuffer(mesh, i, verticesPtr, this._vertices.Count * sizeof(Vector3), 0);
            }
        }
    }
    
    /// <summary>
    /// Retrieves the configuration index for the given cube corners.
    /// </summary>
    /// <param name="corners">The array containing the values of the cube corners.</param>
    /// <returns>The configuration index for the cube corners.</returns>
    private int GetConfigIndex(float[] corners) {
        int index = 0;

        for (int i = 0; i < 8; i++) {
            if (corners[i] > this._heightThreshold) {
                index |= 1 << i;
            }
        }

        return index;
    }
}