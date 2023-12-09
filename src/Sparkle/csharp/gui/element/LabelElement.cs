using System.Numerics;
using Raylib_cs;
using Sparkle.csharp.graphics.helper;
using Sparkle.csharp.gui.element.data;
using Sparkle.csharp.window;

namespace Sparkle.csharp.gui.element; 

public class LabelElement : GuiElement {
    
    public Font Font;
    public float Rotation;
    public Color Color;
    public Color HoverColor;
    
    public string Text;
    public float FontSize;
    public int Spacing;
    
    protected float CalcFontSize { get; private set; }

    /// <summary>
    /// Initializes a new label element with the specified parameters and associated label data.
    /// </summary>
    /// <param name="name">The name of the label element.</param>
    /// <param name="data">Label-specific data including font, rotation, colors, and text.</param>
    /// <param name="anchor">The anchor point for positioning the element.</param>
    /// <param name="offset">The offset for fine-tuning the position.</param>
    /// <param name="clickClickFunc">An optional function to handle click events.</param>
    public LabelElement(string name, LabelData data, Anchor anchor, Vector2 offset, Func<bool>? clickClickFunc = null) : base(name, anchor, offset, data.Size, clickClickFunc) {
        this.Font = data.Font;
        this.Rotation = data.Rotation;
        this.Color = data.Color;
        this.HoverColor = data.HoverColor;
        
        this.Text = data.Text;
        this.FontSize = data.FontSize;
        this.Spacing = data.Spacing;
    }
    
    protected override void CalculateSize() {
        float scale = Window.GetRenderHeight() / (float) Game.Instance.Settings.Height;
        this.CalcFontSize = this.FontSize * scale * GuiManager.Scale;
        
        this.Size = FontHelper.MeasureText(this.Font, this.Text, this.CalcFontSize, this.Spacing);
        this.ScaledSize = this.Size;
    }

    protected internal override void Draw() {
        if (this.Text == string.Empty) return;
        
        Vector2 textPos = new Vector2(this.Position.X + (this.ScaledSize.X / 2), this.Position.Y + (this.ScaledSize.Y / 2));
        Vector2 textOrigin = new Vector2(this.Position.X / 2, this.Position.Y / 2);
            
        FontHelper.DrawText(this.Font, this.Text, textPos, textOrigin, this.Rotation, this.CalcFontSize, this.Spacing, this.IsHovered ? this.HoverColor : this.Color);
    }
}