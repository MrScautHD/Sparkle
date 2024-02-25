using Raylib_cs;

namespace Sparkle.CSharp.GUI.Elements.Data; 

public struct ToggleData : IData {
    
    public float Rotation { get; set; }
    public Color Color { get; set; }
    public Color HoverColor { get; set; }
    public Color ToggledColor { get; set; }
    public Color ToggledTextColor { get; set; }
    
    public Texture2D? Texture { get; set; }
    public Texture2D? ToggledTexture { get; set; }

    public string ToggledText { get; set; }
    
    /// <summary>
    /// Represents data for a toggle element.
    /// </summary>
    public ToggleData() {
        this.Rotation = 0;
        this.Color = Color.White;
        this.HoverColor = Color.Gray;
        this.ToggledColor = Color.White;
        this.ToggledTextColor = Color.White;
        this.ToggledText = string.Empty;
    }
}