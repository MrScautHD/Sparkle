namespace Sparkle.CSharp.GUI.Batching;

public enum GuiBatchType {
    
    /// <summary>
    /// No batch type specified; nothing is being rendered.
    /// </summary>
    None,
    
    /// <summary>
    /// A batch of primitive shapes (e.g. lines, rectangles, quads).
    /// </summary>
    Primitive,
    
    /// <summary>
    /// A batch of textured sprites.
    /// </summary>
    Sprite
}