using Raylib_cs;

namespace Sparkle.csharp.gui.elements.data; 

#if !HEADLESS
public struct ToggleData {
    
    public Texture2D? Texture;
    public Texture2D? ToggledTexture;
    public float Rotation;
    public Color Color;
    public Color HoverColor;
    public Color ToggledColor;

    public string ToggledText;
    public Color ToggledTextColor;

    public ToggleData() {
        this.Rotation = 0;
        this.Color = Color.WHITE;
        this.HoverColor = Color.GRAY;
        this.ToggledColor = Color.WHITE;

        this.ToggledText = string.Empty;
        this.ToggledTextColor = Color.WHITE;
    }
}
#endif