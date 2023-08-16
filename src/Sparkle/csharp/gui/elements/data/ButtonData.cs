using Raylib_cs;

namespace Sparkle.csharp.gui.elements.data; 

public struct ButtonData {
    
    public Texture2D? Texture;
    public Color Color;
    public Color HoverColor;

    public ButtonData() {
        this.Color = Color.WHITE;
        this.HoverColor = Color.LIGHTGRAY;
    }
}