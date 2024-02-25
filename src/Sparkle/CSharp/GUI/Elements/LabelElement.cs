using System.Numerics;
using Raylib_cs;
using Sparkle.CSharp.GUI.Elements.Data;
using Sparkle.CSharp.Rendering.Helpers;
using Sparkle.CSharp.Windowing;

namespace Sparkle.CSharp.GUI.Elements; 

public class LabelElement : GuiElement {
    
    public Font Font;
    public string Text;
    public float FontSize;
    public int Spacing;
    public float Rotation;
    public Color Color;
    public Color HoverColor;
    
    protected float CalcFontSize { get; private set; }

    /// <summary>
    /// Initializes a new label element with the specified parameters and associated label data.
    /// </summary>
    /// <param name="name">The name of the label element.</param>
    /// <param name="data">Label-specific data including font, rotation, colors, and text.</param>
    /// <param name="anchor">The anchor point for positioning the element.</param>
    /// <param name="offset">The offset for fine-tuning the position.</param>
    /// <param name="clickClickFunc">An optional function to handle click events.</param>
    public LabelElement(string name, LabelData data, Anchor anchor, Vector2 offset, Func<bool>? clickClickFunc = null) : base(name, anchor, offset, Vector2.Zero, clickClickFunc) {
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
        
        this.Size = FontHelper.MeasureText(this.Font, this.Text, this.FontSize, this.Spacing);
        this.ScaledSize = FontHelper.MeasureText(this.Font, this.Text, this.CalcFontSize, this.Spacing);
    }

    protected internal override void Draw() {
        if (this.Text != string.Empty) {
            this.DrawText();
        }
    }
    
    /// <summary>
    /// Draws the text of the button element.
    /// </summary>
    protected virtual void DrawText() {
        Vector2 pos = new Vector2(this.Position.X + (this.ScaledSize.X / 2), this.Position.Y + (this.ScaledSize.Y / 2));
        Vector2 origin = new Vector2(this.ScaledSize.X / 2, this.ScaledSize.Y / 2);
        FontHelper.DrawText(this.Font, this.Text, pos, origin, this.Rotation, this.CalcFontSize, this.Spacing, this.IsHovered ? this.HoverColor : this.Color);
    }
}