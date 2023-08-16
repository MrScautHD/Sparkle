using Raylib_cs;
using Sparkle.csharp.graphics.util;

namespace Sparkle.csharp.gui.elements.data; 

public struct ButtonData {
    
    public Texture2D Texture;
    public Color Color;
    public Color HoverColor;

    public ButtonData() {
        this.Texture = TextureHelper.LoadFromImage(ImageHelper.GenColor(10, 10, Color.WHITE)); // Todo check if it get loaded even when you override it!
        this.Color = Color.WHITE;
        this.HoverColor = Color.LIGHTGRAY;
    }
}