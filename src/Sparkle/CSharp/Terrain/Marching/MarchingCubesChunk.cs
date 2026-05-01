using System.Numerics;
using Bliss.CSharp;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Geometry.Meshes;
using Bliss.CSharp.Geometry.Meshes.Data;
using Bliss.CSharp.Graphics.VertexTypes;
using Bliss.CSharp.Materials;
using Veldrid;
using Material = Bliss.CSharp.Materials.Material;

namespace Sparkle.CSharp.Terrain.Marching;

public class MarchingCubesChunk : Disposable, IChunk {
    
    /// <summary>
    /// The terrain this chunk belongs to.
    /// </summary>
    public ITerrain Terrain { get; private set; }
    
    /// <summary>
    /// The uploaded GPU mesh, or <c>null</c> when the chunk has no geometry.
    /// </summary>
    public IMesh? Mesh { get; private set; }
    
    /// <summary>
    /// The world-space origin of this chunk.
    /// </summary>
    public Vector3 Position { get; private set; }
    
    /// <summary>
    /// The width of this chunk in voxels.
    /// </summary>
    public int Width { get; private set; }
    
    /// <summary>
    /// The height of this chunk in voxels.
    /// </summary>
    public int Height { get; private set; }
    
    /// <summary>
    /// The depth of this chunk in voxels.
    /// </summary>
    public int Depth { get; private set; }
    
    /// <summary>
    /// Gets whether this chunk needs its mesh rebuilt.
    /// </summary>
    public bool IsDirty { get; private set; }
    
    /// <summary>
    /// Gets or sets the target LOD level. -1 means the chunk is culled.
    /// </summary>
    public int Lod { get; set; }
    
    /// <summary>
    /// Gets the LOD level that was used for the currently uploaded mesh.
    /// </summary>
    public int CurrentLod { get; private set; }
    
    /// <summary>
    /// CPU-side vertex buffer used during mesh generation.
    /// </summary>
    private List<Vertex3D> _vertices;
    
    /// <summary>
    /// CPU-side index buffer used during mesh generation.
    /// </summary>
    private List<uint> _indices;
    
    /// <summary>
    /// Accumulated vertex normals reused across rebuilds to avoid per-frame allocations.
    /// </summary>
    private Vector3[] _normalAccumulator;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MarchingCubesChunk"/> class.
    /// </summary>
    /// <param name="terrain">The terrain this chunk belongs to.</param>
    /// <param name="position">The world-space origin of the chunk.</param>
    /// <param name="width">The width of the chunk in voxels.</param>
    /// <param name="height">The height of the chunk in voxels.</param>
    /// <param name="depth">The depth of the chunk in voxels.</param>
    public MarchingCubesChunk(ITerrain terrain, Vector3 position, int width, int height, int depth) {
        this.Terrain = terrain;
        this.Position = position;
        this.Width = width;
        this.Height = height;
        this.Depth = depth;
        this.IsDirty = true;
        this.Lod = 1;
        this.CurrentLod = -2;
        this._vertices = [];
        this._indices = [];
        this._normalAccumulator = [];
    }
    
    /// <summary>
    /// Marks this chunk as needing a mesh rebuild.
    /// </summary>
    public void MarkDirty() {
        this.IsDirty = true;
    }
    
