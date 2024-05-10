using Raylib_CSharp.Colors;

namespace Sparkle.CSharp.GUI.Elements.Data;

public interface IData {
    
    /// <summary>
    /// Gets the rotation value of an GUI element.
    /// </summary>
    float Rotation { get; set; }

    /// <summary>
    /// Represents the color property used in GUI elements.
    /// </summary>
    Color Color { get; set; }

    /// <summary>
    /// Represents the hover color property used in GUI elements.
    /// </summary>
    Color HoverColor { get; set; }
}