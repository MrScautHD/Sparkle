using Raylib_cs;

namespace Sparkle.csharp.gui.element.data; 

public struct ButtonData {
    
    public Texture2D? Texture;
    public float Rotation;
    public Color Color;
    public Color HoverColor;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ButtonData"/>, setting default values for Rotation, Color, and HoverColor.
    /// </summary>
    public ButtonData() {
        this.Rotation = 0;
        this.Color = Color.WHITE;
        this.HoverColor = Color.GRAY;
    }
}