    /// <summary>
    /// Generates mesh geometry on the CPU. Safe to call from a background thread.
    /// </summary>
    public void GenerateGeometry() {
        if (!this.IsDirty) {
            return;
        }
        
        this.CurrentLod = this.Lod;
        this._vertices.Clear();
        this._indices.Clear();
        
        if (this.Lod == -1) {
            this.IsDirty = false;
            return;
        }
        
        int step = Math.Max(1, this.Lod);
        
        if (this.IsUniformChunk(step)) {
            this.IsDirty = false;
            return;
        }
        
        int cellCount = (this.Width / step) * (this.Height / step) * (this.Depth / step);
        int estimatedVerts = Math.Max(64, cellCount / 4);
        
        Dictionary<Vector3, uint> vertexIndexMap = new Dictionary<Vector3, uint>(estimatedVerts);
        
        if (this._vertices.Capacity < estimatedVerts) {
            this._vertices.Capacity = estimatedVerts;
            this._indices.Capacity = estimatedVerts;
        }
        
        float[] cornerDensities = new float[8];
        
        for (int x = 0; x < this.Width; x += step) {
            int worldX = (int) this.Position.X + x;
            int stepX = Math.Min(step, this.Width - x);
            
            for (int y = 0; y < this.Height; y += step) {
                int worldY = (int) this.Position.Y + y;
                int stepY = Math.Min(step, this.Height - y);
                
                for (int z = 0; z < this.Depth; z += step) {
                    int worldZ = (int) this.Position.Z + z;
                    int stepZ = Math.Min(step, this.Depth - z);
                    
                    Vector3 cell = new Vector3(x, y, z);
                    int configIndex = 0;
                    
                    for (int c = 0; c < 8; c++) {
                        Vector3 corner = MarchingCubesTables.Corners[c];
                        float density = this.Terrain.GetRawDensityAt(
                            worldX + (int) corner.X * stepX,
                            worldY + (int) corner.Y * stepY,
                            worldZ + (int) corner.Z * stepZ);
                        
                        cornerDensities[c] = density;
                        
                        if (density >= this.Terrain.IsoLevel) {
                            configIndex |= 1 << c;
                        }
                    }
                    
                    if (configIndex is 0 or 255) {
                        continue;
                    }
                    
                    for (int t = 0; t < 16; t += 3) {
                        int edgeA = MarchingCubesTables.Triangles[configIndex, t];
                        
                        if (edgeA == -1) {
                            break;
                        }
                        
                        Vector3 vertex1 = this.GetEdgeVertex(cell, edgeA, cornerDensities, stepX, stepY, stepZ);
                        Vector3 vertex2 = this.GetEdgeVertex(cell, MarchingCubesTables.Triangles[configIndex, t + 1], cornerDensities, stepX, stepY, stepZ);
                        Vector3 vertex3 = this.GetEdgeVertex(cell, MarchingCubesTables.Triangles[configIndex, t + 2], cornerDensities, stepX, stepY, stepZ);
                        
                        this.AppendVertex(vertex1, vertexIndexMap);
                        this.AppendVertex(vertex2, vertexIndexMap);
                        this.AppendVertex(vertex3, vertexIndexMap);
                    }
                }
            }
        }
        
        this.ComputeSmoothNormals();
        this.IsDirty = false;
    }
    
    /// <summary>
    /// Uploads the geometry prepared by <see cref="GenerateGeometry"/> to the GPU.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used to create GPU buffers.</param>
    public void UploadGeometry(GraphicsDevice graphicsDevice) {
        this.Mesh?.Dispose();
        this.Mesh = null;
        
        if (this._vertices.Count > 0 && this._indices.Count > 0) {
            Material material = new Material(GlobalResource.DefaultModelEffect);
            
            material.AddMaterialMap(MaterialMapType.Albedo, 0, new MaterialMap {
                Texture = GlobalResource.DefaultModelTexture,
                Color = Color.White
            });
            
            this.Mesh = new Mesh<Vertex3D>(graphicsDevice, material, new BasicMeshData(this._vertices.ToArray(), this._indices.ToArray()));
        }
        
        this._vertices.Clear();
        this._indices.Clear();
        this._vertices.TrimExcess();
        this._indices.TrimExcess();
    }
    
    /// <summary>
    /// Returns <c>true</c> when the chunk is entirely solid or entirely empty, meaning no surface exists inside it.
    /// </summary>
    /// <param name="step">The LOD step size used to sample density.</param>
    private bool IsUniformChunk(int step) {
        int stride = Math.Max(step, Math.Max(this.Width, Math.Max(this.Height, this.Depth)) / 4);
        bool allSolid = true;
        bool allEmpty = true;
        
        for (int x = 0; x <= this.Width && (allSolid || allEmpty); x += stride) {
            for (int y = 0; y <= this.Height && (allSolid || allEmpty); y += stride) {
                for (int z = 0; z <= this.Depth && (allSolid || allEmpty); z += stride) {
                    float d = this.Terrain.GetRawDensityAt(
                        (int) this.Position.X + x,
                        (int) this.Position.Y + y,
                        (int) this.Position.Z + z);
                    
                    if (d >= this.Terrain.IsoLevel) {
                        allEmpty = false;
                    }
                    else {
                        allSolid = false;
                    }
                }
            }
        }
        
        return allSolid || allEmpty;
    }
    
