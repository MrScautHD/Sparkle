using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Transformations;

namespace Sparkle.CSharp.GUI.Elements.Data;

public class RectangleButtonData {
    
    public Vector2 Scale;
    
    public Color Color;
    
    public Color HoverColor;

    // Outline... idk yet...

    public RectangleButtonData(Vector2? scale = null, Color? color = null, Color? hoverColor = null) {
        this.Scale = scale ?? Vector2.One;
        this.Color = color ?? Color.White;
        this.HoverColor = hoverColor ?? this.Color;
    }
}