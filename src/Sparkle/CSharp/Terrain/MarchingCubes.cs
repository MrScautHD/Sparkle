using System.Numerics;
using Bliss.CSharp;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Geometry;
using Bliss.CSharp.Graphics.VertexTypes;
using Bliss.CSharp.Materials;
using Veldrid;

namespace Sparkle.CSharp.Terrain;

public class MarchingCubes {

    private int _width;
    private int _height;

    private float _heightThreshold;

    private List<Vertex3D> _vertices;
    private List<uint> _indices;
    private float[,,] _heights;

    /// <summary>
    /// Initializes a new MarchingCubes instance with a flat plane.
    /// </summary>
    public MarchingCubes(int width, int height, float heightThreshold) {
        this._width = width;
        this._height = height;
        this._heightThreshold = heightThreshold;
        this._vertices = new List<Vertex3D>();
        this._indices = new List<uint>();

        // Initialize a flat plane
        this._heights = new float[width + 1, height + 1, width + 1];
        for (int x = 0; x <= width; x++) {
            for (int y = 0; y <= height; y++) {
                for (int z = 0; z <= width; z++) {
                    _heights[x, y, z] = 0f; // flat plane at Y=0
                }
            }
        }
    }

    /// <summary>
    /// Allows external tools to modify heights directly.
    /// </summary>
    public void SetHeight(int x, int y, int z, float value) {
        if (x < 0 || x > _width || y < 0 || y > _height || z < 0 || z > _width) return;
        _heights[x, y, z] = value;
    }

    /// <summary>
    /// Marches cubes to generate terrain mesh from manual heights.
    /// </summary>
    public void MarchCubes(Vector3 position, int width, int height) {
        _vertices.Clear();
        _indices.Clear();

        float[] corners = new float[8];

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                for (int z = 0; z < width; z++) {

                    for (int i = 0; i < 8; i++) {
                        Vector3 corner = new Vector3(x, y, z) + MarchingCubesTables.Corners[i];
                        corners[i] = _heights[(int)corner.X, (int)corner.Y, (int)corner.Z];
                    }

                    MarchCube(new Vector3(position.X + x, position.Y + y, position.Z + z), corners);
                }
            }
        }
    }

    private void MarchCube(Vector3 position, float[] corners) {
        int index = GetConfigIndex(corners);
        if (index == 0 || index == 255) return;

        int edgeIndex = 0;
        for (int t = 0; t < 5; t++) {
            for (int v = 0; v < 3; v++) {
                int triTableValue = MarchingCubesTables.Triangles[index, edgeIndex];
                if (triTableValue == -1) return;

                Vector3 edgeStart = position + MarchingCubesTables.Edges[triTableValue, 0];
                Vector3 edgeEnd = position + MarchingCubesTables.Edges[triTableValue, 1];

                Vector3 pos = (edgeStart + edgeEnd) / 2;
                Vector3 normal = Vector3.Normalize(edgeEnd - edgeStart);

                _vertices.Add(new Vertex3D() {
                    Position = pos,
                    Normal = normal,
                    TexCoords = new Vector2(edgeIndex / (float)_width, edgeIndex / (float)_height)
                });
                _indices.Add((uint)(_vertices.Count - 1));
                edgeIndex++;
            }
        }
    }

    private int GetConfigIndex(float[] corners) {
        int index = 0;
        for (int i = 0; i < 8; i++) {
            if (corners[i] >= this._heightThreshold) {
                index |= 1 << i;
            }
        }
        return index;
    }

    public Mesh GenMesh(GraphicsDevice graphicsDevice) {
        Material material = new Material(GlobalResource.DefaultModelEffect);
        material.AddMaterialMap(MaterialMapType.Albedo, new MaterialMap {
            Texture = GlobalResource.DefaultModelTexture,
            Color = Color.White
        });

        return new Mesh(graphicsDevice, material, _vertices.ToArray(), _indices.ToArray());
    }
}