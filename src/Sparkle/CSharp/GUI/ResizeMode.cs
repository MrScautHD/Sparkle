namespace Sparkle.CSharp.GUI;

public enum ResizeMode {
    
    /// <summary>
    /// Renders the texture at its original size without any scaling.
    /// </summary>
    None,
    
    /// <summary>
    /// Uses nine-slice scaling where corners stay fixed and the center is stretched.
    /// </summary>
    NineSlice,
    
    /// <summary>
    /// Uses nine-slice scaling where corners stay fixed and the center is tiled.
    /// </summary>
    TileCenter
}