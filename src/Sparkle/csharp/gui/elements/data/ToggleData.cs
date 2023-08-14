using Raylib_cs;
using Sparkle.csharp.graphics.util;

namespace Sparkle.csharp.gui.elements.data; 

public class ToggleData {
    
    public Texture2D Texture;
    public Texture2D ToggledTexture;
    public Color Color;
    public Color DefaultColor { get; private set; }

    public ToggleData() {
        this.Texture = TextureHelper.LoadFromImage(ImageHelper.GenColor(10, 10, Color.WHITE));
        this.ToggledTexture = TextureHelper.LoadFromImage(ImageHelper.GenColor(10, 10, Color.GRAY));
        this.Color = Color.WHITE;
        this.DefaultColor = this.Color;
    }
}