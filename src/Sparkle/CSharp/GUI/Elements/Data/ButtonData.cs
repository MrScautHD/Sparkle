using Raylib_cs;

namespace Sparkle.CSharp.GUI.Elements.Data; 

public struct ButtonData : IData {
    
    public Texture2D? Texture { get; set; }
    
    public float Rotation { get; set; }
    public Color Color { get; set; }
    public Color HoverColor { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ButtonData"/>, setting default values for Rotation, Color, and HoverColor.
    /// </summary>
    public ButtonData() {
        this.Rotation = 0;
        this.Color = Color.White;
        this.HoverColor = Color.Gray;
    }
}