using Raylib_cs;
using Sparkle.CSharp.Rendering.Helpers;

namespace Sparkle.CSharp.GUI.Elements.Data;

public struct LabelData : IData {
    
    public Font Font { get; set; }
    public string Text { get; set; }
    public float FontSize { get; set; }
    public int Spacing { get; set; }
    
    public float Rotation { get; set; }
    public Color Color { get; set; }
    public Color HoverColor { get; set; }
    
    /// <summary>
    /// Represents data for a label element.
    /// </summary>
    public LabelData() {
        this.Font = FontHelper.GetDefault();
        this.Text = string.Empty;
        this.FontSize = 18;
        this.Spacing = 4;
        this.Rotation = 0;
        this.Color = Color.White;
        this.HoverColor = Color.Gray;
    }
}