namespace Sparkle.CSharp.Terrain;

public enum TerrainBrushType : int {
    
    /// <summary>
    /// A hard-edged circular brush with a uniform effect across its entire radius.
    /// </summary>
    Circle = 0,
    
    /// <summary>
    /// A circular brush with a smooth falloff from full strength at the center to zero at the edges.
    /// </summary>
    SoftCircle = 1,
    
    /// <summary>
    /// A brush type used to smooth out terrain elevations and transitions, creating a flatter
    /// </summary>
    Smoothing = 2,
    
    /// <summary>
    /// A rotated quad brush, typically elongated and oriented along a path, useful for carving roads or rivers.
    /// </summary>
    Route = 3,
    
    /// <summary>
    /// A rectangular (four-sided) brush with a uniform effect across its area.
    /// </summary>
    Quad = 4,
    
    /// <summary>
    /// A five-sided polygonal brush with a uniform effect across its area.
    /// </summary>
    Pentagon = 5,
    
    /// <summary>
    /// A brush that applies randomized, irregular variation to create a natural, uneven terrain effect.
    /// </summary>
    Noisy = 6
}