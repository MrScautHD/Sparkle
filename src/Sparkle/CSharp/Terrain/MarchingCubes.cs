using System.Numerics;
using Bliss.CSharp;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Geometry;
using Bliss.CSharp.Graphics.VertexTypes;
using Bliss.CSharp.Materials;
using LibNoise;
using LibNoise.Primitive;
using Veldrid;

namespace Sparkle.CSharp.Terrain;

public class MarchingCubes {

    private SimplexPerlin _perlin;
    
    private int _width;
    private int _height;
    
    private float _scale;
    private float _heightThreshold;

    private bool _use3DNoise;

    private List<Vertex3D> _vertices;
    private List<uint> _inidces;
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
        this._vertices = new List<Vertex3D>();
        this._inidces = new List<uint>();
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
        this._inidces.Clear();

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

                Vector3 pos = (edgeStart + edgeEnd) / 2;
                Vector3 normal = Vector3.Normalize(edgeEnd - edgeStart);

                // Add vertex.
                this._vertices.Add(new Vertex3D() {
                    Position = pos,
                    Normal = normal,
                    TexCoords = new Vector2(edgeIndex / (float) this._width, edgeIndex / (float) this._height)
                });
                
                // Add index.
                this._inidces.Add((ushort) (this._vertices.Count - 1));
                
                edgeIndex++;
            }
        }
    }

    /// <summary>
    /// Generates a mesh based on the vertices, normals, texture coordinates, and indices
    /// stored within the MarchingCubes instance.
    /// </summary>
    public Mesh GenMesh(GraphicsDevice graphicsDevice) {
        Material material = new Material(GlobalResource.DefaultModelEffect);
        
        material.AddMaterialMap(MaterialMapType.Albedo, new MaterialMap {
            Texture = GlobalResource.DefaultModelTexture,
            Color = Color.White
        });
        
        return new Mesh(graphicsDevice, material, this._vertices.ToArray(), this._inidces.ToArray());
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