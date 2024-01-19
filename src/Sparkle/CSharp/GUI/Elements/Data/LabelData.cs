using System.Numerics;
using Raylib_cs;
using Sparkle.CSharp.Rendering.Helpers;

namespace Sparkle.CSharp.GUI.Elements.Data;

public struct LabelData {
    
    public Font Font;
    public string Text;
    public float FontSize;
    public int Spacing;
    public float Rotation;
    public Vector2 Size;
    public Color Color;
    public Color HoverColor;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="LabelData"/> with default settings. 
    /// Sets the default font, an empty text string, a font size of 18, a spacing of 4, 
    /// a rotation of 0, a size of (0,0), and default colors for normal and hover states.
    /// </summary>
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