using Raylib_cs;

namespace Sparkle.csharp.gui.elements.data; 

#if !HEADLESS
public struct ButtonData {
    
    public Texture2D? Texture;
    public float Rotation;
    public Color Color;
    public Color HoverColor;

    public ButtonData() {
        this.Rotation = 0;
        this.Color = Color.WHITE;
        this.HoverColor = Color.GRAY;
    }
}
#endif