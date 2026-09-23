namespace Sparkle.CSharp.Terrain;

public enum TerrainBrushType {
    
    /// <summary>
    /// A hard-edged circular brush with a uniform effect across its entire radius.
    /// </summary>
    Circle,
    
    /// <summary>
    /// A circular brush with a smooth falloff from full strength at the center to zero at the edges.
    /// </summary>
    SoftCircle,
    
    /// <summary>
    /// A brush type used to smooth out terrain elevations and transitions, creating a flatter
    /// </summary>
    Smoothing,
    
    /// <summary>
    /// A rotated quad brush, typically elongated and oriented along a path, useful for carving roads or rivers.
    /// </summary>
    Route,
    
    /// <summary>
    /// A rectangular (four-sided) brush with a uniform effect across its area.
    /// </summary>
    Quad,
    
    /// <summary>
    /// A five-sided polygonal brush with a uniform effect across its area.
    /// </summary>
    Pentagon,
    
    /// <summary>
    /// A brush that applies randomized, irregular variation to create a natural, uneven terrain effect.
    /// </summary>
    Noisy
}