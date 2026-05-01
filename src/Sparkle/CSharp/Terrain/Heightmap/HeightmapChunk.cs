using System.Numerics;
using Bliss.CSharp;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Geometry.Meshes;
using Bliss.CSharp.Geometry.Meshes.Data;
using Bliss.CSharp.Graphics.VertexTypes;
using Bliss.CSharp.Materials;
using Veldrid;

namespace Sparkle.CSharp.Terrain.Heightmap;

public class HeightmapChunk : IChunk {
    
    public ITerrain Terrain { get; }
    
    public IMesh? Mesh { get; private set; }
    
    public Vector3 Position { get; }
    
    public int Width { get; }
    
    public int Height { get; }
    
    public int Depth { get; }
    
    public bool IsDirty { get; private set; }
    
    private int _lod;
    
    private bool _topologyDirty;
    
    public int Lod {
        get => this._lod;
        set {
            if (this._lod == value) {
                return;
            }
            
            this._lod = value;
            this._topologyDirty = true;
            this.MarkDirty();
        }
    }
    
    public int CurrentLod { get; private set; }
    
    private Vertex3D[] _pendingVertices;
    private uint[] _pendingIndices;
    private int _dirtyVersion;
    private int _generatedVersion;
    private bool _pendingDeferredVertexUpload;
    
    private static Material? s_sharedMaterial;
    
    public HeightmapChunk(ITerrain terrain, Vector3 position, int width, int height, int depth) {
        this.Terrain = terrain;
        this.Position = position;
        this.Width = width;
        this.Height = height;
        this.Depth = depth;
        this.IsDirty = false;
        this._lod = -1;
        this.CurrentLod = -1;
        this._pendingVertices = [];
        this._pendingIndices = [];
        this._dirtyVersion = 0;
        this._generatedVersion = 0;
        this._topologyDirty = true;
        this._pendingDeferredVertexUpload = false;
    }
    
    public void MarkDirty() {
        this.IsDirty = true;
        this._dirtyVersion++;

        if (this.Terrain is HeightmapTerrain terrain) {
            terrain.NotifyChunkDirty(this);
        }
    }
    
    public void GenerateGeometry() {
        int targetVersion = this._dirtyVersion;
        
        if (this.Lod < 0 || this.Width <= 0 || this.Depth <= 0) {
            this._pendingVertices = [];
            this._pendingIndices = [];
            this._generatedVersion = targetVersion;
            return;
        }
        
        int step = Math.Max(1, 1 << this.Lod);
        List<int> xSamples = BuildSampleAxis(this.Width, step);
        List<int> zSamples = BuildSampleAxis(this.Depth, step);
        
        int xCount = xSamples.Count;
        int zCount = zSamples.Count;
        
        int[] worldX = new int[xCount];
        int[] worldZ = new int[zCount];
        float[,] heights = new float[zCount, xCount];
        
        for (int xIndex = 0; xIndex < xCount; xIndex++) {
            worldX[xIndex] = (int) this.Position.X + xSamples[xIndex];
        }
        
        for (int zIndex = 0; zIndex < zCount; zIndex++) {
            worldZ[zIndex] = (int) this.Position.Z + zSamples[zIndex];
        }
        
        for (int zIndex = 0; zIndex < zCount; zIndex++) {
            int z = worldZ[zIndex];
            
            for (int xIndex = 0; xIndex < xCount; xIndex++) {
                heights[zIndex, xIndex] = this.SampleSurfaceHeight(worldX[xIndex], z);
            }
        }
        
        Vertex3D[] vertices = new Vertex3D[xCount * zCount];
        List<uint> indices = new List<uint>((xCount - 1) * (zCount - 1) * 6);
        
        for (int zIndex = 0; zIndex < zCount; zIndex++) {
            for (int xIndex = 0; xIndex < xCount; xIndex++) {
                int xLeft = Math.Max(0, xIndex - 1);
                int xRight = Math.Min(xCount - 1, xIndex + 1);
                int zDown = Math.Max(0, zIndex - 1);
                int zUp = Math.Min(zCount - 1, zIndex + 1);
                
                Vector3 tangentX = new Vector3(
                    worldX[xRight] - worldX[xLeft],
                    heights[zIndex, xRight] - heights[zIndex, xLeft],
                    0.0F
                );
                
                Vector3 tangentZ = new Vector3(
                    0.0F,
                    heights[zUp, xIndex] - heights[zDown, xIndex],
                    worldZ[zUp] - worldZ[zDown]
                );
                
                Vector3 normal = Vector3.Cross(tangentZ, tangentX);
                
                if (normal.LengthSquared() <= 1.0E-10F) {
                    normal = Vector3.UnitY;
                }
                else {
                    normal = Vector3.Normalize(normal);
                }
                
                Vector3 position = new Vector3(worldX[xIndex], heights[zIndex, xIndex], worldZ[zIndex]);
                
                vertices[zIndex * xCount + xIndex] = new Vertex3D {
                    Position = position,
                    TexCoords = new Vector2(worldX[xIndex], worldZ[zIndex]),
                    Normal = normal,
                    Color = Color.White.ToRgbaFloatVec4()
                };
            }
        }
        
        for (int zIndex = 0; zIndex < zCount - 1; zIndex++) {
            for (int xIndex = 0; xIndex < xCount - 1; xIndex++) {
                uint i0 = (uint) (zIndex * xCount + xIndex);
                uint i1 = i0 + 1;
                uint i2 = (uint) ((zIndex + 1) * xCount + xIndex);
                uint i3 = i2 + 1;
                
                indices.Add(i0);
                indices.Add(i1);
                indices.Add(i2);
                
                indices.Add(i1);
                indices.Add(i3);
                indices.Add(i2);
            }
        }
        
        this._pendingVertices = vertices;
        this._pendingIndices = indices.ToArray();
        this._generatedVersion = targetVersion;
    }
    
