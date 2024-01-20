using Raylib_cs;

namespace Sparkle.CSharp.GUI.Elements.Data; 

public struct ToggleData {
    
    public Texture2D? Texture;
    public Texture2D? ToggledTexture;
    public float Rotation;
    public Color Color;
    public Color HoverColor;
    public Color ToggledColor;

    public string ToggledText;
    public Color ToggledTextColor;

    /// <summary>
    /// Initializes a new instance of the <see cref="ToggleData"/> with default settings.
    /// Sets the rotation to 0, assigns default colors for various states, and initializes an empty toggled text with a default color.
    /// </summary>
    public ToggleData() {
        this.Rotation = 0;
        this.Color = Color.White;
        this.HoverColor = Color.Gray;
        this.ToggledColor = Color.White;

        this.ToggledText = string.Empty;
        this.ToggledTextColor = Color.White;
    }
}