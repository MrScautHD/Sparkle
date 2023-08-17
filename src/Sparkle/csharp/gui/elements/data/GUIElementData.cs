using Raylib_cs;
using Sparkle.csharp.graphics.util;
using System.Numerics;

namespace Sparkle.csharp.gui.elements.data;
public class GUIElementData : IData<GUIElementData>
{
    public Vector2 Position;
    public Vector2 Size;
    public float Rotation;
    public Color DefaultColor;
    public Color Color;
    public Color HoverColor;
    public bool Enabled;
    public GUIElementData()
    {
        this.Position = Vector2.Zero;
        this.Size = Vector2.Zero;
        this.Rotation = 0;
        this.DefaultColor = Color.WHITE;
        this.Color = Color.WHITE;
        this.HoverColor = Color.GRAY;
        this.Enabled = true;
    }
    public GUIElementData Clone()
    {
        return null;
    }
}
