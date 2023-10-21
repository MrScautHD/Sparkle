using System.Numerics;
using Raylib_cs;
using Sparkle.csharp.graphics.util;
using Sparkle.csharp.gui.element.data;
using Sparkle.csharp.window;

namespace Sparkle.csharp.gui.element; 

public class ToggleElement : GuiElement {
    
    public Texture2D? Texture;
    public Texture2D? ToggledTexture;
    public float Rotation;
    public Color Color;
    public Color HoverColor;
    public Color ToggledColor;
    
    public Font Font;
    public float TextRotation;
    public Vector2 TextSize;
    public Color TextColor;
    public Color TextHoverColor;
    public Color ToggledTextColor;
    
    public string Text;
    public string ToggledText;
    public float FontSize;
    public int Spacing;
    
    protected float CalcFontSize { get; private set; }
    
    public bool IsToggled { get; private set; }

    /// <summary>
    /// Initializes a new toggle element with the specified parameters and associated toggle and label data.
    /// </summary>
    /// <param name="name">The name of the toggle element.</param>
    /// <param name="toggleData">Toggle-specific data including textures, rotation, and colors.</param>
    /// <param name="labelData">Label-specific data for text display on the toggle.</param>
    /// <param name="anchor">The anchor point for positioning the element.</param>
    /// <param name="offset">The offset for fine-tuning the position.</param>
    /// <param name="size">An optional size for the toggle element; if not provided, it's determined by the texture.</param>
    /// <param name="clickClickFunc">An optional function to handle click events.</param>
    public ToggleElement(string name, ToggleData toggleData, LabelData labelData, Anchor anchor, Vector2 offset, Vector2? size, Func<bool>? clickClickFunc = null) : base(name, anchor, offset, Vector2.Zero, clickClickFunc) {
        this.Texture = toggleData.Texture;
        this.ToggledTexture = toggleData.ToggledTexture;
        this.Size = size ?? (this.Texture != null ? new Vector2(this.Texture.Value.width, this.Texture.Value.height) : Vector2.Zero);
        this.Rotation = toggleData.Rotation;
        this.Color = toggleData.Color;
        this.HoverColor = toggleData.HoverColor;
        this.ToggledColor = toggleData.ToggledColor;
        
        this.Font = labelData.Font;
        this.TextRotation = labelData.Rotation;
        this.TextColor = labelData.Color;
        this.TextHoverColor = labelData.HoverColor;
        this.ToggledTextColor = toggleData.ToggledTextColor;
        
        this.Text = labelData.Text;
        this.ToggledText = toggleData.ToggledText;
        this.FontSize = labelData.FontSize;
        this.Spacing = labelData.Spacing;
    }

    protected internal override void Update() {
        base.Update();
        
        if (this.IsClicked) {
            this.IsToggled = !this.IsToggled;
        }
    }

    protected override void CalculateSize() {
        base.CalculateSize();
        
        float scale = Window.GetRenderHeight() / (float) Game.Instance.Settings.Height;
        this.CalcFontSize = this.FontSize * scale * GuiManager.Scale;
        
        this.TextSize = FontHelper.MeasureText(this.Font, this.Text, this.CalcFontSize, this.Spacing);
    }

    protected internal override void Draw() {
        Texture2D? texture = this.IsToggled ? this.ToggledTexture : this.Texture;
        
        if (texture != null) {
            Rectangle source = new Rectangle(0, 0, texture.Value.width, texture.Value.height);
            Rectangle dest = new Rectangle(this.Position.X + (this.ScaledSize.X / 2), this.Position.Y + (this.ScaledSize.Y / 2), this.ScaledSize.X, this.ScaledSize.Y);
            Vector2 origin = new Vector2(dest.width / 2, dest.height / 2);
            Color color = this.IsHovered ? this.HoverColor : (this.IsToggled ? this.ToggledColor : this.Color);
            TextureHelper.DrawPro(texture.Value, source, dest, origin, this.Rotation, color);
        }
        else {
            Rectangle rec = new Rectangle(this.Position.X + (this.ScaledSize.X / 2), this.Position.Y + (this.ScaledSize.Y / 2), this.ScaledSize.X, this.ScaledSize.Y);
            Vector2 origin = new Vector2(rec.width / 2, rec.height / 2);
            Color color = this.IsHovered ? this.HoverColor : (this.IsToggled ? this.ToggledColor : this.Color);
            ShapeHelper.DrawRectangle(rec, origin, this.Rotation, color);
        }

        string text = this.IsToggled ? this.ToggledText : this.Text;
        if (text != string.Empty) {
            Vector2 textPos = new Vector2(this.Position.X + this.ScaledSize.X / 2, this.Position.Y + this.ScaledSize.Y / 2);
            Vector2 textOrigin = new Vector2(this.TextSize.X / 2, this.TextSize.Y / 2);
            Color textColor = this.IsHovered ? this.TextHoverColor : (this.IsToggled ? this.ToggledTextColor : this.TextColor);
            FontHelper.DrawText(this.Font, text, textPos, textOrigin, this.TextRotation, this.CalcFontSize, this.Spacing, textColor);
        }
    }
}