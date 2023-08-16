using Raylib_cs;
using Sparkle.csharp.graphics.util;

namespace Sparkle.csharp.gui.elements.data; 

public struct ToggleData {
    
    public Texture2D Texture;
    public Texture2D ToggledTexture;
    public Color Color;
    public Color HoverColor;
    public Color ToggledColor;

    public string ToggledText;
    public Color ToggledTextColor;

    public ToggleData() {
        this.Texture = TextureHelper.LoadFromImage(ImageHelper.GenColor(10, 10, Color.WHITE));
        this.ToggledTexture = TextureHelper.LoadFromImage(ImageHelper.GenColor(10, 10, Color.DARKGRAY));
        this.Color = Color.WHITE;
        this.HoverColor = Color.GRAY;
        this.ToggledColor = Color.WHITE;

        this.ToggledText = string.Empty;
        this.ToggledTextColor = Color.WHITE;
    }
}