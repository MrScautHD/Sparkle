using Raylib_cs;
using Sparkle.csharp.graphics.util;

namespace Sparkle.csharp.gui.elements.data; 

public class ButtonData {
    
    public Texture2D Texture;
    public Color DefaultColor;
    public Color Color;
    public Color HoverColor;

    public ButtonData() {
        this.Texture = TextureHelper.LoadFromImage(ImageHelper.GenColor(10, 10, Color.WHITE));
        this.DefaultColor = Color.WHITE;
        this.Color = Color.WHITE;
        this.HoverColor = Color.LIGHTGRAY;
    }
}