using Raylib_cs;

namespace Sparkle.CSharp.GUI.Elements.Data;

public struct ButtonData : IData {
    
    public Texture2D? Texture { get; set; }
    
    public float Rotation { get; set; }
    public Color Color { get; set; }
    public Color HoverColor { get; set; }
    
    /// <summary>
    /// Represents data for a button element.
    /// </summary>
    public ButtonData() {
        this.Rotation = 0;
        this.Color = Color.White;
        this.HoverColor = Color.Gray;
    }
}