    public void UploadGeometry(GraphicsDevice graphicsDevice) {
        if (this._pendingVertices.Length == 0 || this._pendingIndices.Length == 0) {
            this.Mesh?.Dispose();
            this.Mesh = null;
            this._topologyDirty = true;
            this._pendingDeferredVertexUpload = false;
        }
        else if (this.Mesh is Mesh<Vertex3D> existingMesh &&
                 existingMesh.MeshData is BasicMeshData existingData &&
                 !this._topologyDirty &&
                 existingData.Vertices.Length == this._pendingVertices.Length &&
                 existingData.Indices.Length == this._pendingIndices.Length) {
            Array.Copy(this._pendingVertices, existingData.Vertices, this._pendingVertices.Length);
            this._pendingDeferredVertexUpload = true;
        }
        else {
            this.Mesh?.Dispose();
            this.Mesh = new Mesh<Vertex3D>(graphicsDevice, GetSharedMaterial(), new BasicMeshData(this._pendingVertices, this._pendingIndices));
            this._topologyDirty = false;
            this._pendingDeferredVertexUpload = false;
        }
        
        this.CurrentLod = this.Lod;
        this.IsDirty = this._dirtyVersion != this._generatedVersion;

        if (this.Terrain is HeightmapTerrain terrain) {
            if (this.IsDirty) {
                terrain.NotifyChunkDirty(this);
            }
            else {
                terrain.NotifyChunkClean(this);
            }
        }
    }
    
    public void FlushDeferredUpload(CommandList commandList) {
        if (!this._pendingDeferredVertexUpload) {
            return;
        }
        
        if (this.Mesh is Mesh<Vertex3D> existingMesh) {
            existingMesh.UpdateVertexBuffer(commandList);
        }
        
        this._pendingDeferredVertexUpload = false;
    }
    
    private float SampleSurfaceHeight(int worldX, int worldZ) {
        if (this.Terrain is HeightmapTerrain heightmapTerrain) {
            return heightmapTerrain.GetSurfaceHeight(worldX, worldZ);
        }
        
        int maxY = this.Terrain.Height;
        float iso = this.Terrain.IsoLevel;
        float previousDensity = this.Terrain.GetRawDensityAt(worldX, maxY, worldZ);
        
        for (int y = maxY - 1; y >= 0; y--) {
            float density = this.Terrain.GetRawDensityAt(worldX, y, worldZ);
            
            if (density >= iso) {
                float upperY = y + 1;
                float lowerY = y;
                
                if (MathF.Abs(previousDensity - density) < 1.0E-6F) {
                    return lowerY;
                }
                
                float t = (iso - density) / (previousDensity - density);
                return lowerY + t;
            }
            
            previousDensity = density;
        }
        
        return 0.0F;
    }
    
    private static List<int> BuildSampleAxis(int size, int step) {
        List<int> samples = new List<int>();
        
        for (int value = 0; value <= size; value += step) {
            samples.Add(value);
        }
        
        if (samples[^1] != size) {
            samples.Add(size);
        }
        
        return samples;
    }
    
    private static Material GetSharedMaterial() {
        if (s_sharedMaterial != null) {
            return s_sharedMaterial;
        }
        
        RasterizerStateDescription rasterizerState = new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Wireframe, FrontFace.Clockwise, true, false);
        Material material = new Material(GlobalResource.DefaultModelEffect, rasterizerState);
        
        material.AddMaterialMap(MaterialMapType.Albedo, 0, new MaterialMap {
            Texture = GlobalResource.DefaultModelTexture,
            Color = Color.White
        });
        
        s_sharedMaterial = material;
        return material;
    }
    
    public void Dispose() {
        this.Mesh?.Dispose();
        this.Mesh = null;
        this._pendingVertices = [];
        this._pendingIndices = [];
    }
}
