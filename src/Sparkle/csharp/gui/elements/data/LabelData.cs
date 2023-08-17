using System.Numerics;
using Raylib_cs;
using Sparkle.csharp.graphics.util;

namespace Sparkle.csharp.gui.elements.data; 

public struct LabelData {
    
    public Font Font;
    public string Text;
    public float FontSize;
    public int Spacing;
    public float Rotation;
    public Vector2 Size;
    public Color Color;
    public Color HoverColor;
    
    public LabelData() {
        this.Font = FontHelper.GetDefault();
        this.Text = string.Empty;
        this.FontSize = 18;
        this.Spacing = 4;
        this.Rotation = 0;
        this.Size = Vector2.Zero;
        this.Color = Color.WHITE;
        this.HoverColor = Color.GRAY;
    }
}