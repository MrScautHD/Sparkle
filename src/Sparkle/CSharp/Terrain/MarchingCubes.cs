using System.Numerics;
using Raylib_cs;
using Sparkle.CSharp.Rendering.Helpers;

namespace Sparkle.CSharp.Terrain;

public class MarchingCubes {

    private PerlinNoise _perlinNoise;
    
    private int _width;
    private int _height;
    
    private float _resolution;
    private float _scale;
    private float _heightThreshold;

    private bool _use3DNoise;
    
    private List<Vector3> _vertices;
    private List<Vector3> _normals;
    private List<Vector2> _texCoords;
    private List<ushort> _triangles;
    private float[,,] _heights;
    
    public bool HasInitialized { get; private set; }

    /// <summary>
    /// Initializes a new instance of the MarchingCubes class with the specified parameters.
    /// </summary>
    /// <param name="seed">Seed value for initializing the Perlin noise generator.</param>
    /// <param name="width">Width of the 3D grid.</param>
    /// <param name="height">Height of the 3D grid.</param>
    /// <param name="resolution">Resolution of the grid.</param>
    /// <param name="scale">Scaling factor applied to the grid.</param>
    /// <param name="heightThreshold">Threshold value determining the surface generation.</param>
    /// <param name="use3DNoise">Optional parameter indicating whether to use 3D noise for generating terrain.</param>
    public MarchingCubes(int seed, int width, int height, float resolution, float scale, float heightThreshold, bool use3DNoise = false) {
        this._perlinNoise = new PerlinNoise(seed);
        this._width = width;
        this._height = height;
        this._resolution = resolution;
        this._scale = scale;
        this._heightThreshold = heightThreshold;
        this._use3DNoise = use3DNoise;
        this._vertices = new List<Vector3>();
        this._normals = new List<Vector3>();
        this._texCoords = new List<Vector2>();
        this._triangles = new List<ushort>();
    }

    /// <summary>
    /// Initializes the MarchingCubes object by setting heights and marching cubes
    /// </summary>
    public void Init() {
        this._perlinNoise.Init();
        this.SetHeights();
        this.MarchCubes();
        this.HasInitialized = true;
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
        
        MeshHelper.Upload(ref mesh, false);

        return ModelHelper.LoadFromMesh(mesh);
    }


    /// <summary>
    /// Set the heights of the terrain using the Marching Cubes algorithm.
    /// </summary>
    private void SetHeights() {
        this._heights = new float[this._width + 1, this._height + 1, this._width + 1];

        for (int x = 0; x < this._width + 1; x++) {
            for (int y = 0; y < this._height + 1; y++) {
                for (int z = 0; z < this._width + 1; z++) {
                    if (this._use3DNoise) {
                        this._heights[x, y, z] = this.PerlinNoise3D((float) x / this._width * this._scale, (float) y / this._height * this._scale, (float) z / this._width * this._scale);
                    }
                    else {
                        float currentHeight = this._height * this._perlinNoise.Noise(x * this._scale, z * this._scale);
                        float distToSurface;

                        if (y <= currentHeight - 0.5f) {
                            distToSurface = 0f;
                        }
                        else if (y > currentHeight + 0.5f) {
                            distToSurface = 1f;
                        }
                        else if (y > currentHeight) {
                            distToSurface = y - currentHeight;
                        }
                        else {
                            distToSurface = currentHeight - y;
                        }

                        this._heights[x, y, z] = distToSurface;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Generates Perlin noise value based on given x, y, and z coordinates.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <param name="z">The z-coordinate.</param>
    /// <returns>The Perlin noise value.</returns>
    private float PerlinNoise3D(float x, float y, float z) {
        float xy = this._perlinNoise.Noise(x, y);
        float xz = this._perlinNoise.Noise(x, z);
        float yz = this._perlinNoise.Noise(y, z);

        float yx = this._perlinNoise.Noise(y, x);
        float zx = this._perlinNoise.Noise(z, x);
        float zy = this._perlinNoise.Noise(z, y);

        return (xy + xz + yz + yx + zx + zy) / 6;
    }

    /// <summary>
    /// Retrieves the configuration index for the given cube corners.
    /// </summary>
    /// <param name="cubeCorners">The array containing the values of the cube corners.</param>
    /// <returns>The configuration index for the cube corners.</returns>
    private int GetConfigIndex(float[] cubeCorners) {
        int index = 0;

        for (int i = 0; i < 8; i++) {
            if (cubeCorners[i] > this._heightThreshold) {
                index |= 1 << i;
            }
        }

        return index;
    }

    /// <summary>
    /// Represents a Marching Cubes algorithm implementation for generating 3D terrain mesh.
    /// </summary>
    private void MarchCubes() {
        this._vertices.Clear();
        this._normals.Clear();
        this._triangles.Clear();
        
        for (int x = 0; x < this._width; x++) {
            for (int y = 0; y < this._height; y++) {
                for (int z = 0; z < this._width; z++) {
                    float[] corners = new float[8];
                    
                    for (int i = 0; i < 8; i++) {
                        Vector3 corner = new Vector3(x, y, z) + MarchingCubesTables.Corners[i];
                        corners[i] = this._heights[(int) corner.X, (int) corner.Y, (int) corner.Z];
                    }

                    this.MarchCube(new Vector3(x, y, z), corners);
                }
            }
        }
    }

    /// <summary>
    /// Marches a cube at the given position using the Marching Cubes algorithm, generating the mesh vertices, normals, and triangles.
    /// </summary>
    /// <param name="position">The position of the cube in 3D space.</param>
    /// <param name="cubeCorners">The array containing the values of the cube corners.</param>
    private void MarchCube(Vector3 position, float[] cubeCorners) {
        int index = this.GetConfigIndex(cubeCorners);

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
}