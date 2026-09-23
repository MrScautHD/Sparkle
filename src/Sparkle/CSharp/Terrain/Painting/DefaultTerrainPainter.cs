using System.Numerics;
using Bliss.CSharp;
using Bliss.CSharp.Materials;

namespace Sparkle.CSharp.Terrain.Painting;

public class DefaultTerrainPainter : Disposable, ITerrainPainter {
    
    /// <summary>
    /// The material used for rendering the terrain.
    /// </summary>
    public Material Material { get; private set; }
    
    /// <summary>
    /// Creates a new terrain painter that uses the given material for rendering.
    /// </summary>
    /// <param name="material">The material used for rendering the terrain.</param>
    public DefaultTerrainPainter(Material material) {
        this.Material = material;
    }
    
    /// <summary>
    /// Applies a brush to the terrain texture/layer data.
    /// </summary>
    /// <param name="center">The terrain-space center of the brush.</param>
    /// <param name="radius">The radius of the brush in terrain units.</param>
    /// <param name="strength">The paint strength. Positive values paint, negative values may erase depending on implementation.</param>
    /// <param name="layer">The texture/material layer index.</param>
    /// <param name="brushType">The brush shape/falloff.</param>
    /// <returns><c>true</c> if any paint data was modified; otherwise, <c>false</c>.</returns>
    public bool ApplyTextureLayerBrush(Vector3 center, float radius, float strength, int layer, TerrainBrushType brushType) {
        return false;
    }
    
    protected override void Dispose(bool disposing) { }
}