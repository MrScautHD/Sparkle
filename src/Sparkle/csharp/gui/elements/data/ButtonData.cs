using Raylib_cs;
using Sparkle.csharp.graphics.util;

namespace Sparkle.csharp.gui.elements.data; 

public struct ButtonData {
    
    public Texture2D Texture;
    public Color HoverColor;
    public Color PressColor;

    public ButtonData() {
        this.Texture = TextureHelper.LoadFromImage(ImageHelper.GenColor(10, 10, Color.WHITE));
        this.HoverColor = Color.LIGHTGRAY;
        this.PressColor = Color.GRAY;
    }
}