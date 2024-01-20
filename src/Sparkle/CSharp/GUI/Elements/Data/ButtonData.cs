using Raylib_cs;

namespace Sparkle.CSharp.GUI.Elements.Data; 

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
        this.Color = Color.White;
        this.HoverColor = Color.Gray;
    }
}