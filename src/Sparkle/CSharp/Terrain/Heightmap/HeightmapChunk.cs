using System.Numerics;
using Bliss.CSharp;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Geometry.Meshes;
using Bliss.CSharp.Geometry.Meshes.Data;
using Bliss.CSharp.Graphics.VertexTypes;
using Veldrid;

namespace Sparkle.CSharp.Terrain.Heightmap;

public class HeightmapChunk : Disposable, IChunk {
    
    public ITerrain Terrain { get; private set; }
    
    public IMesh? Mesh { get; private set; }
    
    public Vector3 Position { get; private set; }
    
    public int Width { get; private set; }
    
    public int Height { get; private set; }
    
    public int Depth { get; private set; }
    
    public bool IsDirty { get; private set; }
    
    public int Lod {
        get => this._lod;
        set {
            if (this._lod == value) {
                return;
            }
            
            this._lod = value;
            this.MarkDirty();
        }
    }
    
    public int CurrentLod { get; private set; }
    
    public int PendingVertexCount => this._pendingVertices.Length;
    
    public int PendingIndexCount => this._pendingIndices.Length;
    
    private int _lod;
    
    private Vertex3D[] _pendingVertices;
    
    private uint[] _pendingIndices;
    
    private float[] _heightSamples;
    
    private int _dirtyVersion;
    
    private int _generatedVersion;
    
    public HeightmapChunk(HeightmapTerrain terrain, Vector3 position, int width, int height, int depth) {
        this.Terrain = terrain;
        this.Position = position;
        this.Width = width;
        this.Height = height;
        this.Depth = depth;
        this.IsDirty = false;
        this.CurrentLod = -1;
        this._lod = -1;
        this._pendingVertices = [];
        this._pendingIndices = [];
        this._heightSamples = [];
        this._dirtyVersion = 0;
        this._generatedVersion = 0;
    }
    
    public void MarkDirty() {
        this.IsDirty = true;
        this._dirtyVersion++;
    }
    
    public void GenerateGeometry() {
        int targetVersion = this._dirtyVersion;
        
        // Early out for invalid chunk dimensions/LOD.
        if (this.Lod < 0 || this.Width <= 0 || this.Depth <= 0) {
            this._pendingVertices = [];
            this._pendingIndices = [];
            this._generatedVersion = targetVersion;
            return;
        }
        
        if (this.Terrain is not HeightmapTerrain heightmapTerrain) {
            throw new InvalidOperationException($"{nameof(HeightmapChunk)} requires a {nameof(HeightmapTerrain)}.");
        }
        
        int step = Math.Max(1, 1 << this.Lod);
        
        int width = this.Width;
        int depth = this.Depth;
        
        int startX = (int) this.Position.X;
        int startZ = (int) this.Position.Z;
        
        // Build sample grid for the current LOD, always including chunk edges.
        int xBaseCount = (width / step) + 1;
        int zBaseCount = (depth / step) + 1;
        
        bool appendWidth = width % step != 0;
        bool appendDepth = depth % step != 0;
        
        int xCount = xBaseCount + (appendWidth ? 1 : 0);
        int zCount = zBaseCount + (appendDepth ? 1 : 0);
        
        int xLastIndex = xCount - 1;
        int zLastIndex = zCount - 1;
        
        // Sample terrain heights for each grid point.
        int heightSampleCount = zCount * xCount;
        float[] heights = this._heightSamples.Length == heightSampleCount ? this._heightSamples : new float[heightSampleCount];
        
        for (int zIndex = 0; zIndex < zCount; zIndex++) {
            int worldZValue = appendDepth && zIndex == zLastIndex ? startZ + depth : startZ + (zIndex * step);
            int rowOffset = zIndex * xCount;
            
            for (int xIndex = 0; xIndex < xCount; xIndex++) {
                int worldXValue = appendWidth && xIndex == xLastIndex ? startX + width : startX + (xIndex * step);
                heights[rowOffset + xIndex] = heightmapTerrain.GetSurfaceHeight(worldXValue, worldZValue);
            }
        }
        
        // Create/update vertex data.
        int vertexCount = xCount * zCount;
        Vertex3D[] vertices = this._pendingVertices.Length == vertexCount ? this._pendingVertices : new Vertex3D[vertexCount];
        Vector4 whiteColor = Color.White.ToRgbaFloatVec4();
        
        for (int zIndex = 0; zIndex < zCount; zIndex++) {
            int zDown = zIndex > 0 ? zIndex - 1 : 0;
            int zUp = zIndex < zCount - 1 ? zIndex + 1 : zCount - 1;
            
            int rowOffset = zIndex * xCount;
            int rowDownOffset = zDown * xCount;
            
            int rowUpOffset = zUp * xCount;
            
            int worldZValue = appendDepth && zIndex == zLastIndex ? startZ + depth : startZ + (zIndex * step);
            int worldZDownValue = appendDepth && zDown == zLastIndex ? startZ + depth : startZ + (zDown * step);
            int worldZUpValue = appendDepth && zUp == zLastIndex ? startZ + depth : startZ + (zUp * step);
            
            for (int xIndex = 0; xIndex < xCount; xIndex++) {
                int xLeft = xIndex > 0 ? xIndex - 1 : 0;
                int xRight = xIndex < xCount - 1 ? xIndex + 1 : xCount - 1;
                
                int centerIndex = rowOffset + xIndex;
                
                int worldXValue = appendWidth && xIndex == xLastIndex ? startX + width : startX + (xIndex * step);
                int worldXLeftValue = appendWidth && xLeft == xLastIndex ? startX + width : startX + (xLeft * step);
                int worldXRightValue = appendWidth && xRight == xLastIndex ? startX + width : startX + (xRight * step);
                
                Vector3 tangentX = new Vector3(
                    worldXRightValue - worldXLeftValue,
                    heights[rowOffset + xRight] - heights[rowOffset + xLeft],
                    0.0F
                );
                
                Vector3 tangentZ = new Vector3(
                    0.0F,
                    heights[rowUpOffset + xIndex] - heights[rowDownOffset + xIndex],
                    worldZUpValue - worldZDownValue
                );
                
                Vector3 normal = Vector3.Cross(tangentZ, tangentX);
                
                normal = normal.LengthSquared() <= 1.0E-10F ? Vector3.UnitY : Vector3.Normalize(normal);
                
                Vector3 position = new Vector3(worldXValue, heights[centerIndex], worldZValue);
                
                vertices[centerIndex] = new Vertex3D {
                    Position = position,
                    TexCoords = new Vector2(worldXValue, worldZValue),
                    Normal = normal,
                    Color = whiteColor
                };
            }
        }
        
        // Create/update triangle indices for the sampled grid.
        int indexCount = (xCount - 1) * (zCount - 1) * 6;
        bool rebuildIndices = this._pendingIndices.Length != indexCount;
        uint[] indices = rebuildIndices ? new uint[indexCount] : this._pendingIndices;
        
        if (rebuildIndices) {
            int indexWrite = 0;
            
            for (int zIndex = 0; zIndex < zCount - 1; zIndex++) {
                int rowOffset = zIndex * xCount;
                int nextRowOffset = (zIndex + 1) * xCount;
                
                for (int xIndex = 0; xIndex < xCount - 1; xIndex++) {
                    uint i0 = (uint) (rowOffset + xIndex);
                    uint i1 = i0 + 1;
                    uint i2 = (uint) (nextRowOffset + xIndex);
                    uint i3 = i2 + 1;
                    
                    indices[indexWrite++] = i0;
                    indices[indexWrite++] = i1;
                    indices[indexWrite++] = i2;
                    indices[indexWrite++] = i1;
                    indices[indexWrite++] = i3;
                    indices[indexWrite++] = i2;
                }
            }
        }
        
        // Publish generated geometry for upload/update stage.
        this._pendingVertices = vertices;
        this._pendingIndices = indices;
        this._heightSamples = heights;
        this._generatedVersion = targetVersion;
    }
    
