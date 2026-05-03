using System.Numerics;
using Bliss.CSharp.Geometry.Meshes;
using Veldrid;

namespace Sparkle.CSharp.Terrain;

public interface IChunk : IDisposable {
    
    /// <summary>
    /// The terrain this chunk belongs to.
    /// </summary>
    ITerrain Terrain { get; }
    
    /// <summary>
    /// The uploaded GPU mesh, or <c>null</c> when the chunk has no geometry.
    /// </summary>
    IMesh? Mesh { get; }
    
    /// <summary>
    /// The world-space origin of this chunk.
    /// </summary>
    Vector3 Position { get; }
    
    /// <summary>
    /// The width of this chunk in voxels.
    /// </summary>
    int Width { get; }
    
    /// <summary>
    /// The height of this chunk in voxels.
    /// </summary>
    int Height { get; }
    
    /// <summary>
    /// The depth of this chunk in voxels.
    /// </summary>
    int Depth { get; }
    
    /// <summary>
    /// Gets whether this chunk needs its mesh rebuilt.
    /// </summary>
    bool IsDirty { get; }
    
    /// <summary>
    /// Gets or sets the target LOD level. -1 means the chunk is culled.
    /// </summary>
    int Lod { get; set; }
    
    /// <summary>
    /// Gets the LOD level that was used for the currently uploaded mesh.
    /// </summary>
    int CurrentLod { get; }
    
    /// <summary>
    /// Gets whether this chunk currently has generated geometry waiting for upload/update.
    /// </summary>
    bool HasPendingGeometry { get; }
    
    /// <summary>
    /// Gets whether pending geometry can be applied in place via <see cref="UpdateGeometry"/>.
    /// </summary>
    bool CanUpdateGeometryInPlace { get; }
    
    /// <summary>
    /// Marks this chunk as needing a mesh rebuild.
    /// </summary>
    void MarkDirty();
    
    /// <summary>
    /// Generates mesh geometry on the CPU. Safe to call from a background thread.
    /// </summary>
    void GenerateGeometry();
    
    /// <summary>
    /// Uploads the geometry prepared by <see cref="GenerateGeometry"/> to the GPU.
    /// Must be called on the render thread.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used to create GPU buffers.</param>
    void UploadGeometry(GraphicsDevice graphicsDevice);

    /// <summary>
    /// Updates the geometry of the chunk by issuing the required rendering commands.
    /// </summary>
    /// <param name="commandList">The command list used to submit rendering and resource update commands to the GPU.</param>
    void UpdateGeometry(CommandList commandList);
}