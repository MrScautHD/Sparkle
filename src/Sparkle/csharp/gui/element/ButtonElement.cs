using System.Numerics;
using Raylib_cs;
using Sparkle.csharp.graphics.helper;
using Sparkle.csharp.gui.element.data;
using Sparkle.csharp.window;

namespace Sparkle.csharp.gui.element; 

public class ButtonElement : GuiElement {
    
    public Texture2D? Texture;
    public float Rotation;
    public Color Color;
    public Color HoverColor;
    
    public Font Font;
    public float TextRotation;
    public Vector2 TextSize;
    public Color TextColor;
    public Color TextHoverColor;
    
    public string Text;
    public float FontSize;
    public int Spacing;
    
    protected float CalcFontSize { get; private set; }
    
    /// <summary>
    /// Initializes a new button element with the specified parameters and associated button and label data.
    /// </summary>
    /// <param name="name">The name of the button element.</param>
    /// <param name="buttonData">Button-specific data including texture, rotation, and colors.</param>
    /// <param name="labelData">Label-specific data for text display on the button.</param>
    /// <param name="anchor">The anchor point for positioning the element.</param>
    /// <param name="offset">An optional offset for fine-tuning the position.</param>
    /// <param name="size">An optional size for the button element; if not provided, it's determined by the texture.</param>
    /// <param name="clickClickFunc">An optional function to handle click events.</param>
    public ButtonElement(string name, ButtonData buttonData, LabelData labelData, Anchor anchor, Vector2 offset, Vector2? size, Func<bool>? clickClickFunc = null) : base(name, anchor, offset, Vector2.Zero, clickClickFunc) {
        this.Texture = buttonData.Texture;
        this.Size = size ?? (this.Texture != null ? new Vector2(this.Texture.Value.Width, this.Texture.Value.Height) : Vector2.Zero);
        this.Rotation = buttonData.Rotation;
        this.Color = buttonData.Color;
        this.HoverColor = buttonData.HoverColor;
        
        this.Font = labelData.Font;
        this.TextRotation = labelData.Rotation;
        this.TextColor = labelData.Color;
        this.TextHoverColor = labelData.HoverColor;
        
        this.Text = labelData.Text;
        this.FontSize = labelData.FontSize;
        this.Spacing = labelData.Spacing;
    }

    protected override void CalculateSize() {
        base.CalculateSize();
        
        float scale = Window.GetRenderHeight() / (float) Game.Instance.Settings.Height;
        this.CalcFontSize = this.FontSize * scale * GuiManager.Scale;
        
        this.TextSize = FontHelper.MeasureText(this.Font, this.Text, this.CalcFontSize, this.Spacing);
    }

    protected internal override void Draw() {
        if (this.Texture != null) {
            Rectangle source = new Rectangle(0, 0, this.Texture.Value.Width, this.Texture.Value.Height);
            Rectangle dest = new Rectangle(this.Position.X + (this.ScaledSize.X / 2), this.Position.Y + (this.ScaledSize.Y / 2), this.ScaledSize.X, this.ScaledSize.Y);
            Vector2 origin = new Vector2(dest.Width / 2, dest.Height / 2);
            TextureHelper.DrawPro(this.Texture.Value, source, dest, origin, this.Rotation, this.IsHovered ? this.HoverColor : this.Color);
        }
        else {
            Rectangle rec = new Rectangle(this.Position.X + (this.ScaledSize.X / 2), this.Position.Y + (this.ScaledSize.Y / 2), this.ScaledSize.X, this.ScaledSize.Y);
            Vector2 origin = new Vector2(rec.Width / 2, rec.Height / 2);
            ShapeHelper.DrawRectangle(rec, origin, this.Rotation, this.IsHovered ? this.HoverColor : this.Color);
        }

        if (this.Text != string.Empty) {
            Vector2 textPos = new Vector2(this.Position.X + this.ScaledSize.X / 2, this.Position.Y + this.ScaledSize.Y / 2);
            Vector2 textOrigin = new Vector2(this.TextSize.X / 2, this.TextSize.Y / 2);
            FontHelper.DrawText(this.Font, this.Text, textPos, textOrigin, this.TextRotation, this.CalcFontSize, this.Spacing, this.IsHovered ? this.TextHoverColor : this.TextColor);
        }
    }
}