    public void UploadGeometry(GraphicsDevice graphicsDevice) {
        bool hasGeometry = this._pendingVertices.Length > 0 && this._pendingIndices.Length > 0;
        
        // Recreate mesh if it has geometry.
        this.Mesh?.Dispose();
        this.Mesh = !hasGeometry ? null : new Mesh<Vertex3D>(graphicsDevice, this.Terrain.Material, new BasicMeshData(this._pendingVertices, this._pendingIndices));
        
        // Set current lod and mark this chunk as dirty.
        this.CurrentLod = this.Lod;
        this.IsDirty = this._dirtyVersion != this._generatedVersion;
    }
    
    public void UpdateGeometry(CommandList commandList) {
        if (this.Mesh is not Mesh<Vertex3D> mesh) {
            throw new InvalidOperationException($"{nameof(UpdateGeometry)} requires a valid {nameof(Mesh<>)} instance.");
        }
        
        if (mesh.MeshData is not BasicMeshData meshData) {
            throw new InvalidOperationException($"{nameof(UpdateGeometry)} requires {nameof(BasicMeshData)} mesh data.");
        }
        
        if (meshData.Vertices.Length != this._pendingVertices.Length) {
            throw new InvalidOperationException($"Vertex buffer length mismatch. Mesh has {meshData.Vertices.Length}, pending has {this._pendingVertices.Length}.");
        }
        
        // Set vertices into mesh.
        Array.Copy(this._pendingVertices, meshData.Vertices, this._pendingVertices.Length);
        
        // Update vertex buffer.
        //mesh.UpdateVertexBuffer(commandList); // TODO: Add this back when fixed: https://github.com/jhm-ciberman/neo-veldrid/issues/33
        mesh.UpdateVertexBufferImmediate();
        
        // Keep state transitions consistent with UploadGeometry.
        this.CurrentLod = this.Lod;
        this.IsDirty = this._dirtyVersion != this._generatedVersion;
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            this.Mesh?.Dispose();
            this.Mesh = null;
            this._pendingVertices = [];
            this._pendingIndices = [];
            this._heightSamples = [];
        }
    }
}