    /// <summary>
    /// Adds a vertex to the mesh if it does not already exist and appends its index to the index buffer.
    /// Ensures vertex deduplication using a lookup dictionary.
    /// </summary>
    /// <param name="localPosition">The vertex position in local chunk space.</param>
    /// <param name="vertexIndexMap">A dictionary mapping vertex positions to their assigned indices.</param>
    private void AppendVertex(Vector3 localPosition, Dictionary<Vector3, uint> vertexIndexMap) {
        if (!vertexIndexMap.TryGetValue(localPosition, out uint index)) {
            index = (uint) this._vertices.Count;
            vertexIndexMap[localPosition] = index;
            
            Vector3 worldPosition = this.Position + localPosition;
            
            this._vertices.Add(new Vertex3D {
                Position = localPosition,
                Normal = Vector3.UnitY,
                TexCoords = new Vector2(
                    worldPosition.X / Math.Max(1.0F, this.Terrain.Width),
                    worldPosition.Z / Math.Max(1.0F, this.Terrain.Depth)),
                Color = Color.White.ToRgbaFloatVec4()
            });
        }
        
        this._indices.Add(index);
    }
    
    /// <summary>
    /// Computes smooth per-vertex normals by averaging adjacent triangle face normals.
    /// </summary>
    private void ComputeSmoothNormals() {
        int vertexCount = this._vertices.Count;
        
        if (this._normalAccumulator.Length < vertexCount) {
            this._normalAccumulator = new Vector3[vertexCount];
        }
        else {
            Array.Clear(this._normalAccumulator, 0, vertexCount);
        }
        
        for (int i = 0; i < this._indices.Count; i += 3) {
            int index1 = (int) this._indices[i];
            int index2 = (int) this._indices[i + 1];
            int index3 = (int) this._indices[i + 2];
            
            Vector3 face = Vector3.Cross(
                this._vertices[index2].Position - this._vertices[index1].Position,
                this._vertices[index3].Position - this._vertices[index1].Position);
            
            this._normalAccumulator[index1] += face;
            this._normalAccumulator[index2] += face;
            this._normalAccumulator[index3] += face;
        }
        
        for (int i = 0; i < vertexCount; i++) {
            Vertex3D vertex = this._vertices[i];
            vertex.Normal = this._normalAccumulator[i].LengthSquared() > float.Epsilon ? -Vector3.Normalize(this._normalAccumulator[i]) : Vector3.UnitY;
            this._vertices[i] = vertex;
        }
    }
    
    /// <summary>
    /// Computes the interpolated position along a cube edge where the isosurface intersects.
    /// </summary>
    /// <param name="cellOrigin">The origin of the current voxel cell in local chunk space.</param>
    /// <param name="edgeIndex">The index of the edge being evaluated in the marching cubes lookup table.</param>
    /// <param name="cornerDensities">Precomputed density values for the 8 cube corners.</param>
    /// <param name="stepX">Voxel step size along the X axis.</param>
    /// <param name="stepY">Voxel step size along the Y axis.</param>
    /// <param name="stepZ">Voxel step size along the Z axis.</param>
    /// <returns>The interpolated vertex position along the edge.</returns>
    private Vector3 GetEdgeVertex(Vector3 cellOrigin, int edgeIndex, float[] cornerDensities, int stepX, int stepY, int stepZ) {
        int cornerIndex1 = MarchingCubesTables.EdgeCornerIndices[edgeIndex, 0];
        int cornerIndex2 = MarchingCubesTables.EdgeCornerIndices[edgeIndex, 1];
        
        float density1 = cornerDensities[cornerIndex1];
        float density2 = cornerDensities[cornerIndex2];
        float delta = density2 - density1;
        float densityDelta = MathF.Abs(delta) <= float.Epsilon ? 0.5F : Math.Clamp((this.Terrain.IsoLevel - density1) / delta, 0.0F, 1.0F);
        
        Vector3 corner1 = MarchingCubesTables.Corners[cornerIndex1];
        Vector3 corner2 = MarchingCubesTables.Corners[cornerIndex2];
        
        Vector3 start = cellOrigin + new Vector3(corner1.X * stepX, corner1.Y * stepY, corner1.Z * stepZ);
        Vector3 end = cellOrigin + new Vector3(corner2.X * stepX, corner2.Y * stepY, corner2.Z * stepZ);
        
        return Vector3.Lerp(start, end, densityDelta);
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            this.Mesh?.Dispose();
        }
    }
}