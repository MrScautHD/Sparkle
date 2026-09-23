using System.Numerics;
using Bliss.CSharp.Materials;

namespace Sparkle.CSharp.Terrain.Painting;

public interface ITerrainPainter : IDisposable {
    
    /// <summary>
    /// The material used for rendering the terrain.
    /// </summary>
    Material Material { get; }
    
    /// <summary>
    /// Applies a brush to the terrain texture/layer data.
    /// </summary>
    /// <param name="center">The terrain-space center of the brush.</param>
    /// <param name="radius">The radius of the brush in terrain units.</param>
    /// <param name="strength">The paint strength. Positive values paint, negative values may erase depending on implementation.</param>
    /// <param name="layer">The texture/material layer index.</param>
    /// <param name="brushType">The brush shape/falloff.</param>
    /// <returns><c>true</c> if any paint data was modified; otherwise, <c>false</c>.</returns>
    bool ApplyTextureLayerBrush(Vector3 center, float radius, float strength, int layer, TerrainBrushType brushType);
}